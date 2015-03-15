//
//  ReplyPacketFactory.cs
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
using System.Collections.Generic;
using System.Linq;
using NitroDebugger.RSP.Packets;

namespace NitroDebugger.RSP
{
	public static class ReplyPacketFactory
	{
		public static ReplyPacket CreateReplyPacket(string data, CommandPacket commandSent = null)
		{
			if (data == "OK")
				return new OkReply();

			if (data.Length == 3 && data[0] == 'S')
				return new StopSignalReply(Convert.ToInt32(data.Substring(1), 16));

			if (data.Length == 3 && data[0] == 'E')
				return new ErrorReply(Convert.ToInt32(data.Substring(1), 16));

			if (commandSent is ReadMemoryCommand) {
				try {
					byte[] dataBytes = ConvertToByte(data).ToArray();
					return new DataReply(dataBytes);
				} catch (FormatException) { }
			}

			if (commandSent is ReadRegisters) {
				try {
					List<Register> registers = new List<Register>();
					var byteList = ConvertToByte(data).ToArray();

					// General registers
					registers.AddRange(Enumerable.Range(0, 16)
						.Select(i => new Register(
							(RegisterType)i, BitConverter.ToUInt32(byteList, i * 4)))
					);

					// CPSR
					registers.Add(new Register(
						RegisterType.CPSR, BitConverter.ToUInt32(byteList, 164))
					);

					return new RegistersReply(registers.ToArray());
				} catch (FormatException) { }
			}

			throw new FormatException("Unknown reply");
		}

		private static IEnumerable<byte> ConvertToByte(string data)
		{
			return Enumerable.Range(0, data.Length / 2)
				.Select(i => data.Substring(i * 2, 2))
				.Select(b => Convert.ToByte(b, 16));
		}
	}
}

