//
// TargetSignals.cs
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

namespace NitroDebugger.RSP
{
	/// <summary>
	/// Enumeration to indicate the state of the program.
	/// Values from: https://sourceforge.net/p/desmume/code/HEAD/tree/trunk/desmume/src/gdbstub/gdbstub.cpp
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
	/// Values from: https://sourceforge.net/p/desmume/code/HEAD/tree/trunk/desmume/src/gdbstub/gdbstub_internal.h
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

