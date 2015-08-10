//
//  ConnectionManager.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2015 Benito Palacios Sánchez
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Net;
using System.Net.Sockets;

namespace NitroDebugger.RSP
{
	public delegate void LostConnectionEventHandle(object sender, EventArgs e);

	public class ConnectionManager
	{
		internal ConnectionManager(GdbClient client)
		{
			this.Client = client;
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

		public GdbClient Client {
			get;
			private set;
		}

		public event LostConnectionEventHandle LostConnection;

		internal void OnLostConnection(EventArgs e)
		{
			this.IsConnected = false;

			if (LostConnection != null)
				LostConnection(this, e);
		}

		public void Connect(string host, int port)
		{
			if (this.IsConnected)
				return;

			try {
				this.Host = host;
				this.Port = port;
				this.Client.Presentation = new Presentation(this.Host, this.Port);
				this.IsConnected = true;
			} catch (SocketException ex) {
				this.IsConnected = false;
				Console.WriteLine("{ERROR} " + ex.Message);
			}
		}

		public void Disconnect()
		{
			if (!this.IsConnected)
				return;

			this.Client.Presentation.Close();
			this.IsConnected = false;
		}
	}
}
