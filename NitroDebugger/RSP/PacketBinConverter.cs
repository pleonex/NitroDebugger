//
//  PacketBinConverter.cs
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
using System.Linq;
using System.Text;
using System.IO;

namespace NitroDebugger.RSP
{
	public static class PacketBinConverter
	{
		private static readonly Encoding TextEncoding = Encoding.ASCII;

		private const string Prefix = "$";
		private const string Suffix = "#";

		public static byte[] ToBinary(CommandPacket packet)
		{
			string dataText = packet.Pack();
			byte[] dataBin = TextEncoding.GetBytes(dataText);

			StringBuilder binPacket = new StringBuilder();
			binPacket.Append(PacketBinConverter.Prefix);
			binPacket.Append(dataText);
			binPacket.Append(PacketBinConverter.Suffix);
			binPacket.Append(Checksum.Calculate(dataBin).ToString("x2"));

			return TextEncoding.GetBytes(binPacket.ToString());
		}

		public static ReplyPacket FromBinary(byte[] data)
		{
			if (!ValidateBinary(data))
				throw new FormatException("[BIN] Invalid packet");
				
			string packet = TextEncoding.GetString(data);
			string packetData = packet.Substring(1, packet.Length - 4);
			packetData = Rle.Decode(packetData);

			// Compare checksum
			string receivedChecksum = packet.Substring(packet.Length - 2, 2);
			uint calculatedChecksum = Checksum.Calculate(packetData);
			if (receivedChecksum != calculatedChecksum.ToString("x2"))
				throw new FormatException("[BIN] Invalid checksum");

			return ReplyPacketFactory.CreateReplyPacket(packetData);
		}

		private static bool ValidateBinary(byte[] data)
		{
			if (data.Length < 4)
				return false;
				
			if (data[0] != Prefix[0])
				return false;

			if (data[data.Length - 3] != Suffix[0])
				return false;

			return true;
		}
	}
}

