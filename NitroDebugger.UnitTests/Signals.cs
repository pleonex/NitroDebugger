//
// Signals.cs
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

