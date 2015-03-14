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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using NitroDebugger;
using NitroDebugger.RSP;

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

		[Test]
		public void StoreHostAndPort()
		{
			Assert.AreEqual(Host, this.client.Connection.Host);
			Assert.AreEqual(Port, this.client.Connection.Port);
		}

		[Test]
		public void Connect()
		{
			Assert.IsTrue(this.client.Connection.IsConnected);
		}

		[Test]
		public void NotConnectedOnBadPort()
		{
			GdbClient clientError = new GdbClient();
			clientError.Connection.Connect("localhost", 10102);
			Assert.IsFalse(clientError.Connection.IsConnected);
		}

		[Test]
		public void Disconnect()
		{
			this.client.Connection.Disconnect();
			Assert.IsFalse(this.client.Connection.IsConnected);
		}

		[Test]
		public void CannotConnectIfConnected()
		{
			Assert.DoesNotThrow(() => this.client.Connection.Connect("localhost", 10102));
		}

		[Test]
		public void CannnotDisconnectIfNotConnected()
		{
			this.client.Connection.Disconnect();
			Assert.DoesNotThrow(() => this.client.Connection.Disconnect());
		}

		public void SendPacket(string cmd, string args)
		{
			Mock<CommandPacket> packet = new Mock<CommandPacket>(cmd);
			packet.Protected().Setup<string>("PackArguments").Returns(args);
			byte[] packetBin = PacketBinConverter.ToBinary(packet.Object);

			this.serverStream.WriteByte(RawPacket.Ack);
			this.serverStream.Write(packetBin, 0, packetBin.Length);
		}

		[Test]
		public void CommandUnknownReplyPacket()
		{
			// Send as many times as the maximum counter of Presentation layer
			for (int i = 0; i < 10; i++)
				this.SendPacket("@", "");

			StopSignal reason = this.client.Execution.AskHaltedReason();
			Assert.IsFalse(this.client.Connection.IsConnected);
			Assert.AreEqual(StopSignal.Unknown, reason);
			Assert.AreEqual(ErrorCode.ProtocolError, this.client.Error);
		}

		[Test]
		public void CommandUnexepectedReply()
		{
			this.SendPacket("OK", "");
			StopSignal reason = this.client.Execution.AskHaltedReason();
			Assert.IsFalse(this.client.Connection.IsConnected);
			Assert.AreEqual(StopSignal.Unknown, reason);
			Assert.AreEqual(ErrorCode.ProtocolError, this.client.Error);
		}

		[Test]
		public void CommandConnectionLostRaiseHandle()
		{
			this.serverClient.Close();
			this.client.Connection.LostConnection += new LostConnectionEventHandle(LostConnection);
			this.client.Execution.AskHaltedReason();
		}

		private void LostConnection(object sender, EventArgs e)
		{
			this.client.Connection.LostConnection -= new LostConnectionEventHandle(LostConnection);
			Assert.IsFalse(this.client.Connection.IsConnected);
			Assert.AreEqual(ErrorCode.NetworkError, this.client.Error);
		}

		[Test]
		public void CommandConnectionLostDoesNotRaiseHandle()
		{
			this.serverClient.Close();
			Assert.DoesNotThrow(() => this.client.Execution.AskHaltedReason());
		}

		[Test]
		public void Interrupt()
		{
			this.SendPacket("S", "02");
			bool stopped = this.client.Execution.Stop();
			Assert.IsTrue(stopped);

			string rcv = this.Read();
			Assert.AreEqual("\x03", rcv.Substring(0, rcv.Length - 1));
		}

		[Test]
		public void InterruptInvalidSignal()
		{
			this.SendPacket("S", "03");
			bool stopped = this.client.Execution.Stop();
			Assert.IsFalse(stopped);
		}

		[Test]
		public void InterruptUnknownReplyPacket()
		{
			// Send as many times as the maximum counter of Presentation layer
			for (int i = 0; i < 10; i++)
				this.SendPacket("@", "");

			bool stopped = this.client.Execution.Stop();
			Assert.IsFalse(this.client.Connection.IsConnected);
			Assert.IsFalse(stopped);
			Assert.AreEqual(ErrorCode.ProtocolError, this.client.Error);
		}

		[Test]
		public void InterruptUnexepectedReply()
		{
			this.SendPacket("OK", "");
			bool stopped = this.client.Execution.Stop();
			Assert.IsFalse(this.client.Connection.IsConnected);
			Assert.IsFalse(stopped);
			Assert.AreEqual(ErrorCode.ProtocolError, this.client.Error);
		}

		[Test]
		public void InterruptConnectionLostRaiseHandle()
		{
			this.serverClient.Close();
			this.client.Connection.LostConnection += new LostConnectionEventHandle(LostConnection);
			bool stopped = this.client.Execution.Stop();
			Assert.IsFalse(stopped);
		}

		private string Read()
		{
			Thread.Sleep(50);
			byte[] buffer = new byte[10 * 1024];
			int read = this.serverStream.Read(buffer, 0, buffer.Length);
			return Encoding.ASCII.GetString(buffer, 0, read);
		}

		[Test]
		public void AskHaltedReason()
		{
			this.SendPacket("S", "02");
			StopSignal reason = this.client.Execution.AskHaltedReason();
			Assert.IsTrue(reason.HasFlag(StopSignal.HostBreak));

			string rcv = this.Read();
			Assert.AreEqual("?", rcv.Substring(1, rcv.Length - 5));
		}

		[Test]
		public void ContinueExecution()
		{
			this.serverStream.WriteByte(RawPacket.Ack);
			this.client.Execution.Continue();

			string rcv = this.Read();
			Assert.AreEqual("c", rcv.Substring(1, rcv.Length - 4));
		}

		[Test]
		public void ContinueExecutionAsync()
		{
			this.client.Execution.BreakExecution += new BreakExecutionEventHandle(BreakpointExecution);
			this.client.Execution.Continue();

			string rcv = this.Read();
			Assert.AreEqual("c", rcv.Substring(1, rcv.Length - 4));

			this.SendPacket("S", "05");
		}

		private void BreakpointExecution(object sender, StopSignal signal)
		{
			Assert.AreEqual(RawPacket.Ack, this.serverStream.ReadByte());
			Assert.AreEqual(0, this.serverClient.Available);

			Assert.IsTrue(signal.HasFlag(StopSignal.Breakpoint));
		}

		[Test]
		public void StepInto()
		{
			this.serverStream.WriteByte(RawPacket.Ack);
			this.client.Execution.StepInto();

			string rcv = this.Read();
			Assert.AreEqual("s", rcv.Substring(1, rcv.Length - 4));
		}

		[Test]
		public void StepIntoAsync()
		{
			this.client.Execution.BreakExecution += new BreakExecutionEventHandle(BreakpointExecution);
			this.client.Execution.StepInto();

			string rcv = this.Read();
			Assert.AreEqual("s", rcv.Substring(1, rcv.Length - 4));

			this.SendPacket("S", "05");
		}

		[Test]
		public void ContinueIsInterrupted()
		{
			this.client.Execution.BreakExecution += new BreakExecutionEventHandle(BreakpointExecution);

			this.client.Execution.Continue();
			string rcv = this.Read();
			Assert.AreEqual("c", rcv.Substring(1, rcv.Length - 4));
			this.serverStream.WriteByte(RawPacket.Ack);

			Task.Run(() => {
				while (this.serverStream.ReadByte() != 0x03) ;
				this.SendPacket("S", "02");
			});

			this.client.Execution.Stop();

			Assert.AreEqual(RawPacket.Ack, this.serverStream.ReadByte());
			Assert.AreEqual(0, this.serverClient.Available);
		}
	}
}

