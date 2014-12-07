//
//  GdbClient.cs
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

namespace NitroDebugger.RSP
{
	/// <summary>
	/// GDB Client. Direct connection to emulator.
	/// Application layer, only packets supported by DeSmuME implemented.
	/// </summary>
	public class GdbClient
	{
		private Presentation presentation;

		public GdbClient(string hostname, int port)
		{
			this.presentation = new Presentation(hostname, port);
		}

		public void Close()
		{
			this.presentation.Close();
		}

		public StopType GetHaltedReason()
		{
			presentation.Write("?");
			string response = presentation.Read();
			return this.ParseStopResponse(response);
		}

		public StopType Stop()
		{
			string response = this.presentation.Break();
			return this.ParseStopResponse(response);
		}

		public void Continue()
		{
			this.presentation.Write("c");
		}

		public string Test()
		{
			this.presentation.Write("n2000800,4");
			return this.presentation.Read();
		}

		public StopType NextStep()
		{
			this.presentation.Write("s");
			string response = presentation.Read();
			return this.ParseStopResponse(response);
		}

		private StopType ParseStopResponse(string response)
		{
			if (response.Length == 3 && response[0] == 'S') {
				int signal = Convert.ToInt32(response.Substring(1, 2));
				return this.SignalToStop((TargetSignals)signal);
			} else {
				throw new FormatException("Unexpected response");
			}
		}

		/// <summary>
		/// Converts from the TargetSignal enum used by GDB officially to StopType used internally by DeSmuME.
		/// It gives more info about the reason.
		/// </summary>
		/// <returns>The to stop.</returns>
		/// <param name="signal">Signal.</param>
		private StopType SignalToStop(TargetSignals signal)
		{
			switch (signal) {
			case TargetSignals.TARGET_SIGNAL_INT:
				return StopType.STOP_HOST_BREAK;

			case TargetSignals.TARGET_SIGNAL_TRAP:
				return StopType.STOP_BREAKPOINT;	// Should return also STOP_STEP_BREAK

			case TargetSignals.TARGET_SIGNAL_ABRT:	// Should return also STOP_RWATCHPOINT and STOP_AWATCHPOINT
				return StopType.STOP_WATCHPOINT;

			default:
				return StopType.STOP_UNKNOWN;
			}
		}
	}
}

