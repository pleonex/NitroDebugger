//
// PacketTypesTest.cs
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

			ReadMemoryCommand readMemory = new ReadMemoryCommand(0x00, 0x00);
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket(dataString, readMemory);

			Assert.IsInstanceOf<DataReply>(reply);
			Assert.AreEqual(expected, ((DataReply)reply).GetData());
		}

		[Test]
		public void CreateErrorReply()
		{
			int expected = 3;
			ErrorReply reply = new ErrorReply(expected);
			Assert.AreEqual(ErrorCode.ReadMemoryError, reply.Error);
		}

		[Test]
		public void FactoryErrorReply()
		{
			ErrorCode expected = ErrorCode.ReadMemoryError;
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

		[Test]
		public void CreateReadRegistersCommand()
		{
			ReadRegisters cmd = new ReadRegisters();
			Assert.AreEqual("g", cmd.Command);
			Assert.AreEqual("g", cmd.Pack());
		}

		[Test]
		public void CreateRegistersReply()
		{
			Register[] registers = new Register[] {
				new Register((RegisterType)0, 0x1423),
				new Register((RegisterType)1, 0x3412)};
			RegistersReply reply = new RegistersReply(registers);
			Assert.AreEqual(registers, reply.GetRegisters());
		}

		[Test]
		public void FactoryRegistersReply()
		{
			Register[] expected = CreateRegisters();
			byte[] binRegisters = CreateNetworkRegisters();
			string regString = BitConverter.ToString(binRegisters).Replace("-", "");

			ReadRegisters readRegisters = new ReadRegisters();
			ReplyPacket reply = ReplyPacketFactory.CreateReplyPacket(regString, readRegisters);

			Assert.IsInstanceOf<RegistersReply>(reply);
			Assert.AreEqual(expected, ((RegistersReply)reply).GetRegisters());
		}

		public static Register[] CreateRegisters()
		{
			return new Register[] {
				new Register(RegisterType.R0, 0),   new Register(RegisterType.R1, 1),
				new Register(RegisterType.R2, 2),   new Register(RegisterType.R3, 3),
				new Register(RegisterType.R4, 4),   new Register(RegisterType.R5, 5),
				new Register(RegisterType.R6, 6),   new Register(RegisterType.R7, 7),
				new Register(RegisterType.R8, 8),   new Register(RegisterType.R9, 9),
				new Register(RegisterType.R10, 10), new Register(RegisterType.R11, 11),
				new Register(RegisterType.R12, 12), new Register(RegisterType.LR, 13),
				new Register(RegisterType.SP, 14),  new Register(RegisterType.PC, 15),
				new Register(RegisterType.CPSR, 25)
			};
		}

		public static byte[] CreateNetworkRegisters()
		{
			return new byte[] { 
				0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
				0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
				0x04, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
				0x06, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00,
				0x08, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00,
				0x0A, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00,
				0x0C, 0x00, 0x00, 0x00, 0x0D, 0x00, 0x00, 0x00,
				0x0E, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00,

				// Floating point registers, not used but sent as 0x00
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

				// Floating point status register, not used but send as 0x00
				0x00, 0x00, 0x00, 0x00,

				// CPSR
				0x19, 0x0, 0x0, 0x00
			};
		}

		[Test]
		public void CreateWriteRegistersCommand()
		{
			Register[] registers = CreateRegisters();
			string networkRegisters = BitConverter.ToString(CreateNetworkRegisters());
			networkRegisters = networkRegisters.Replace("-", "");

			WriteRegisters cmd = null;
			Assert.DoesNotThrow(() => cmd = new WriteRegisters(registers));
			Assert.AreEqual("G", cmd.Command);
			Assert.AreEqual("G" + networkRegisters, cmd.Pack());
		}

		[Test]
		public void CreateWriteSingleRegisterCommand()
		{
			Register register = new Register(RegisterType.R12, 0x5040);

			WriteSingleRegister cmd = new WriteSingleRegister(register);
			Assert.AreEqual("P", cmd.Command);
			Assert.AreEqual("P0C000000=40500000", cmd.Pack());
		}
	}
}

