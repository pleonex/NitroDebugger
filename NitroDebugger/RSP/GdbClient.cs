//
// GdbClient.cs
//
// Author:
//       Benito Palacios <benito356@gmail.com>
//
// Copyright (c) 2016 Benito Palacios Sánchez
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NitroDebugger.RSP.Packets;
using System.IO;

namespace NitroDebugger.RSP
{
	public delegate void BreakExecutionEventHandle(object sender, StopSignal signal);

	/// <summary>
	/// GDB Client. Direct connection to emulator.
	/// Application layer, only packets supported by DeSmuME implemented.
	/// </summary>
	public class GdbClient
	{
		private Task currentTask;

		public GdbClient()
		{
			this.Error = ErrorCode.NoError;
			this.Connection = new ConnectionManager(this);
			this.Execution = new ExecutionManager(this);
			this.Stream = new GdbStream(this);
			this.Registers = new RegisterManager(this);
            this.Breakpoints = new BreakpointManager(this);
		}

		public ConnectionManager Connection {
			get;
			private set;
		}

		public ExecutionManager Execution {
			get;
			private set;
		}

		public GdbStream Stream {
			get;
			private set;
		}

		public RegisterManager Registers {
			get;
			private set;
		}

        public BreakpointManager Breakpoints {
            get;
            private set;
        }

		public ErrorCode Error {
			get;
			private set;
		}

		internal Presentation Presentation {
			get;
			set;
		}

		internal void CancelPendingTask()
		{
			if (this.currentTask != null) {
				this.Presentation.CancelRead(this.currentTask);
				this.currentTask = null;
			}
		}

		internal T SendCommandWithoutErrorReply<T>(CommandPacket command)
			where T : ReplyPacket
		{
			ReplyPacket reply = this.SafeSending(command, typeof(T), typeof(ErrorReply));
			if (reply == null)
				return null;

			ErrorReply error = reply as ErrorReply;
			if (error != null) {
				this.Error = error.Error;
				return null;
			}

			return reply as T;
		}

		internal void SendCommandWithSignalAsync(CommandPacket cmd)
		{
			this.currentTask = Task.Run(() => {
				StopSignal signal = SendCommandWithSignal(cmd);
				this.Execution.OnBreakExecution(signal);
			});
		}

		internal StopSignal SendCommandWithSignal(CommandPacket cmd)
		{
			ReplyPacket response = this.SafeSending(cmd, typeof(StopSignalReply));
			if (response == null)
				return StopSignal.Unknown;

			return ((StopSignalReply)response).Signal;
		}

		internal ReplyPacket SafeInterruption()
		{
			this.Error = ErrorCode.NoError;
			ReplyPacket response = null;

			try {
				response = this.Presentation.SendInterrupt();
			} catch (SocketException) {
				this.Error = ErrorCode.NetworkError;
			} catch (ProtocolViolationException) {
				this.Error = ErrorCode.ProtocolError;
			}

			if (response != null && !(response is StopSignalReply))
				this.Error = ErrorCode.ProtocolError;

			if (this.Error != ErrorCode.NoError) {
				NetworkError();
				response = null;
			}

			return response;
		}

		private ReplyPacket SafeSending(CommandPacket command, params Type[] validReplyTypes)
		{
			this.Error = ErrorCode.NoError;
			ReplyPacket response = null;

			try {
				this.Presentation.SendCommand(command);

				if (validReplyTypes.Length > 0)
					response = this.Presentation.ReceiveReply();
			} catch (SocketException) {
				this.Error = ErrorCode.NetworkError;
			} catch (ProtocolViolationException) {
				this.Error = ErrorCode.ProtocolError;
			}

			if (response != null && !ValidateType(response, validReplyTypes))
				this.Error = ErrorCode.ProtocolError;

			if (this.Error != ErrorCode.NoError) {
				NetworkError();
				response = null;
			}

			return response;
		}

		private bool ValidateType(ReplyPacket reply, Type[] validReplyTypes)
		{
			return validReplyTypes.Contains(reply.GetType());
		}

		private void NetworkError()
		{
			this.Connection.Disconnect();
			this.Connection.OnLostConnection(EventArgs.Empty);
		}
	}
}

