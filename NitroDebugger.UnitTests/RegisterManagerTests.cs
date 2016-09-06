//
// RegisterManagerTests.cs
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
using NitroDebugger;

namespace UnitTests
{
	[TestFixture]
	public class RegisterManagerTests : GdbTestingBase
	{
		private RegisterManager manager;

		public RegisterManagerTests()
			: base(2)
		{
		}

		[TestFixtureSetUp]
		protected override void Setup()
		{
			base.Setup();
			manager = Client.Registers;
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
		public void ReadQuery()
		{
			Register[] expected = PacketTypesTest.CreateRegisters();
			byte[] networkData = PacketTypesTest.CreateNetworkRegisters();

			this.SendPacket("", BitConverter.ToString(networkData).Replace("-", ""));
			Register[] actual = manager.GetRegisters();
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);

			string rcv = this.Read();
			Assert.AreEqual("g", rcv.Substring(1, rcv.Length - 5));
		}

		[Test]
		public void GetAGeneralRegister()
		{
			TestGetRegister(RegisterType.R10);
		}

		[Test]
		public void GetRegisterFromIndexer()
		{
			Register expected = new Register(RegisterType.R0, 0);

			byte[] networkData = PacketTypesTest.CreateNetworkRegisters();
			this.SendPacket("", BitConverter.ToString(networkData).Replace("-", ""));

			Register actual = manager[RegisterType.R0];
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);
		}

		[Test]
		public void GetCpsrRegister()
		{
			TestGetRegister(RegisterType.CPSR);
		}

		private void TestGetRegister(RegisterType type)
		{
			Register expected = new Register(type, (uint)type);

			byte[] networkData = PacketTypesTest.CreateNetworkRegisters();
			this.SendPacket("", BitConverter.ToString(networkData).Replace("-", ""));

			Register actual = manager.GetRegister(type);
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);
		}

		[Test]
		public void WriteQuery()
		{
			Register[] registers = PacketTypesTest.CreateRegisters();
			byte[] expected = PacketTypesTest.CreateNetworkRegisters();

			this.SendPacket("OK", "");
			manager.SetRegisters(registers);
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);

			string rcv = this.Read();
			string dataString = BitConverter.ToString(expected).Replace("-", "");
			Assert.AreEqual("G" + dataString, rcv.Substring(1, rcv.Length - 5));
		}

		[Test]
		public void WriteSingleQuery()
		{
			Register register = new Register(RegisterType.R1, 0x2);
			string expected = "P01000000=02000000";

			this.SendPacket("OK", "");
			manager.SetRegister(register);
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);

			string rcv = this.Read();
			Assert.AreEqual(expected, rcv.Substring(1, rcv.Length - 5));
		}

		[Test]
		public void WriteSingleQueryWithIndexer()
		{
			Register register = new Register(RegisterType.R1, 0x2);
			string expected = "P01000000=02000000";

			this.SendPacket("OK", "");
			manager[RegisterType.R1] = register;
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);

			string rcv = this.Read();
			Assert.AreEqual(expected, rcv.Substring(1, rcv.Length - 5));
		}
	}
}

