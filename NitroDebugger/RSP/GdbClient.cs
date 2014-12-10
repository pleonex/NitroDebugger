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
using System.Net.Sockets;
using NitroDebugger.RSP.Packets;

namespace NitroDebugger.RSP
{
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
			HaltedReasonCommand packet = new HaltedReasonCommand();
			this.presentation.SendCommand(packet);

			ReplyPacket response = this.presentation.ReceiveReply();
			throw new NotImplementedException();
		}

		private StopSignal ParseStopResponse(string response)
		{
			if (response.Length == 3 && response[0] == 'S') {
				int signal = Convert.ToInt32(response.Substring(1, 2));
				return ((TargetSignals)signal).ToStopSignal();
			} else {
				throw new FormatException("Unexpected response");
			}
		}
	}
}

