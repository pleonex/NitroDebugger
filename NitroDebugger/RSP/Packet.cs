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
			{ Suffix,         EscapeChar + (char)(Suffix[0]         ^ 0x20) },
			{ Prefix,         EscapeChar + (char)(Prefix[0]         ^ 0x20) },
			{ EscapeChar,     EscapeChar + (char)(EscapeChar[0]     ^ 0x20) },
			{ RunLengthStart, EscapeChar + (char)(RunLengthStart[0] ^ 0x20) }	// Only in response from stub
		};

		public Packet(string command)
		{
			this.Command = command;
		}

		public static byte Ack {
			get {
				return 0x2B;	// '+'
			}
		}

		public static byte Nack {
			get {
				return 0x2D;	// '-'
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
			binPacket.Append(this.CalculateChecksum().ToString("x"));

			return Encoding.GetBytes(binPacket.ToString());
		}

		public static Packet FromBinary(System.Text.StringBuilder dataReceived)
		{
			// Check if there is enough data
			if (dataReceived.Length < 4)
				throw new FormatException("Too small packet");

			// Check the prefix
			if (dataReceived[0] != Packet.Prefix[0])
				throw new FormatException("Invalid packet");

			// Get the packet length
			int packetLength = 0;
			for (int i = 0; i < dataReceived.Length && packetLength == 0; i++) {
				if (dataReceived[i] == Packet.Suffix[0] && (i > 0 && dataReceived[i - 1] != Packet.EscapeChar[0]))
					packetLength = i + 3;
			}

			// Check if there is enough data
			if (packetLength == 0 || packetLength > dataReceived.Length)
				throw new FormatException("Not enough data received");

			// Get the packet and remove from the stringbuilder
			char[] charData = new char[packetLength];
			dataReceived.CopyTo(0, charData, 0, packetLength);
			string packetData = new string(charData);
			dataReceived.Remove(0, packetLength);

			// Get the original command
			string command  = packetData.Substring(1, packetData.Length - 4);
			command = Packet.Unescape(command);
			command = Packet.DecodeRunLength(command);

			// Compare checksum
			string checksum = packetData.Substring(packetData.Length - 2, 2); 
			Packet packet = new Packet(command);

			if (checksum != packet.CalculateChecksum().ToString("x").PadLeft(2, '0'))
				throw new FormatException("Invalid checksum");

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

		private static string DecodeRunLength(string data)
		{
			int startIdx = data.IndexOf(Packet.RunLengthStart);
			while (startIdx != -1) {
				char repeatedChar = data[startIdx - 1];
				int length = data[startIdx + 1] - 29;
				if (length < 3)
					throw new FormatException("Error decoding. Invalid length");

				// Remove RunLengthStart and insert the chars
				data = data.Remove(startIdx, 1);
				data = data.Insert(startIdx, new string(repeatedChar, length));

				startIdx = data.IndexOf(Packet.RunLengthStart);
			}

			return data;
		}
	}
}

