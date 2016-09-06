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
using System.Reflection;
using NitroDebugger.RSP;

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

			// Start the interactive console
			bool finish = false;
			do {
				Console.Write("(gdb) "); 	string cmd = Console.ReadLine();

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
					client.Execution.Continue();
					break;

				case "b":
					client.Execution.Stop();
					break;

				case "f":
					System.IO.FileStream fs = new System.IO.FileStream("dump.bin", System.IO.FileMode.Create);
					client.Stream.CopyTo(fs);
					fs.Close();
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
