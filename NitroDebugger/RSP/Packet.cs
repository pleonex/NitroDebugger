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
using System.Text;

namespace NitroDebugger.RSP
{
	public abstract class Packet
	{
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

		public static byte Interrupt {
			get {
				return 0x03;
			}
		}

		public string Command {
			get;
			private set;
		}

		public string Pack() {
			return this.Command + this.PackArguments();
		}

		protected abstract string PackArguments();
	}
}

