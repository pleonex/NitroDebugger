//
//  PacketTypesTest.cs
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
using NitroDebugger.RSP.Packets;
using NitroDebugger;

namespace UnitTests
{
	[TestFixture]
	public class PacketTypesTest
	{
		[Test]
		public void CreateOkReply()
		{
			new OkReply();
			Assert.Pass();
		}

		[Test]
		public void FactoryCreateOkReply()
		{
			Assert.IsInstanceOf<OkReply>(ReplyPacketFactory.CreateReplyPacket("OK"));
		}

		[Test]
		public void CreateStopSignalReplyHostBreak()
		{
			StopSignalReply reply = new StopSignalReply(2);
			Assert.IsTrue(reply.Signal.HasFlag(StopSignal.HostBreak));
		}

		[Test]
		public void FactoryStopSignalReplyHostBreak()
		{
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket("S02");
			Assert.IsInstanceOf<StopSignalReply>(reply);
			Assert.IsTrue(((StopSignalReply)reply).Signal.HasFlag(StopSignal.HostBreak));
		}

		[Test]
		public void CreateStopSignalReplyBreakpoint()
		{
			StopSignalReply reply = new StopSignalReply(5);
			Assert.IsTrue(reply.Signal.HasFlag(StopSignal.Breakpoint));
		}

		[Test]
		public void FactoryStopSignalReplyBreakpoint()
		{
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket("S05");
			Assert.IsInstanceOf<StopSignalReply>(reply);
			Assert.IsTrue(((StopSignalReply)reply).Signal.HasFlag(StopSignal.Breakpoint));
		}

		[Test]
		public void CreateStopSignalReplyStepBreak()
		{
			StopSignalReply reply = new StopSignalReply(5);
			Assert.IsTrue(reply.Signal.HasFlag(StopSignal.StepBreak));
		}

		[Test]
		public void FactoryStopSignalReplyStepBreak()
		{
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket("S05");
			Assert.IsInstanceOf<StopSignalReply>(reply);
			Assert.IsTrue(((StopSignalReply)reply).Signal.HasFlag(StopSignal.StepBreak));
		}

		[Test]
		public void CreateStopSignalReplyWatchpoint()
		{
			StopSignalReply reply = new StopSignalReply(6);
			Assert.IsTrue(reply.Signal.HasFlag(StopSignal.Watchpoint));
		}

		[Test]
		public void FactoryStopSignalReplyWatchpoint()
		{
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket("S06");
			Assert.IsInstanceOf<StopSignalReply>(reply);
			Assert.IsTrue(((StopSignalReply)reply).Signal.HasFlag(StopSignal.Watchpoint));
		}

		[Test]
		public void CreateStopSignalReplyAccessWatchpoint()
		{
			StopSignalReply reply = new StopSignalReply(6);
			Assert.IsTrue(reply.Signal.HasFlag(StopSignal.AccessWatchpoint));
		}

		[Test]
		public void FactoryStopSignalReplyAccessWatchpoint()
		{
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket("S06");
			Assert.IsInstanceOf<StopSignalReply>(reply);
			Assert.IsTrue(((StopSignalReply)reply).Signal.HasFlag(StopSignal.AccessWatchpoint));
		}

		[Test]
		public void CreateStopSignalReplyReadWatchpoint()
		{
			StopSignalReply reply = new StopSignalReply(6);
			Assert.IsTrue(reply.Signal.HasFlag(StopSignal.ReadWatchpoint));
		}

		[Test]
		public void FactoryStopSignalReplyReadWatchpoint()
		{
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket("S06");
			Assert.IsInstanceOf<StopSignalReply>(reply);
			Assert.IsTrue(((StopSignalReply)reply).Signal.HasFlag(StopSignal.ReadWatchpoint));
		}

		[Test]
		public void CreateHaltedReasonCommand()
		{
			HaltedReasonCommand cmd = new HaltedReasonCommand();
			Assert.AreEqual("?", cmd.Command);
			Assert.AreEqual("?", cmd.Pack());
		}

		[Test]
		public void CreateContinueCommand()
		{
			ContinueCommand cmd = new ContinueCommand();
			Assert.AreEqual("c", cmd.Command);
			Assert.AreEqual("c", cmd.Pack());
		}

		[Test]
		public void CreateSingleStepCommand()
		{
			SingleStep cmd = new SingleStep();
			Assert.AreEqual("s", cmd.Command);
			Assert.AreEqual("s", cmd.Pack());
		}

		[Test]
		public void CreateReadMemoryCommand()
		{
			uint address = 0x02000800;
			int size = 8;
			ReadMemoryCommand cmd = new ReadMemoryCommand(address, size);
			Assert.AreEqual(address, cmd.Address);
			Assert.AreEqual(size, cmd.Size);
			Assert.AreEqual("m", cmd.Command);
			Assert.AreEqual("m2000800,8", cmd.Pack());
		}

		[Test]
		public void CreateDataReply()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0x00, 0x10, 0x20 };
			DataReply reply = new DataReply(expected);
			Assert.AreEqual(expected, reply.GetData());
		}

		[Test]
		public void FactoryDataReply()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0x00, 0x10, 0x20 };
			string dataString = BitConverter.ToString(expected).Replace("-", "");

			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket(dataString);

			Assert.IsInstanceOf<DataReply>(reply);
			Assert.AreEqual(expected, ((DataReply)reply).GetData());
		}

		[Test]
		public void CreateErrorReply()
		{
			int expected = 0x3;
			ErrorReply reply = new ErrorReply(expected);
			Assert.AreEqual(expected, reply.Error);
		}

		[Test]
		public void FactoryErrorReply()
		{
			int expected = 0x3;
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket("E03");

			Assert.IsInstanceOf<ErrorReply>(reply);
			Assert.AreEqual(expected, ((ErrorReply)reply).Error);
		}

		[Test]
		public void CreateWriteMemoryCommand()
		{
			uint address = 0x02000800;
			int size = 8;
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0x00, 0x10, 0x20, 0x39 };
			string dataString = BitConverter.ToString(expected).Replace("-", "");

			WriteMemoryCommand cmd = new WriteMemoryCommand(address, size, expected);
			Assert.AreEqual(address, cmd.Address);
			Assert.AreEqual(size, cmd.Size);
			Assert.AreEqual(expected, cmd.GetData());
			Assert.AreEqual("M", cmd.Command);
			Assert.AreEqual("M2000800,8:" + dataString, cmd.Pack());
		}
	}
}

