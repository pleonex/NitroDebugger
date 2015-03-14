//
//  PacketRle.cs
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
using System.Text;

namespace NitroDebugger.RSP
{
	public static class Rle
	{
		private const string RunLengthStart = "*";

		public static string Decode(string data)
		{
			bool done = false;
			int startIdx;
			do {
				// Find next RLE start point
				startIdx = data.IndexOf(Rle.RunLengthStart);

				// No more RLE
				if (startIdx == -1) {
					done = true;
					continue;
				}

				// The first char must be the repeated char, not the start point
				if (startIdx == 0)
					throw new FormatException("[RLE] Invalid message");

				data = DecodeAt(data, startIdx);
			} while (!done);

			return data;
		}

		private static string DecodeAt(string data, int start)
		{
			char repeatedChar = data[start - 1];
			int length = data[start + 1] - 28;

			// Validate length
			if (!CheckLength(length))
				throw new FormatException("[RLE] Invalid length");

			// Remove RunLengthStart and length and insert the chars (keep the repeated char)
			data = data.Remove(start, 2);
			data = data.Insert(start, new string(repeatedChar, length - 1));
			return data;
		}

		private static bool CheckLength(int length)
		{
			return length >= 4 && length <= 97 && length != 6 && length != 7;
		}
	}
}

