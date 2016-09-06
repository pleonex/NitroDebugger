//
// GdbTestingBase.cs
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
using System.Text;
using System.Threading;
using Moq;
using Moq.Protected;
using NitroDebugger.RSP;

namespace UnitTests
{
	public class GdbTestingBase
	{
		private const int PortBase = 10104;

		protected GdbTestingBase(int index)
		{
			Port = PortBase + index;
		}

		protected virtual void SetUp()
		{
			this.Server = new TcpListener(IPAddress.Loopback, Port);
			this.Server.Start();
			this.Client = new GdbClient();
		}

		protected virtual void Dispose()
		{
			this.Server.Stop();
		}

        protected virtual void OpenServer()
        {
            this.Client.Connection.Connect(Host, Port);
            this.ServerClient = this.Server.AcceptTcpClient();
            this.ServerStream = this.ServerClient.GetStream();
        }

		protected virtual void CloseServer()
		{
			this.Client.Connection.Disconnect();
			if (this.ServerClient.Connected)
				this.ServerClient.Close();

			while (this.Server.Pending())
				this.Server.AcceptTcpClient().Close();
		}

		protected void SendPacket(string cmd, string args)
		{
			Mock<CommandPacket> packet = new Mock<CommandPacket>(cmd);
			packet.Protected().Setup<string>("PackArguments").Returns(args);
			byte[] packetBin = PacketBinConverter.ToBinary(packet.Object);

			this.ServerStream.WriteByte(RawPacket.Ack);
			this.ServerStream.Write(packetBin, 0, packetBin.Length);
		}

		protected string Read()
		{
			Thread.Sleep(50);
			byte[] buffer = new byte[10 * 1024];
			int read = this.ServerStream.Read(buffer, 0, buffer.Length);
			return Encoding.ASCII.GetString(buffer, 0, read);
		}

		protected GdbClient Client {
			get;
			private set;
		}

		protected NetworkStream ServerStream {
			get;
			private set;
		}

		protected TcpClient ServerClient {
			get;
			private set;
		}

		protected TcpListener Server {
			get;
			private set;
		}

		protected static string Host {
			get { return "localhost"; }
		}

		protected int Port {
			get;
			private set;
		}
	}
}

