//
//  WriteMemoryCommand.cs
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
	public class WriteMemoryCommand : CommandPacket
	{
		private byte[] data;

		public WriteMemoryCommand(uint address, int size, byte[] data)
			: base("M")
		{
			this.Address = address;
			this.Size = size;
			this.data = (byte[])data.Clone();
		}

		public uint Address {
			get;
			private set;
		}

		public int Size {
			get;
			private set;
		}

		public byte[] GetData()
		{
			return (byte[])this.data.Clone();
		}

		protected override string PackArguments()
		{
			string dataString = BitConverter.ToString(this.data).Replace("-", "");
			return string.Format("{0:X},{1:X}:{2}", this.Address, this.Size, dataString);
		}
	}
}

