//
//  GdbClient.cs
//
//  Author:
//       Benito Palacios <benito356@gmail.com>
//
//  Copyright (c) 2014 Benito Palacios
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NitroDebugger;
using NitroDebugger.RSP.Packets;

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
		}

		public ConnectionManager Connection {
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

		public event BreakExecutionEventHandle BreakExecution;

		private void OnBreakExecution(StopSignal signal)
		{
			if (BreakExecution != null)
				BreakExecution(this, signal);
		}

		public bool StopExecution()
		{
			// Stop any pending continue / step asyn task
			if (this.currentTask != null) {
				this.Presentation.CancelRead(this.currentTask);
				this.currentTask = null;
			}

			ReplyPacket response = this.SafeInterruption();
			if (response == null)
				return false;

			return ((StopSignalReply)response).Signal.HasFlag(StopSignal.HostBreak);
		}

		public StopSignal AskHaltedReason()
		{
			HaltedReasonCommand command = new HaltedReasonCommand();
			return this.SendCommandWithSignal(command);
		}

		public byte[] ReadMemory(uint address, int size)
		{
			ReadMemoryCommand command = new ReadMemoryCommand(address, size);
			DataReply reply = this.SendCommandWithoutErrorReply<DataReply>(command);
			if (reply == null)
				return new byte[0];

			return reply.GetData();
		}

		public void WriteMemory(uint address, int size, byte[] data)
		{
			WriteMemoryCommand command = new WriteMemoryCommand(address, size, data);
			this.SendCommandWithoutErrorReply<OkReply>(command);
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

		public void ContinueExecution()
		{
			ContinueCommand cont = new ContinueCommand();
			this.SendCommandWithSignalAsync(cont);
		}

		public void StepInto()
		{
			SingleStep step = new SingleStep();
			this.SendCommandWithSignalAsync(step);
		}

		internal void SendCommandWithSignalAsync(CommandPacket cmd)
		{
			this.currentTask = Task.Run(() => {
				StopSignal signal = SendCommandWithSignal(cmd);
				OnBreakExecution(signal);
			});
		}

		internal StopSignal SendCommandWithSignal(CommandPacket cmd)
		{
			ReplyPacket response = this.SafeSending(cmd, typeof(StopSignalReply));
			if (response == null)
				return StopSignal.Unknown;

			return ((StopSignalReply)response).Signal;
		}

		private ReplyPacket SafeInterruption()
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

