//
// Program.cs
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
using System.IO;
using System.Reflection;
using NitroDebugger.RSP;
using Gee.External.Capstone;

namespace NitroDebugger
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
			Console.WriteLine("/*");
			Console.WriteLine("**************************");
			Console.WriteLine("**     NitroDebugger    **");
			Console.WriteLine("** The new way to debug **");
			Console.WriteLine("**      your games      **");
			Console.WriteLine("**************************");
			Console.WriteLine("~~~~~~~ by pleoNeX ~~~~~~~");
			Console.WriteLine("| Version: {0,-14}|", Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("~~~~ Good RomHacking! ~~~~");
			Console.WriteLine("*/");
			Console.WriteLine();

			// Create session
			Console.Write("Hostname: "); 	string hostname = Console.ReadLine();
			Console.Write("Port: ");		int port = Convert.ToInt32(Console.ReadLine());
			Console.WriteLine();
			GdbClient client = new GdbClient();
			client.Connection.Connect(hostname, port);

            var disassembler = CapstoneDisassembler.CreateArmDisassembler(DisassembleMode.Arm32);

			// Start the interactive console
            bool finish = false;
            do {
				Console.Write("(gdb) ");
				string cmd = Console.ReadLine();

				switch (cmd) {
				case "quit":
				case "exit":
				case "gotobed":
				case "gotouni":
					Console.WriteLine("You are late again...");
					finish = true;
					break;

				case "HR?":
				case "haltedreason":
					Console.WriteLine(client.Execution.AskHaltedReason());
					break;

				case "c":
                case "cont":
                case "continue":
					client.Execution.Continue();
					break;

                case "s":
                case "step":
                   client.Execution.StepInto();
                    break;

                case "d":
                case "dis":
                    uint pc = client.Registers.GetRegister(RegisterType.PC).Value;
                    byte[] data = client.Stream.Read(pc - 0x10, 0x20);
                    foreach (var inst in disassembler.DisassembleAll(data, pc)) {
                        Console.WriteLine("{0:X8}h\t{1}\t{2}",
                            inst.Address,
                            inst.Mnemonic,
                            inst.Operand);
                    }
                    break;

				case "halt":
                        client.Execution.Stop();
					break;

				case "dump3dsRAM":
                        const uint start = 0x08000000;
                        const uint length = 0x08000000;
                        const int blockSize = 1024;

                        Console.WriteLine("Dumping 0x{0:X8} bytes to {1} starting at 0x{2:X8}",
                                          length, "dump.bin", start);
                        using (var fs = new BinaryWriter(new FileStream("dump.bin", FileMode.Create))) {
                            client.Stream.Seek(start, SeekOrigin.Begin);
                            for (uint i = 0; i < length; i += blockSize) {
                                Console.WriteLine("{0:F2}%", i * 100.0 / length);
                                byte[] dumpData = client.Stream.Read(start + i, blockSize);
                                Console.WriteLine("Received {0} bytes from {1:X8}",
                                    dumpData.Length, start + i);
                                fs.Write(dumpData);
                            }
                        }
                        Console.WriteLine();
					break;

                case "m":
                case "memory":
                    Console.Write("Address: ");
                    uint addr = Convert.ToUInt32(Console.ReadLine(), 16);

                    Console.WriteLine("Memory: ");
                    Console.WriteLine(BitConverter.ToString(client.Stream.Read(addr, 0x10)));
                    break;

				default:
					Console.WriteLine("Unknown command");
					break;
				}
			} while (!finish);

			client.Connection.Disconnect();
		}
	}
}
