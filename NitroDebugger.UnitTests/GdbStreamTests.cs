//
// GdbStreamTests.cs
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
using NitroDebugger;

namespace UnitTests
{
	[TestFixture]
	public class GdbStreamTests : GdbTestingBase
	{
		public GdbStreamTests() : base(1)
		{
		}

		[TestFixtureSetUp]
		protected override void SetUp()
		{
			base.SetUp();
		}

		[TestFixtureTearDown]
		protected override void Dispose()
		{
			base.Dispose();
		}

        [SetUp]
        protected override void OpenServer()
        {
            base.OpenServer();
        }

		[TearDown]
		protected override void CloseServer()
		{
            base.CloseServer();
		}

		[Test]
		public void SeekBegin()
		{
			uint offset = 0x08342142;
			Assert.AreEqual(offset, this.Client.Stream.Seek(offset, System.IO.SeekOrigin.Begin));
			Assert.AreEqual(offset, this.Client.Stream.Position);
		}

		[Test]
		public void SeekCurrent()
		{
			uint start = 0x10;
			uint offset = 0x02;
			uint expected = 0x12;
			this.Client.Stream.Seek(start, System.IO.SeekOrigin.Begin);

			Assert.AreEqual(expected, this.Client.Stream.Seek(offset, System.IO.SeekOrigin.Current));
			Assert.AreEqual(expected, this.Client.Stream.Position);
		}

		[Test]
		public void ReadMemoryGood()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0x00, 0x10, 0x20, 0x39 };
			uint address = 0x02000800;
			int size = 8;

			this.SendPacket("", BitConverter.ToString(expected).Replace("-", ""));
			byte[] actual = this.Client.Stream.Read(address, size);
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);

			string rcv = this.Read();
			Assert.AreEqual("m2000800,8", rcv.Substring(1, rcv.Length - 5));
		}

		[Test]
		public void ReadMemoryErrorReading()
		{
			byte[] expected = new byte[0];
			uint address = 0x02000800;
			int size = 8;

			this.SendPacket("E", "03");
			byte[] actual = this.Client.Stream.Read(address, size);
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(ErrorCode.ReadMemoryError, this.Client.Error);
		}

		[Test]
		public void WriteMemoryGood()
		{
			byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0x00, 0x10, 0x20, 0x39 };
			uint address = 0x02000800;
			int size = 8;

			this.SendPacket("OK", "");
			this.Client.Stream.Write(address, size, expected);
			Assert.AreEqual(ErrorCode.NoError, this.Client.Error);

			string rcv = this.Read();
			string dataString = BitConverter.ToString(expected).Replace("-", "");
			Assert.AreEqual("M2000800,8:" + dataString, rcv.Substring(1, rcv.Length - 5));
		}
	}
}

