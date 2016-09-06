//
// RegisterManager.cs
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

		public Register this[RegisterType type] {
			get { return GetRegister(type); }
			set { SetRegister(value); }
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

