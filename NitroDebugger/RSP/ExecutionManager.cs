//
// ExecutionManager.cs
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
using NitroDebugger.RSP.Packets;

namespace NitroDebugger.RSP
{
	public class ExecutionManager
	{
		internal ExecutionManager(GdbClient client)
		{
			this.Client = client;
		}

		public GdbClient Client {
			get;
			private set;
		}

		public event BreakExecutionEventHandle BreakExecution;

		internal void OnBreakExecution(StopSignal signal)
		{
			if (BreakExecution != null)
				BreakExecution(this, signal);
		}

		public bool Stop()
		{
			// Stop any pending continue / step asyn task
			this.Client.CancelPendingTask();

			ReplyPacket response = this.Client.SafeInterruption();
			if (response == null)
				return false;

            StopSignalReply stopResponse = response as StopSignalReply;

			return ((StopSignalReply)response).Signal.HasFlag(StopSignal.HostBreak);
		}

		public StopSignal AskHaltedReason()
		{
			HaltedReasonCommand command = new HaltedReasonCommand();
			return this.Client.SendCommandWithSignal(command);
		}

		public void Continue()
		{
			ContinueCommand cont = new ContinueCommand();
			this.Client.SendCommandWithSignalAsync(cont);
		}

		public void StepInto()
		{
			SingleStep step = new SingleStep();
			this.Client.SendCommandWithSignalAsync(step);
		}
	}
}

