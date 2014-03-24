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

namespace NitroDebugger
{
	public static class MainClass
	{
		private static List<char> CmdWithoutResp = new List<char> { 'c' };

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
			Session session = new Session(hostname, port);

			// Start the interactive console
			bool finish = false;
			do {
				Console.Write("(gdb) "); 	string cmd = Console.ReadLine();

				// Different ways to quit the program
				if (cmd == "exit") {
					finish = true;
				} else if (cmd == "gotobed") { 
					finish = true;
					Console.WriteLine("Ok, time to sleep.");
				} else if (cmd == "gotouni") {
					finish = true;
					Console.WriteLine("You are late again...");
				}
				// Now seriously...
				else if (cmd == "break") {
					Console.WriteLine(session.Break());
				} else {
					session.Write(cmd);

					// Check if the command will have a reply
					if (!CmdWithoutResp.Contains(cmd[0])) {
						string response = session.Read();
						if (string.IsNullOrEmpty(response))
							Console.WriteLine("{ERROR}: Unsopported command");
						else
							Console.WriteLine(response);
					}
				}
			} while (!finish);

			session.Close();
		}
	}
}
