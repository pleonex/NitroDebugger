//
//  RegisterManager.cs
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
using NitroDebugger.RSP.Packets;

namespace NitroDebugger.RSP
{
	public class RegisterManager
	{
		private Register[] registers;

		public RegisterManager(GdbClient client)
		{
			this.Client = client;

			this.registers = new Register[17];
			for (int i = 0; i < 16; i++)
				this.registers[i] = new Register((RegisterType)i, 0x00);
			this.registers[16] = new Register(RegisterType.CPSR, 0x00);
		}

		public GdbClient Client {
			get;
			private set;
		}

		public Register[] GetRegisters()
		{
			ReadRegisters();	// TODO: Call it, only when invalidated
			return this.registers;
		}

		public Register GetRegister(RegisterType type)
		{
			ReadRegisters();
			if (type != RegisterType.CPSR)
				return this.registers[(int)type];
			else
				return this.registers[16];
		}

		public void SetRegisters(Register[] registers)
		{
			this.registers = registers;
			WriteRegisters();
		}

		public void SetRegister(Register register)
		{
			if (register.Type != RegisterType.CPSR)
				this.registers[(int)register.Type] = register;
			else
				this.registers[16] = register;

			WriteSingleRegister(register);
		}

		private void ReadRegisters()
		{
			ReadRegisters command = new ReadRegisters();
			RegistersReply reply = 
				this.Client.SendCommandWithoutErrorReply<RegistersReply>(command);
			if (reply == null)
				return;

			this.registers = reply.GetRegisters();
		}

		private void WriteRegisters()
		{
			WriteRegisters command = new WriteRegisters(this.registers);
			this.Client.SendCommandWithoutErrorReply<OkReply>(command);
		}

		private void WriteSingleRegister(Register register)
		{
			WriteSingleRegister command = new WriteSingleRegister(register);
			this.Client.SendCommandWithoutErrorReply<OkReply>(command);
		}
	}
}

