//
//  GdbClientTests.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2014 Benito Palacios Sánchez
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
using System.Net.Sockets;
using System.Net;
using Moq;
using Moq.Protected;
using NitroDebugger;
using System.Threading;

namespace UnitTests
{
	[TestFixture]
	public class GdbClientTests
	{
		private const string Host = "localhost";
		private const int Port = 10104;
		private GdbClient client;

		private TcpListener server;
		private TcpClient serverClient;
		private NetworkStream serverStream;

		[TestFixtureSetUp]
		public void Setup()
		{
			this.server = new TcpListener(IPAddress.Loopback, Port);
			this.server.Start();

			this.client = new GdbClient(Host, Port);
			this.client.Connect();

			this.serverClient = this.server.AcceptTcpClient();
			this.serverStream = this.serverClient.GetStream();
		}

		[TestFixtureTearDown]
		public void Dispose()
		{
			this.client.Disconnect();
			if (this.serverClient.Connected)
				this.serverClient.Close();
			this.server.Stop();
		}

		[TearDown]
		public void ResetServer()
		{
			this.client.Disconnect();
			if (this.serverClient.Connected)
				this.serverClient.Close();

			while (this.server.Pending())
				this.server.AcceptTcpClient().Close();

			this.client.Connect();
			this.serverClient = this.server.AcceptTcpClient();
			this.serverStream = this.serverClient.GetStream();
		}

		[Test]
		public void StoreHostAndPort()
		{
			Assert.AreEqual(Host, this.client.Host);
			Assert.AreEqual(Port, this.client.Port);
		}

		[Test]
		public void Connect()
		{
			Assert.IsTrue(this.client.IsConnected);
		}

		[Test]
		public void NotConnectedOnBadPort()
		{
			GdbClient clientError = new GdbClient("localhost", 10102);
			clientError.Connect();
			Assert.IsFalse(clientError.IsConnected);
		}

		[Test]
		public void Disconnect()
		{
			this.client.Disconnect();
			Assert.IsFalse(this.client.IsConnected);
		}

		[Test]
		public void CannotConnectIfConnected()
		{
			Assert.DoesNotThrow(() => this.client.Connect());
		}

		[Test]
		public void CannnotDisconnectIfNotConnected()
		{
			this.client.Disconnect();
			Assert.DoesNotThrow(() => this.client.Disconnect());
		}

		public void SendPacket(string cmd, string args)
		{
			Mock<CommandPacket> packet = new Mock<CommandPacket>(cmd);
			packet.Protected().Setup<string>("PackArguments").Returns(args);
			byte[] packetBin = PacketBinConverter.ToBinary(packet.Object);

			this.serverStream.WriteByte(RawPacket.Ack);
			this.serverStream.Write(packetBin, 0, packetBin.Length);
		}

		private void Read()
		{
			Thread.Sleep(50);
			byte[] buffer = new byte[10 * 1024];
			this.serverStream.Read(buffer, 0, buffer.Length);
		}

		[Test]
		public void AskHaltedReason()
		{
			this.SendPacket("S", "02");
			StopSignal reason = this.client.AskHaltedReason();
			this.Read();

			Assert.IsTrue(reason.HasFlag(StopSignal.HostBreak));
		}

		[Test]
		public void CommandError()
		{
			for (int i = 0; i < 10; i++)
				this.SendPacket("@", "");

			StopSignal reason = this.client.AskHaltedReason();
			this.Read();

			Assert.IsFalse(this.client.IsConnected);
			Assert.AreEqual(StopSignal.Unknown, reason);
		}

		[Test]
		public void CommandInvalidReply()
		{
			this.SendPacket("OK", "");
			StopSignal reason = this.client.AskHaltedReason();
			this.Read();

			Assert.IsFalse(this.client.IsConnected);
			Assert.AreEqual(StopSignal.Unknown, reason);
		}

		[Test]
		public void ConnectionClosedRaiseHandle()
		{
			this.serverClient.Close();
			this.client.LostConnection += new LostConnectionEventHandle(LostConnection);
			this.client.AskHaltedReason();
		}

		private void LostConnection(object sender, EventArgs e)
		{
			this.client.LostConnection -= new LostConnectionEventHandle(LostConnection);
			Assert.IsTrue(!this.client.IsConnected);
		}

		[Test]
		public void ConnectionClosedDoesNotRaiseHandle()
		{
			this.serverClient.Close();
			Assert.DoesNotThrow(() => this.client.AskHaltedReason());
		}

		[Test]
		public void Interrupt()
		{
			this.SendPacket("S", "02");
			bool stopped = this.client.StopExecution();
			this.Read();

			Assert.IsTrue(stopped);
		}

		[Test]
		public void InterruptError()
		{
			for (int i = 0; i < 10; i++)
				this.SendPacket("@", "");
			bool stopped = this.client.StopExecution();
			this.Read();

			Assert.IsFalse(this.client.IsConnected);
			Assert.IsFalse(stopped);
		}

		[Test]
		public void InterruptInvalidReply()
		{
			this.SendPacket("OK", "");
			bool stopped = this.client.StopExecution();
			this.Read();

			Assert.IsFalse(this.client.IsConnected);
			Assert.IsFalse(stopped);
		}

		[Test]
		public void InterruptConnectionLost()
		{
			this.serverClient.Close();
			this.client.LostConnection += new LostConnectionEventHandle(LostConnection);
			bool stopped = this.client.StopExecution();
			Assert.IsFalse(stopped);
		}
	}
}

