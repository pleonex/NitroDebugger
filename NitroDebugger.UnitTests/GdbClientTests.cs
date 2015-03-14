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
	public class GdbClientTests : GdbTestingBase
	{
		public GdbClientTests() : base(0)
		{
		}

		[TestFixtureSetUp]
		protected override void Setup()
		{
			base.Setup();
		}

		[TestFixtureTearDown]
		protected override void Dispose()
		{
			base.Dispose();
		}

		[TearDown]
		protected override void ResetServer()
		{
			base.ResetServer();
		}

		[Test]
		public void StoreHostAndPort()
		{
			Assert.AreEqual(Host, this.Client.Connection.Host);
			Assert.AreEqual(Port, this.Client.Connection.Port);
		}

		[Test]
		public void Connect()
		{
			Assert.IsTrue(this.Client.Connection.IsConnected);
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
			this.Client.Connection.Disconnect();
			Assert.IsFalse(this.Client.Connection.IsConnected);
		}

		[Test]
		public void CannotConnectIfConnected()
		{
			Assert.DoesNotThrow(() => this.Client.Connection.Connect("localhost", 10102));
		}

		[Test]
		public void CannnotDisconnectIfNotConnected()
		{
			this.Client.Connection.Disconnect();
			Assert.DoesNotThrow(() => this.Client.Connection.Disconnect());
		}

		[Test]
		public void CommandUnknownReplyPacket()
		{
			// Send as many times as the maximum counter of Presentation layer
			for (int i = 0; i < 10; i++)
				this.SendPacket("@", "");

			StopSignal reason = this.Client.Execution.AskHaltedReason();
			Assert.IsFalse(this.Client.Connection.IsConnected);
			Assert.AreEqual(StopSignal.Unknown, reason);
			Assert.AreEqual(ErrorCode.ProtocolError, this.Client.Error);
		}

		[Test]
		public void CommandUnexepectedReply()
		{
			this.SendPacket("OK", "");
			StopSignal reason = this.Client.Execution.AskHaltedReason();
			Assert.IsFalse(this.Client.Connection.IsConnected);
			Assert.AreEqual(StopSignal.Unknown, reason);
			Assert.AreEqual(ErrorCode.ProtocolError, this.Client.Error);
		}

		[Test]
		public void CommandConnectionLostRaiseHandle()
		{
			this.ServerClient.Close();
			this.Client.Connection.LostConnection += new LostConnectionEventHandle(LostConnection);
			this.Client.Execution.AskHaltedReason();
		}

		private void LostConnection(object sender, EventArgs e)
		{
			this.Client.Connection.LostConnection -= new LostConnectionEventHandle(LostConnection);
			Assert.IsFalse(this.Client.Connection.IsConnected);
			Assert.AreEqual(ErrorCode.NetworkError, this.Client.Error);
		}

		[Test]
		public void CommandConnectionLostDoesNotRaiseHandle()
		{
			this.ServerClient.Close();
			Assert.DoesNotThrow(() => this.Client.Execution.AskHaltedReason());
		}

		[Test]
		public void Interrupt()
		{
			this.SendPacket("S", "02");
			bool stopped = this.Client.Execution.Stop();
			Assert.IsTrue(stopped);

			string rcv = this.Read();
			Assert.AreEqual("\x03", rcv.Substring(0, rcv.Length - 1));
		}

		[Test]
		public void InterruptInvalidSignal()
		{
			this.SendPacket("S", "03");
			bool stopped = this.Client.Execution.Stop();
			Assert.IsFalse(stopped);
		}

		[Test]
		public void InterruptUnknownReplyPacket()
		{
			// Send as many times as the maximum counter of Presentation layer
			for (int i = 0; i < 10; i++)
				this.SendPacket("@", "");

			bool stopped = this.Client.Execution.Stop();
			Assert.IsFalse(this.Client.Connection.IsConnected);
			Assert.IsFalse(stopped);
			Assert.AreEqual(ErrorCode.ProtocolError, this.Client.Error);
		}

		[Test]
		public void InterruptUnexepectedReply()
		{
			this.SendPacket("OK", "");
			bool stopped = this.Client.Execution.Stop();
			Assert.IsFalse(this.Client.Connection.IsConnected);
			Assert.IsFalse(stopped);
			Assert.AreEqual(ErrorCode.ProtocolError, this.Client.Error);
		}

		[Test]
		public void InterruptConnectionLostRaiseHandle()
		{
			this.ServerClient.Close();
			this.Client.Connection.LostConnection += new LostConnectionEventHandle(LostConnection);
			bool stopped = this.Client.Execution.Stop();
			Assert.IsFalse(stopped);
		}

		[Test]
		public void AskHaltedReason()
		{
			this.SendPacket("S", "02");
			StopSignal reason = this.Client.Execution.AskHaltedReason();
			Assert.IsTrue(reason.HasFlag(StopSignal.HostBreak));

			string rcv = this.Read();
			Assert.AreEqual("?", rcv.Substring(1, rcv.Length - 5));
		}

		[Test]
		public void ContinueExecution()
		{
			this.ServerStream.WriteByte(RawPacket.Ack);
			this.Client.Execution.Continue();

			string rcv = this.Read();
			Assert.AreEqual("c", rcv.Substring(1, rcv.Length - 4));
		}

		[Test]
		public void ContinueExecutionAsync()
		{
			this.Client.Execution.BreakExecution += new BreakExecutionEventHandle(BreakpointExecution);
			this.Client.Execution.Continue();

			string rcv = this.Read();
			Assert.AreEqual("c", rcv.Substring(1, rcv.Length - 4));

			this.SendPacket("S", "05");
		}

		private void BreakpointExecution(object sender, StopSignal signal)
		{
			Assert.AreEqual(RawPacket.Ack, this.ServerStream.ReadByte());
			Assert.AreEqual(0, this.ServerClient.Available);

			Assert.IsTrue(signal.HasFlag(StopSignal.Breakpoint));
		}

		[Test]
		public void StepInto()
		{
			this.ServerStream.WriteByte(RawPacket.Ack);
			this.Client.Execution.StepInto();

			string rcv = this.Read();
			Assert.AreEqual("s", rcv.Substring(1, rcv.Length - 4));
		}

		[Test]
		public void StepIntoAsync()
		{
			this.Client.Execution.BreakExecution += new BreakExecutionEventHandle(BreakpointExecution);
			this.Client.Execution.StepInto();

			string rcv = this.Read();
			Assert.AreEqual("s", rcv.Substring(1, rcv.Length - 4));

			this.SendPacket("S", "05");
		}

		[Test]
		public void ContinueIsInterrupted()
		{
			this.Client.Execution.BreakExecution += new BreakExecutionEventHandle(BreakpointExecution);

			this.Client.Execution.Continue();
			string rcv = this.Read();
			Assert.AreEqual("c", rcv.Substring(1, rcv.Length - 4));
			this.ServerStream.WriteByte(RawPacket.Ack);

			Task.Run(() => {
				while (this.ServerStream.ReadByte() != 0x03) ;
				this.SendPacket("S", "02");
			});

			this.Client.Execution.Stop();

			Assert.AreEqual(RawPacket.Ack, this.ServerStream.ReadByte());
			Assert.AreEqual(0, this.ServerClient.Available);
		}
	}
}

