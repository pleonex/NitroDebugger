//
// GdbStream.cs
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
using System.IO;
using NitroDebugger.RSP.Packets;

namespace NitroDebugger.RSP
{
	public class GdbStream : Stream
	{
		private const uint MaxAddress = 0x0A012D00;
		private long position;

		internal GdbStream(GdbClient client)
		{
			Client = client;
			Position = 0;
		}

		public GdbClient Client {
			get;
			private set;
		}

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override long Length {
			get { return MaxAddress; }
		}

		public override long Position {
			get { return position; }
			set { position = value; }
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			// We don't use intermediate buffers.
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin)
				Position = offset;
			else if (origin == SeekOrigin.Current)
				Position += offset;
			else if (origin == SeekOrigin.End)
				Position = Length - offset;

			return Position;
		}

		public byte[] Read(uint address, int size)
		{
			// Seek & Read
			Seek(address, SeekOrigin.Begin);

			byte[] data = SendRead(address, size);
			Seek(data.Length, SeekOrigin.Current);

			return data;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			byte[] data = SendRead((uint)Position, count);
			Seek(data.Length, SeekOrigin.Current);

			Array.Copy(data, 0, buffer, offset, data.Length);
			return data.Length;
		}

		private byte[] SendRead(uint address, int size)
		{
			ReadMemoryCommand command = new ReadMemoryCommand(address, size);
			DataReply reply = this.Client.SendCommandWithoutErrorReply<DataReply>(command);
			if (reply == null)
				return new byte[0];

			return reply.GetData();
		}

		public void Write(uint address, int size, byte[] buffer)
		{
			// Seek & Write
			Seek(address, SeekOrigin.Begin);

			SendWrite(address, size, buffer);
			Seek(size, SeekOrigin.Current);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			byte[] data = new byte[count];
			Array.Copy(buffer, offset, data, 0, count);

			SendWrite((uint)Position, count, data);
			Seek(count, SeekOrigin.Current);
		}

		private void SendWrite(uint address, int size, byte[] data)
		{
			WriteMemoryCommand command = new WriteMemoryCommand(address, size, data);
			this.Client.SendCommandWithoutErrorReply<OkReply>(command);
		}
	}
}

