//
// Test.cs
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
using NUnit.Framework;
using System;
using NitroDebugger.RSP;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters;

namespace UnitTests
{
	[TestFixture]
	public class SessionTests
	{
		private const int DefaultPort = 10101;
		private const int BadPort = 10102;

		private TcpListener server;

        [OneTimeSetUp]
		public void CreateServer()
		{
			server = new TcpListener(IPAddress.Loopback, DefaultPort);
			server.Start();
		}

        [OneTimeTearDown]
		public void StopServer()
		{
			server.Stop();
		}

		[TearDown]
		public void ResetServer()
		{
			while (server.Pending())
				server.AcceptTcpClient().Close();
		}

		[Test]
		public void ConnectLocalhostIpDefaultPort()
		{
			Assert.DoesNotThrow(() => new Session("127.0.0.1", DefaultPort));
		}

		[Test]
		public void ConnectLocalhostDefaultPort()
		{
			Assert.DoesNotThrow(() => new Session("localhost", DefaultPort));
		}

		[Test]
		public void ConnectBadPort()
		{
			Assert.Throws<SocketException>(() => new Session("localhost", BadPort));
		}

		[Test]
		public void ConnectCloseLocalhostDefaultPort()
		{
			Assert.DoesNotThrow(() => new Session("localhost", DefaultPort).Close());
		}

		private void AcceptAndSendBytes(byte[] data)
		{
			TcpClient client = server.AcceptTcpClient();
			client.GetStream().Write(data, 0, data.Length);
			client.Close();
		}

		[Test]
		public void CanSendByte()
		{
			byte expected = 0xCA;
			Session session = new Session("localhost", DefaultPort);
			session.Write(expected);

			TcpClient client = server.AcceptTcpClient();
			int actual = client.GetStream().ReadByte();

			Assert.AreEqual(expected, actual);

			client.Close();
			session.Close();
		}

		[Test]
		public void CanSendBytes()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE };
			Session session = new Session("localhost", DefaultPort);
			session.Write(expected);

			TcpClient client = server.AcceptTcpClient();
			byte[] actual = new byte[2];
			client.GetStream().Read(actual, 0, 2);

			Assert.AreEqual(expected, actual);

			client.Close();
			session.Close();
		}

		[Test]
		public void CanReceiveByte()
		{
			byte expected = 0x03;

			Session session = new Session("localhost", DefaultPort);
			AcceptAndSendBytes(new byte[] { expected });

			byte actual = session.ReadByte();
			session.Close();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ReceiveUntilByteIsFirstToo()
		{
			Session session = new Session("localhost", DefaultPort);
			TcpClient client = server.AcceptTcpClient();

			byte sepByte = 0xCA;
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE };
			byte[] packet = new byte[] { 0xCA, 0xFE, 0xBE, 0xCA, 0xFE };
			client.GetStream().Write(packet, 0, packet.Length);

			byte[] actual = session.ReadPacket(sepByte);
			Assert.AreEqual(expected, actual);

			client.Close();
			session.Close();
		}

		[Test]
		public void ReceiveUntilByteNoRepeated()
		{
			Session session = new Session("localhost", DefaultPort);
			TcpClient client = server.AcceptTcpClient();

			byte sepByte = 0xCA;
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE };
			client.GetStream().Write(expected, 0, expected.Length);

			byte[] actual = session.ReadPacket(sepByte);
			Assert.AreEqual(expected, actual);

			client.Close();
			session.Close();
		}

		[Test]
		public void ThrowsExceptionTryingToReadByteOnClosed()
		{
			Session session = new Session("localhost", DefaultPort);
			server.AcceptTcpClient().Close();
			Assert.Throws<SocketException>(() => session.ReadByte());
		}

		[Test]
		public void ThrowsExceptionTryingToReadPacketOnClosed()
		{
			Session session = new Session("localhost", DefaultPort);
			server.AcceptTcpClient().Close();
			Assert.Throws<SocketException>(() => session.ReadPacket(0xCA));
		}

		[Test]
		public void ThrowsExceptionTryingToWriteByteOnClosed()
		{
			Session session = new Session("localhost", DefaultPort);
			server.AcceptTcpClient().Close();
			Assert.Throws<SocketException>(() => session.Write(0xCA));
		}

		[Test]
		public void ThrowsExceptionTryingToWriteBytesOnClosed()
		{
			Session session = new Session("localhost", DefaultPort);
			server.AcceptTcpClient().Close();
			Assert.Throws<SocketException>(() => session.Write(new byte[] { 0xCA }));
		}
	}
}

