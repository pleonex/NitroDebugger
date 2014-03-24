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

namespace NitroDebugger
{
	/// <summary>
	/// Enumeration to indicate the state of the program.
	/// Copied from: https://sourceforge.net/p/desmume/code/HEAD/tree/trunk/desmume/src/gdbstub/gdbstub.cpp
	/// </summary>
	public enum TargetSignals : byte
	{
		/* Used some places (e.g. stop_signal) to record the concept that there is no signal. */
		TARGET_SIGNAL_0 = 0,
		TARGET_SIGNAL_FIRST = 0,
		TARGET_SIGNAL_HUP = 1,
		TARGET_SIGNAL_INT = 2,
		TARGET_SIGNAL_QUIT = 3,
		TARGET_SIGNAL_ILL = 4,
		TARGET_SIGNAL_TRAP = 5,
		TARGET_SIGNAL_ABRT = 6,
		TARGET_SIGNAL_EMT = 7,
		TARGET_SIGNAL_FPE = 8,
		TARGET_SIGNAL_KILL = 9,
		TARGET_SIGNAL_BUS = 10,
		TARGET_SIGNAL_SEGV = 11,
		TARGET_SIGNAL_SYS = 12,
		TARGET_SIGNAL_PIPE = 13,
		TARGET_SIGNAL_ALRM = 14,
		TARGET_SIGNAL_TERM = 15,
		TARGET_SIGNAL_URG = 16,
		TARGET_SIGNAL_STOP = 17,
		TARGET_SIGNAL_TSTP = 18,
		TARGET_SIGNAL_CONT = 19,
		TARGET_SIGNAL_CHLD = 20,
		TARGET_SIGNAL_TTIN = 21,
		TARGET_SIGNAL_TTOU = 22,
		TARGET_SIGNAL_IO = 23,
		TARGET_SIGNAL_XCPU = 24,
		TARGET_SIGNAL_XFSZ = 25,
		TARGET_SIGNAL_VTALRM = 26,
		TARGET_SIGNAL_PROF = 27,
		TARGET_SIGNAL_WINCH = 28,
		TARGET_SIGNAL_LOST = 29,
		TARGET_SIGNAL_USR1 = 30,
		TARGET_SIGNAL_USR2 = 31,
		TARGET_SIGNAL_PWR = 32,
		/* Similar to SIGIO. Perhaps they should have the same number. */
		TARGET_SIGNAL_POLL = 33,
		TARGET_SIGNAL_WIND = 34,
		TARGET_SIGNAL_PHONE = 35,
		TARGET_SIGNAL_WAITING = 36,
		TARGET_SIGNAL_LWP = 37,
		TARGET_SIGNAL_DANGER = 38,
		TARGET_SIGNAL_GRANT = 39,
		TARGET_SIGNAL_RETRACT = 40,
		TARGET_SIGNAL_MSG = 41,
		TARGET_SIGNAL_SOUND = 42,
		TARGET_SIGNAL_SAK = 43,
		TARGET_SIGNAL_PRIO = 44,
	};

	/// <summary>
	/// Enumeration to indicate why the game halted.
	/// Copied from: https://sourceforge.net/p/desmume/code/HEAD/tree/trunk/desmume/src/gdbstub/gdbstub_internal.h
	/// </summary>
	public enum StopType {
		STOP_UNKNOWN,
		STOP_HOST_BREAK,
		STOP_STEP_BREAK,
		STOP_BREAKPOINT,
		STOP_WATCHPOINT,
		STOP_RWATCHPOINT,
		STOP_AWATCHPOINT
	}; 
}

