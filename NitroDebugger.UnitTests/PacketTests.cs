//
// PacketTests.cs
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
using Moq;
using Moq.Protected;
using NUnit.Framework;
using NitroDebugger.RSP;
using NitroDebugger;
using System.Text;
using NitroDebugger.RSP.Packets;

namespace UnitTests
{
	[TestFixture]
	public class PacketTests
	{
		private void TestRle(char ch, int count)
		{
			string expected = new string(ch, count);
			string actual = Rle.Decode(ch + "*" + (char)(28 + count));
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void DecodeRle4()
		{
			TestRle('W', 4);
		}

		[Test]
		public void DecodeRle6()
		{
			Assert.Throws<FormatException>(() => TestRle('W', 6));
		}

		[Test]
		public void DecodeRle80()
		{
			TestRle('W', 80);
		}

		[Test]
		public void DecodeRle2()
		{
			Assert.Throws<FormatException>(() => TestRle('W', 2));
		}

		[Test]
		public void DecodeRle98()
		{
			Assert.Throws<FormatException>(() => TestRle('W', 98));
		}

		[Test]
		public void DecodeRleStartPointAt0()
		{
			Assert.Throws<FormatException>(() => Rle.Decode("*" + (char)(28 + 4)));
		}

		[Test]
		public void DecodeRleNoEncoded()
		{
			string expected = "WWWW";
			string actual = Rle.Decode("WWWW");
			Assert.AreEqual(expected, actual);
		}

		private void TestChecksum(uint expected, byte[] data)
		{
			uint actual = Checksum.Calculate(data);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ChecksumEmpty()
		{
			TestChecksum(0, new byte[0]);
		}

		[Test]
		public void ChecksumEmptyString()
		{
			uint expected = 0;
			uint actual = Checksum.Calculate("");
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ChecksumWithDataLower256()
		{
			TestChecksum(0x81, new byte[] { 0x40, 0x01, 0x40 });
		}

		[Test]
		public void ChecksumWithDataEqual256()
		{
			TestChecksum(0x00, new byte[] { 0x40, 0x01, 0x0F, 0x30, 0x80 });
		}

		[Test]
		public void ChecksumWithDataGreater256()
		{
			TestChecksum(0x11, new byte[] { 0x80, 0x41, 0x3F, 0x10, 0x01 });
		}

		[Test]
		public void ChecksumWithDataGreater256String()
		{
			uint expected = 0xe4;
			uint actual = Checksum.Calculate("m02000800,0100");
			Assert.AreEqual(expected, actual);
		}

		private void TestPacketToBin(string cmd, string args, string crc)
		{
			string expected = "$" + cmd + args + "#" + crc;

			Mock<CommandPacket> packet = new Mock<CommandPacket>(cmd);
			packet.Protected().Setup<String>("PackArguments").Returns(args);

			byte[] binary = PacketBinConverter.ToBinary(packet.Object);
			string actual = Encoding.ASCII.GetString(binary);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void EmptyPacketToBin()
		{
			TestPacketToBin("", "", "00");
		}

		[Test]
		public void NoArgsPacketToBin()
		{
			TestPacketToBin("c", "", "63");
		}

		[Test]
		public void ArgsPacketToBin()
		{
			TestPacketToBin("m", "02000800,0100", "e4");
		}

		[Test]
		public void FromBinSmallPacket()
		{
			byte[] data = new byte[] { 0xFF, 0xFF };
			Assert.Throws<FormatException>(() => PacketBinConverter.FromBinary(data));
		}

		[Test]
		public void FromBinNoPrefix()
		{
			//							m     c     #     0     0
			byte[] data = new byte[] { 0x6D, 0x63, 0x23, 0x30, 0x30 };
			Assert.Throws<FormatException>(() => PacketBinConverter.FromBinary(data));
		}

		[Test]
		public void FromBinNoSuffix()
		{
			//						    $     c     0     0 
			byte[] data = new byte[] { 0x24, 0x63, 0x30, 0x30 };
			Assert.Throws<FormatException>(() => PacketBinConverter.FromBinary(data));
		}

		[Test]
		public void FromBinInvalidChecksum()
		{
			//							$     c     #     0     0
			byte[] data = new byte[] { 0x24, 0x63, 0x23, 0x30, 0x30 };
			Assert.Throws<FormatException>(() => PacketBinConverter.FromBinary(data));
		}

		[Test]
		public void FromBinReplyPacket()
		{
			//							$     O     K     #     9     a
			byte[] data = new byte[] { 0x24, 0x4F, 0x4B, 0x23, 0x39, 0x61 };
			Assert.IsInstanceOf<OkReply>(PacketBinConverter.FromBinary(data));
		}

		[Test]
		public void FromBinUnknownReplyPacket()
		{
			//							$     P     K     #     9     b
			byte[] data = new byte[] { 0x24, 0x50, 0x4B, 0x23, 0x39, 0x62 };
			Assert.Throws<FormatException>(
				() => PacketBinConverter.FromBinary(data));
		}
	}
}

