//
//  TargetSignals.cs
//
//  Author:
//       Benito Palacios <benito356@gmail.com>
//
//  Copyright (c) 2014 Benito Palacios
//
//	Copyright (C) 2008-2010 DeSmuME team
//	Originally written by Ben Jaques. 
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
	/// Enumeration to indicate the state of the program.
	/// Copied from: https://sourceforge.net/p/desmume/code/HEAD/tree/trunk/desmume/src/gdbstub/gdbstub.cpp
	/// </summary>
	public enum TargetSignals : byte
	{
		NoSignal = 0,
		First = 0,
		Hup = 1,
		Int = 2,
		Quit = 3,
		Ill = 4,
		Trap = 5,
		Abrt = 6,
		Emt = 7,
		Fpe = 8,
		Kill = 9,
		Bus = 10,
		Segv = 11,
		Sys = 12,
		Pipe = 13,
		Alrm = 14,
		Term = 15,
		Urg = 16,
		Stop = 17,
		Tstp = 18,
		Cont = 19,
		Chld = 20,
		Ttin = 21,
		Ttou = 22,
		Io = 23,
		Xcpu = 24,
		Xfsz = 25,
		Vtalrm = 26,
		Prof = 27,
		Winch = 28,
		Lost = 29,
		Usr1 = 30,
		Usr2 = 31,
		Pwr = 32,
		Poll = 33,
		Wind = 34,
		Phone = 35,
		Waiting = 36,
		Lwp = 37,
		Danger = 38,
		Grant = 39,
		Retract = 40,
		Msg = 41,
		Sound = 42,
		Sak = 43,
		Prio = 44,
	};

	/// <summary>
	/// Enumeration to indicate why the game halted.
	/// Copied from: https://sourceforge.net/p/desmume/code/HEAD/tree/trunk/desmume/src/gdbstub/gdbstub_internal.h
	/// </summary>
	[Flags]
	public enum StopSignal {
		Unknown = 0,
		HostBreak = 1,
		StepBreak = 2,
		Breakpoint = 4,
		Watchpoint = 8,
		ReadWatchpoint = 16,
		AccessWatchpoint = 32
	};

	public static class StopSignalExtension
	{
		/// <summary>
		/// Converts from the TargetSignal enum used by GDB officially to StopType used internally by DeSmuME.
		/// It gives more info about the reason.
		/// </summary>
		/// <returns>The to stop.</returns>
		/// <param name="signal">Signal.</param>
		public static StopSignal ToStopSignal(this TargetSignals signal)
		{
			switch (signal) {
			case TargetSignals.Int:
				return StopSignal.HostBreak;

			case TargetSignals.Trap:
				return StopSignal.Breakpoint | StopSignal.StepBreak;

			case TargetSignals.Abrt:
				return StopSignal.Watchpoint | StopSignal.ReadWatchpoint | StopSignal.AccessWatchpoint;

			default:
				return StopSignal.Unknown;
			}
		}
	}
}

