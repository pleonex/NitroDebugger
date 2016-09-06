//
// ConnectionManager.cs
//
// Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
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
