//
//  PresentationTests.cs
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
using System.Text;
using System.Threading;
using NitroDebugger.RSP.Packets;

namespace UnitTests
{
	[TestFixture]
	public class PresentationTests
	{
		private TcpListener server;
		private TcpClient connection;
		private Presentation presentation;

		[TestFixtureSetUp]
		public void CreateServer()
		{
			server = new TcpListener(IPAddress.Loopback, 10103);
			server.Start();

			presentation = new Presentation("localhost", 10103);
			connection = server.AcceptTcpClient();
		}

		[TestFixtureTearDown]
		public void StopServer()
		{
			presentation.Close();
			connection.Close();
			server.Stop();
		}

		[TearDown]
		public void ResetServer()
		{
			presentation.Close();
			connection.Close();
			presentation = new Presentation("localhost", 10103);
			connection = server.AcceptTcpClient();

			while (server.Pending())
				server.AcceptTcpClient().Close();
		}

		private string Read()
		{
			Thread.Sleep(50);
			byte[] buffer = new byte[10 * 1024];
			int read = connection.GetStream().Read(buffer, 0, buffer.Length);
			return Encoding.ASCII.GetString(buffer, 0, read);
		}

		private void SendCommand(string command, string args, string crc, int times)
		{
			Mock<CommandPacket> packet = new Mock<CommandPacket>(command);
			packet.Protected().Setup<string>("PackArguments").Returns(args);

			string commandBin = "$" + command + args + "#" + crc;
			string expected = "";
			for (int i = 0; i < times; i++)
				expected += commandBin;

			for (int i = 0; i < times - 1; i++)
				connection.GetStream().WriteByte(RawPacket.Nack);
			connection.GetStream().WriteByte(RawPacket.Ack);

			this.presentation.SendCommand(packet.Object);

			string actual = Read();
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(0, connection.Available);
		}

		[Test]
		public void SendCommandOneAck()
		{
			SendCommand("test1", "arg1,arg2", "f4", 1);
		}

		[Test]
		public void SendCommandThreeAck()
		{
			SendCommand("test2", "arg1,arg2", "f5", 3);
		}

		[Test]
		public void SendCommandToManyNack()
		{
			Assert.Throws<ProtocolViolationException>(
				() => SendCommand("test2", "arg1,arg2", "f5", 11));
		}

		private void ReceiveMessageBad(int attempts)
		{
			//							       $     O     K     #     9     b
			byte[] responseBad = new byte[] { 0x24, 0x4F, 0x4B, 0x23, 0x39, 0x62 };

			//							        $     O     K     #     9     a
			byte[] responseGood = new byte[] { 0x24, 0x4F, 0x4B, 0x23, 0x39, 0x61 };

			for (int i = 0; i < attempts - 1; i++)
				connection.GetStream().Write(responseBad, 0, responseBad.Length);
			connection.GetStream().Write(responseGood, 0, responseGood.Length);

			ReplyPacket responsePacket = presentation.ReceiveReply();
			Assert.IsInstanceOf<OkReply>(responsePacket);

			for (int i = 0; i < attempts - 1; i++) {
				byte nack = (byte)connection.GetStream().ReadByte();
				Assert.AreEqual(RawPacket.Nack, nack);
			}

			byte ack = (byte)connection.GetStream().ReadByte();
			Assert.AreEqual(RawPacket.Ack, ack);

			Assert.AreEqual(0, connection.Available);
		}

		[Test]
		public void ReceiveMessage()
		{
			ReceiveMessageBad(1);
		}

		[Test]
		public void RequestResent()
		{
			ReceiveMessageBad(3);
		}

		[Test]
		public void ReceiveTooManyBadMessages()
		{
			Assert.Throws<ProtocolViolationException>(() => ReceiveMessageBad(11));
		}

		[Test]
		public void SendInterrupt()
		{
			//							    $     O     K     #     9     a
			byte[] response = new byte[] { 0x24, 0x4F, 0x4B, 0x23, 0x39, 0x61 };
			connection.GetStream().WriteByte(RawPacket.Ack);
			connection.GetStream().Write(response, 0, response.Length);

			ReplyPacket responsePacket = presentation.SendInterrupt();
			Assert.IsInstanceOf<OkReply>(responsePacket);

			byte expected = 0x03;
			byte actual = (byte)connection.GetStream().ReadByte();
			Assert.AreEqual(expected, actual);

			byte ack = (byte)connection.GetStream().ReadByte();
			Assert.AreEqual(RawPacket.Ack, ack);

			Assert.AreEqual(0, connection.Available);
		}
	}
}
