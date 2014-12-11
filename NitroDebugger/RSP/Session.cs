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
using System.Threading;
using System.Threading.Tasks;

namespace NitroDebugger.RSP
{
	/// <summary>
	/// Represents the session layer.
	/// </summary>
	public class Session
	{
		private TcpClient client;
		private NetworkStream stream;
		private Queue<byte> buffer;

		public Session(string hostname, int port)
		{
			this.client = new TcpClient(hostname, port);
			this.stream = this.client.GetStream();
			this.buffer = new Queue<byte>();
		}

		public bool IsConnected {
			get {
				return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Available == 0);
			}
		}

		public CancellationToken Cancellation {
			get;
			set;
		}

		public void Close()
		{
			if (this.IsConnected)
				this.client.Close();
		}

		public void Write(byte data)
		{
			if (!this.IsConnected)
				throw new SocketException();

			this.stream.WriteByte(data);
		}

		public void Write(byte[] data)
		{
			if (!this.IsConnected)
				throw new SocketException();

			this.stream.Write(data, 0, data.Length);
		}
			
		public byte ReadByte()
		{
			if (!this.IsConnected)
				throw new SocketException();

			while (this.buffer.Count == 0) {
				this.Cancellation.ThrowIfCancellationRequested();
				this.UpdateBuffer();
			}

			return this.buffer.Dequeue();
		}

		public byte[] ReadPacket(byte separator)
		{
			if (!this.IsConnected)
				throw new SocketException();

			do {
				this.Cancellation.ThrowIfCancellationRequested();
				this.UpdateBuffer();
			} while (this.buffer.Count == 0);

			return this.GetPacket(separator);
		}

		private byte[] GetPacket(byte separator)
		{
			List<byte> packet = new List<byte>(5);
			packet.Add(this.buffer.Dequeue());

			while (this.buffer.Count > 0 && this.buffer.Peek() != separator)
				packet.Add(this.buffer.Dequeue());

			return packet.ToArray();
		}

		private void UpdateBuffer()
		{
			byte[] buffer = new byte[1024];
			while (this.stream.DataAvailable) {
				int read = this.stream.Read(buffer, 0, buffer.Length);;
				for (int i = 0; i < read; i++)
					this.buffer.Enqueue(buffer[i]);
			}
		}
	}
}

