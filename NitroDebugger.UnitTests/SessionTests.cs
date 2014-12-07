//
//  Test.cs
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

		[TestFixtureSetUp]
		public void CreateServer()
		{
			server = new TcpListener(IPAddress.Loopback, DefaultPort);
			server.Start();
		}

		[TestFixtureTearDown]
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
			byte expected = 0xCA;

			Session session = new Session("localhost", DefaultPort);
			AcceptAndSendBytes(new byte[] { expected });

			byte actual = session.ReadByte();
			session.Close();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ReceiveEndOfStream()
		{
			Session session = new Session("localhost", DefaultPort);

			TcpClient client = server.AcceptTcpClient();
			client.Close();

			Assert.Throws<EndOfStreamException>(() => session.ReadByte());

			session.Close();
		}

		[Test]
		public void CanReceiveBytes()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE };
		
			Session session = new Session("localhost", DefaultPort);
			AcceptAndSendBytes(expected);

			byte[] actual = session.ReadBytes();
			session.Close();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void HasDataAvailable()
		{
			Session session = new Session("localhost", DefaultPort);
			AcceptAndSendBytes(new byte[] { 0xCA, 0xFE });
			session.ReadByte();

			Assert.IsTrue(session.DataAvailable);
		}

		[Test]
		public void HasNoDataAvailable()
		{
			Session session = new Session("localhost", DefaultPort);
			AcceptAndSendBytes(new byte[] { 0xCA, 0xFE });
			session.ReadByte();
			session.ReadByte();

			Assert.IsFalse(session.DataAvailable);
		}
	}
}

