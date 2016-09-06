//
// Session.cs
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

