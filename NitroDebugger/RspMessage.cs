//
//  RspMessage.cs
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
using System.Text;

namespace NitroDebugger
{
	public class RspMessage
	{
		public RspMessage(string command)
		{
			this.Command = command;
		}

		public string Command {
			get;
			private set;
		}

		public string GetPacket()
		{
			string packet = "$" + this.Command + "#";
			packet += RspMessage.CalculateChecksum(packet).ToString("x");
			return packet;
		}

		public static string CreatePacket(string command)
		{
			return new RspMessage(command).GetPacket();
		}

		private static uint CalculateChecksum(string data)
		{
			uint sum = 0;
			byte[] dataInBytes = Encoding.ASCII.GetBytes(data);

			foreach (byte b in dataInBytes)
				sum += b;

			return sum % 256;
		}
	}
}

