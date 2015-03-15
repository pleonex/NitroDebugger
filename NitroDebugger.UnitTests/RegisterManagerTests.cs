//
//  RegisterManagerTests.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2015 Benito Palacios Sánchez
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
	}
}

