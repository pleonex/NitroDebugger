//
//  Packet.cs
//
//  Author:
//       Benito Palacios <benito356@gmail.com>
//
//  Copyright (c) 2014 Benito Palacios
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace NitroDebugger
{
	public class Packet
	{
		private static System.Text.Encoding Encoding = System.Text.Encoding.ASCII;
		private const string Prefix = "$";
		private const string Suffix = "#";
		private const string RunLengthStart = "*";
		private const string EscapeChar = "}";
		private static string[,] EscapeMat = {
			{ Suffix,         EscapeChar + (Suffix[0]         ^ 0x20) },
			{ Prefix,         EscapeChar + (Prefix[0]         ^ 0x20) },
			{ EscapeChar,     EscapeChar + (EscapeChar[0]     ^ 0x20) },
			{ RunLengthStart, EscapeChar + (RunLengthStart[0] ^ 0x20) }	// Only in response from stub
		};

		public Packet(string command)
		{
			this.Command = command;
		}

		public static byte[] Ack {
			get {
				return Encoding.GetBytes("+");
			}
		}

		public static byte[] Nack {
			get {
				return Encoding.GetBytes("-");
			}
		}

		public static char ArgumentSeparator {
			get { return ','; }
		}

		public string Command {
			get;
			private set;
		}

		public byte[] GetBinary()
		{
			System.Text.StringBuilder binPacket = new System.Text.StringBuilder();
			binPacket.Append(Packet.Prefix);
			binPacket.Append(Packet.Escape(this.Command));
			binPacket.Append(Packet.Suffix);
			binPacket.Append(this.CalculateChecksum());

			return Encoding.GetBytes(binPacket.ToString());
		}

		public static Packet FromBinary(byte[] binPacket)
		{
			String packetData = Encoding.GetString(binPacket);

			if (packetData.Length < 4)
				throw new FormatException("Too small packet");

			if (packetData[0] != Packet.Prefix[0] || packetData[packetData.Length - 3] != Packet.Suffix[0])
				throw new FormatException("Invalid packet");

			string command  = packetData.Substring(1, packetData.Length - 4);
			string checksum = packetData.Substring(packetData.Length - 2, 2); 
			Packet packet = new Packet(Packet.Unescape(command));

			if (checksum != packet.CalculateChecksum().ToString("x"))
				throw new FormatException("Invalid checksum");

			// TODO: Decode run-length data

			return packet;
		}

		public uint CalculateChecksum()
		{
			uint sum = 0;
			byte[] dataInBytes = Encoding.GetBytes(this.Command);

			foreach (byte b in dataInBytes)
				sum += b;

			return sum % 256;
		}

		private static string Escape(string command)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(command);
			for (int i = 0; i < 3; i++)
				sb.Replace(EscapeMat[i, 0], EscapeMat[i, 1]);

			return sb.ToString();
		}

		private static string Unescape(string command)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(command);
			for (int i = 0; i < 4; i++)
				sb.Replace(EscapeMat[i, 1], EscapeMat[i, 0]);

			return sb.ToString();
		}
	}
}

