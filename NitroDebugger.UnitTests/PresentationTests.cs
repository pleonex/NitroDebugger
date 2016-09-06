//
// PresentationTests.cs
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
