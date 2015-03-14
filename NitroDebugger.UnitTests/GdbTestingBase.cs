//
//  GdbTestingBase.cs
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

		protected virtual void Setup()
		{
			this.Server = new TcpListener(IPAddress.Loopback, Port);
			this.Server.Start();

			this.Client = new GdbClient();
			this.Client.Connection.Connect(Host, Port);

			this.ServerClient = this.Server.AcceptTcpClient();
			this.ServerStream = this.ServerClient.GetStream();
		}

		protected virtual void Dispose()
		{
			this.Client.Connection.Disconnect();
			if (this.ServerClient.Connected)
				this.ServerClient.Close();
			this.Server.Stop();
		}

		protected virtual void ResetServer()
		{
			this.Client.Connection.Disconnect();
			if (this.ServerClient.Connected)
				this.ServerClient.Close();

			while (this.Server.Pending())
				this.Server.AcceptTcpClient().Close();

			this.Client.Connection.Connect(Host, Port);
			this.ServerClient = this.Server.AcceptTcpClient();
			this.ServerStream = this.ServerClient.GetStream();
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

