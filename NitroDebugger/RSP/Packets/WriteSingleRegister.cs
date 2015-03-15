//
//  WriteSingleRegister.cs
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

namespace NitroDebugger.RSP.Packets
{
	public class WriteSingleRegister : CommandPacket
	{
		public WriteSingleRegister(Register register)
			: base("P")
		{
			this.Register = register;
		}

		public Register Register {
			get;
			private set;
		}

		protected override string PackArguments()
		{
			return UIntToString((uint)this.Register.Type) + "=" +
				UIntToString(this.Register.Value);
		}

		private string UIntToString(uint data)
		{
			return BitConverter.ToString(BitConverter.GetBytes(data)).Replace("-", "");
		}
	}
}

