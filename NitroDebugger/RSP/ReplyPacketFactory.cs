//
// ReplyPacketFactory.cs
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
				return new StopSignalReply(Convert.ToInt32(data.Substring(1), 16), null);

            if (data.Length > 3 && data[0] == 'T') {
                int signal = Convert.ToInt32(data.Substring(1, 2), 16);
                List<Tuple<byte, uint>> regs = new List<Tuple<byte, uint>>();
                foreach (string reg in data.Substring(3).Split(';')) {
                    if (string.IsNullOrEmpty(reg))
                        continue;
                    
                    string[] fields = reg.Split(':');
                    byte num = Convert.ToByte(fields[0], 16);
                    uint val = Convert.ToUInt32(fields[1], 16);
                    regs.Add(new Tuple<byte, uint>(num, val));
                }
                return new StopSignalReply(signal, regs.ToArray());
            }

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

