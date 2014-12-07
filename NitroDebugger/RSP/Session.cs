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
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace NitroDebugger.RSP
{
	/// <summary>
	/// Represents the session layer.
	/// </summary>
	public class Session
	{
		private TcpClient client;
		private NetworkStream stream;

		public Session(string hostname, int port)
		{
			this.client = new TcpClient(hostname, port);
			this.stream = this.client.GetStream();
		}

		public bool DataAvailable {
			get { return this.stream.DataAvailable; }
		}

		public void Close()
		{
			this.client.Close();
		}

		public void Write(byte data)
		{
			this.stream.WriteByte(data);
		}

		public void Write(byte[] data)
		{
			this.stream.Write(data, 0, data.Length);
		}

		public byte ReadByte()
		{
			int result = this.stream.ReadByte();
			if (result == -1)
				throw new EndOfStreamException("No more data to read!");

			return (byte)result;
		}

		public byte[] ReadBytes()
		{
			List<byte> received = new List<byte>();

			byte[] buffer = new byte[1024];
			while (this.DataAvailable) {
				int read = this.stream.Read(buffer, 0, buffer.Length);
				Array.Resize<byte>(ref buffer, read);
				received.AddRange(buffer);
			}

			return received.ToArray();
		}
	}
}

