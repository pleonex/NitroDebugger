//
//  PacketTests.cs
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
			Assert.Throws<NotSupportedException>(
				() => PacketBinConverter.FromBinary(data));
		}
	}
}

