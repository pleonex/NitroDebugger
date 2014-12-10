//
//  Program.cs
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
			GdbClient client = new GdbClient(hostname, port);

			// Start the interactive console
			bool finish = false;
			do {
				Console.Write("(gdb) "); 	string cmd = Console.ReadLine();

				switch (cmd) {
				case "exit":
				case "gotobed":
				case "gotouni":
					Console.WriteLine("You are late again...");
					finish = true;
					break;

				default:
					Console.WriteLine("Unknown command");
					break;
				}
			} while (!finish);

			client.Disconnect();
		}
	}
}
