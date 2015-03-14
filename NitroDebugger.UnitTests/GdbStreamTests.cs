//
//  GdbStreamTests.cs
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
using NUnit.Framework;
using NitroDebugger.RSP;
using System.Net;
using System.Net.Sockets;
using Moq;
using Moq.Protected;
using System.Threading;
using System.Text;
using NitroDebugger;

namespace UnitTests
{
	[TestFixture]
	public class GdbStreamTests
	{
		private const string Host = "localhost";
		private const int Port = 10105;
		private GdbClient client;

		private TcpListener server;
		private TcpClient serverClient;
		private NetworkStream serverStream;

		[TestFixtureSetUp]
		public void Setup()
		{
			this.server = new TcpListener(IPAddress.Loopback, Port);
			this.server.Start();

			this.client = new GdbClient();
			this.client.Connection.Connect(Host, Port);

			this.serverClient = this.server.AcceptTcpClient();
			this.serverStream = this.serverClient.GetStream();
		}

		[TestFixtureTearDown]
		public void Dispose()
		{
			this.client.Connection.Disconnect();
			if (this.serverClient.Connected)
				this.serverClient.Close();
			this.server.Stop();
		}

		[TearDown]
		public void ResetServer()
		{
			this.client.Connection.Disconnect();
			if (this.serverClient.Connected)
				this.serverClient.Close();

			while (this.server.Pending())
				this.server.AcceptTcpClient().Close();

			this.client.Connection.Connect(Host, Port);
			this.serverClient = this.server.AcceptTcpClient();
			this.serverStream = this.serverClient.GetStream();
		}

		public void SendPacket(string cmd, string args)
		{
			Mock<CommandPacket> packet = new Mock<CommandPacket>(cmd);
			packet.Protected().Setup<string>("PackArguments").Returns(args);
			byte[] packetBin = PacketBinConverter.ToBinary(packet.Object);

			this.serverStream.WriteByte(RawPacket.Ack);
			this.serverStream.Write(packetBin, 0, packetBin.Length);
		}

		private string Read()
		{
			Thread.Sleep(50);
			byte[] buffer = new byte[10 * 1024];
			int read = this.serverStream.Read(buffer, 0, buffer.Length);
			return Encoding.ASCII.GetString(buffer, 0, read);
		}

		[Test]
		public void SeekBegin()
		{
			uint offset = 0x08342142;
			Assert.AreEqual(offset, this.client.Stream.Seek(offset, System.IO.SeekOrigin.Begin));
			Assert.AreEqual(offset, this.client.Stream.Position);
		}

		[Test]
		public void SeekCurrent()
		{
			uint start = 0x10;
			uint offset = 0x02;
			uint expected = 0x12;
			this.client.Stream.Seek(start, System.IO.SeekOrigin.Begin);

			Assert.AreEqual(expected, this.client.Stream.Seek(offset, System.IO.SeekOrigin.Current));
			Assert.AreEqual(expected, this.client.Stream.Position);
		}

		[Test]
		public void ReadMemoryGood()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0x00, 0x10, 0x20, 0x39 };
			uint address = 0x02000800;
			int size = 8;

			this.SendPacket("", BitConverter.ToString(expected).Replace("-", ""));
			byte[] actual = this.client.Stream.Read(address, size);
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(ErrorCode.NoError, this.client.Error);

			string rcv = this.Read();
			Assert.AreEqual("m2000800,8", rcv.Substring(1, rcv.Length - 5));
		}

		[Test]
		public void ReadMemoryErrorReading()
		{
			byte[] expected = new byte[0];
			uint address = 0x02000800;
			int size = 8;

			this.SendPacket("E", "03");
			byte[] actual = this.client.Stream.Read(address, size);
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(ErrorCode.ReadMemoryError, this.client.Error);
		}

		[Test]
		public void WriteMemoryGood()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0x00, 0x10, 0x20, 0x39 };
			uint address = 0x02000800;
			int size = 8;

			this.SendPacket("OK", "");
			this.client.Stream.Write(address, size, expected);
			Assert.AreEqual(ErrorCode.NoError, this.client.Error);

			string rcv = this.Read();
			string dataString = BitConverter.ToString(expected).Replace("-", "");
			Assert.AreEqual("M2000800,8:" + dataString, rcv.Substring(1, rcv.Length - 5));
		}
	}
}

