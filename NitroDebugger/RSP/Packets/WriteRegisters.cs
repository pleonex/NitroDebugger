//
//  WriteRegisters.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2015 Benito Palacios Sánchez
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

namespace NitroDebugger.RSP.Packets
{
	public class WriteRegisters : CommandPacket
	{
		private Register[] registers;

		public WriteRegisters(Register[] registers)
			: base("G")
		{
			if (registers.Length != 17)
				throw new ArgumentException("Invalid number of registers");

			if (Enumerable.Range(0, 16).Any(i => registers[i].Type != (RegisterType)i) ||
				registers[16].Type != RegisterType.CPSR)
				throw new ArgumentException("Invalid registers");

			this.registers = registers;
		}

		public Register[] GetRegisters()
		{
			return this.registers;
		}

		protected override string PackArguments()
		{
			StringBuilder args = new StringBuilder();
			for (int i = 0; i < 16; i++)
				args.AppendFormat("{0:08}", RegisterDataToString(registers[i]));

			// Floating point registers, not used but sent
			args.Append(new String('0', 192));

			// Floating point status registers, not used but sent
			args.Append(new String('0', 8));

			// CPRS
			args.AppendFormat("{0:04}", RegisterDataToString(registers[16]));

			return args.ToString();
		}

		private string RegisterDataToString(Register reg)
		{
			return BitConverter.ToString(BitConverter.GetBytes(reg.Value))
				.Replace("-", "");
		}
	}
}

