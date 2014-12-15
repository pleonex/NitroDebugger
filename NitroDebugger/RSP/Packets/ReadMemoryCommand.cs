//
//  ReadMemoryCommand.cs
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

namespace NitroDebugger.RSP.Packets
{
	public class ReadMemoryCommand : CommandPacket
	{
		public ReadMemoryCommand(uint address, int size)
			: base("m")
		{
			this.Address = address;
			this.Size = size;
		}

		public uint Address { 
			get; 
			private set;
		}

		public int Size {
			get;
			private set;
		}

		protected override string PackArguments()
		{
			return String.Format("{0:X},{1:X}", this.Address, this.Size);
		}
	}
}

