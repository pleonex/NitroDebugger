//
//  Register.cs
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

namespace NitroDebugger
{
	public class Register
	{
		public Register(RegisterType type, uint value)
		{
			Type = type;
			Value = value;
		}

		public RegisterType Type {
			get;
			private set;
		}

		public uint Value {
			get;
			set;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != typeof(Register))
				return false;
			Register other = (Register)obj;
			return Type == other.Type && Value == other.Value;
		}

		public override int GetHashCode()
		{
			unchecked {
				return Type.GetHashCode() ^ Value.GetHashCode();
			}
		}
	}

	public enum RegisterType : byte {
		R0 = 0,
		R1 = 1,
		R2 = 2,
		R3 = 3,
		R4 = 4,
		R5 = 5,
		R6 = 6,
		R7 = 7,
		R8 = 8,
		R9 = 9,
		R10 = 10,
		R11 = 11,
		R12 = 12,
		LR = 13,
		SP = 14,
		PC = 15,
		CPSR = 25
	}
}

