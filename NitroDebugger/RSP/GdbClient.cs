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
	public delegate void LostConnectionEventHandle(object sender, EventArgs e);

	public delegate void BreakExecutionEventHandle(object sender, StopSignal signal);

	/// <summary>
	/// GDB Client. Direct connection to emulator.
	/// Application layer, only packets supported by DeSmuME implemented.
	/// </summary>
	public class GdbClient
	{
		private Presentation presentation;
		private Task currentTask;

		public GdbClient(string host, int port)
		{
			this.Host = host;
			this.Port = port;
			this.ErrorCode = 0;
		}

		public string Host {
			get;
			private set;
		}

		public int Port {
			get;
			private set;
		}

		public bool IsConnected {
			get;
			private set;
		}

		public int ErrorCode {
			get;
			private set;
		}

		public event LostConnectionEventHandle LostConnection;

		public event BreakExecutionEventHandle BreakExecution;

		private void OnLostConnection(EventArgs e)
		{
			this.IsConnected = false;

			if (LostConnection != null)
				LostConnection(this, e);
		}

		private void OnBreakExecution(StopSignal signal)
		{
			if (BreakExecution != null)
				BreakExecution(this, signal);
		}

		public void Connect()
		{
			if (this.IsConnected)
				return;

			try {
				this.presentation = new Presentation(this.Host, this.Port);
				this.IsConnected = true;
			} catch (SocketException) {
				this.IsConnected = false;
			}
		}

		public void Disconnect()
		{
			if (!this.IsConnected)
				return;

			this.presentation.Close();
			this.IsConnected = false;
		}

		public bool StopExecution()
		{
			// Stop any pending continue / step asyn task
			if (this.currentTask != null) {
				this.presentation.CancelRead(this.currentTask);
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

		private T SendCommandWithoutErrorReply<T>(CommandPacket command)
			where T : ReplyPacket
		{
			ReplyPacket reply = this.SafeSending(command, typeof(T), typeof(ErrorReply));
			if (reply == null)
				return null;

			ErrorReply error = reply as ErrorReply;
			if (error != null) {
				this.ErrorCode = error.Error;
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

		private void SendCommandWithSignalAsync(CommandPacket cmd)
		{
			this.currentTask = Task.Run(() => {
				StopSignal signal = SendCommandWithSignal(cmd);
				OnBreakExecution(signal);
			});
		}

		private StopSignal SendCommandWithSignal(CommandPacket cmd)
		{
			ReplyPacket response = this.SafeSending(cmd, typeof(StopSignalReply));
			if (response == null)
				return StopSignal.Unknown;

			return ((StopSignalReply)response).Signal;
		}

		private ReplyPacket SafeInterruption()
		{
			this.ErrorCode = 0;

			ReplyPacket response = null;
			bool error = false;
			try {
				response = this.presentation.SendInterrupt();
			} catch (SocketException) {
				error = true;
			} catch (ProtocolViolationException) {
				error = true;
			}

			if (response != null && !(response is StopSignalReply))
				error = true;

			if (error) {
				this.ErrorCode = 0xFF;
				NetworkError();
				response = null;
			}

			return response;
		}

		private ReplyPacket SafeSending(CommandPacket command, params Type[] validReplyTypes)
		{
			ReplyPacket response = null;
			bool error = false;

			try {
				this.presentation.SendCommand(command);

				if (validReplyTypes.Length > 0)
					response = this.presentation.ReceiveReply();
			} catch (SocketException) {
				error = true;
			} catch (ProtocolViolationException) {
				error = true;
			}

			if (response != null && !ValidateType(response, validReplyTypes))
				error = true;

			if (error) {
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
			this.Disconnect();
			OnLostConnection(EventArgs.Empty);
		}
	}
}

