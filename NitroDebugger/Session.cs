//
//  Session.cs
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
using System.Text;

namespace NitroDebugger
{
	/// <summary>
	/// Represents the presentation layer.
	/// </summary>
	public class Session
	{
		private const int MaxWriteAttemps = 10;

		private TcpClient client;
		private NetworkStream stream;
		private StringBuilder buffer;

		public Session(string hostname, int port)
		{
			this.client = new TcpClient(hostname, port);
			this.stream = this.client.GetStream();
			this.buffer = new StringBuilder();
		}

		public void Close()
		{
			this.client.Close();
		}

		public void Write(string message)
		{
			int count = 0;
			do {
				if (count == MaxWriteAttemps)
					throw new Exception("Can not send packet successfully");
				count++;

				Packet packet = new Packet(message);
				byte[] data = packet.GetBinary();
				this.stream.Write(data, 0, data.Length);
			} while (this.stream.ReadByte() != Packet.Ack);
		}

		public string Read()
		{
			byte[] data = new byte[1024];
			while (this.stream.DataAvailable) {
				int read = this.stream.Read(data, 0, data.Length);
				buffer.AppendFormat("{0}", Encoding.ASCII.GetString(data, 0, read));
			}

			return Packet.FromBinary(buffer).Command;
		}
	}
}

