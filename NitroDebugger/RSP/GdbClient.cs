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
using System.Net.Sockets;
using NitroDebugger.RSP.Packets;
using System.Net;
using NitroDebugger;

namespace NitroDebugger.RSP
{
	public delegate void LostConnectionEventHandle(object sender, EventArgs e);

	/// <summary>
	/// GDB Client. Direct connection to emulator.
	/// Application layer, only packets supported by DeSmuME implemented.
	/// </summary>
	public class GdbClient
	{
		private Presentation presentation;

		public GdbClient(string host, int port)
		{
			this.Host = host;
			this.Port = port;
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

		public event LostConnectionEventHandle LostConnection;

		private void OnLostConnection(EventArgs e)
		{
			this.IsConnected = false;

			if (LostConnection != null)
				LostConnection(this, e);
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

		public StopSignal AskHaltedReason()
		{
			HaltedReasonCommand command = new HaltedReasonCommand();
			ReplyPacket response = this.SafeSending(command, typeof(StopSignalReply));
			if (response == null)
				return StopSignal.Unknown;

			return ((StopSignalReply)response).Signal;
		}

		public bool StopExecution()
		{
			ReplyPacket response = this.SafeInterruption();
			if (response == null)
				return false;

			return ((StopSignalReply)response).Signal.HasFlag(StopSignal.HostBreak);
		}

		private ReplyPacket SafeInterruption()
		{
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

