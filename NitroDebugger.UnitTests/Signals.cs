//
//  Signals.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2014 Benito Palacios Sánchez
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
using NUnit.Framework;
using NitroDebugger.RSP;

namespace UnitTests
{
	[TestFixture]
	public class Signals
	{
		[Test]
		public void TargetInt2StopHostBreak()
		{
			StopSignal signal = TargetSignals.Int.ToStopSignal();
			Assert.IsTrue(signal.HasFlag(StopSignal.HostBreak));
		}

		[Test]
		public void TargetTrap2StopBreakpoint()
		{
			StopSignal signal = TargetSignals.Trap.ToStopSignal();
			Assert.IsTrue(signal.HasFlag(StopSignal.Breakpoint));
		}

		[Test]
		public void TargetTrap2StopStepBreak()
		{
			StopSignal signal = TargetSignals.Trap.ToStopSignal();
			Assert.IsTrue(signal.HasFlag(StopSignal.StepBreak));
		}

		[Test]
		public void TargetAbrt2StopWatchpoint()
		{
			StopSignal signal = TargetSignals.Abrt.ToStopSignal();
			Assert.IsTrue(signal.HasFlag(StopSignal.Watchpoint));
		}

		[Test]
		public void TargetAbrt2StopReadWatchpoint()
		{
			StopSignal signal = TargetSignals.Abrt.ToStopSignal();
			Assert.IsTrue(signal.HasFlag(StopSignal.ReadWatchpoint));
		}

		[Test]
		public void TargetAbrt2StopAccessWatchpoint()
		{
			StopSignal signal = TargetSignals.Abrt.ToStopSignal();
			Assert.IsTrue(signal.HasFlag(StopSignal.AccessWatchpoint));
		}
	}
}

