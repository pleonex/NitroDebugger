//
//  ExecutionManager.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2015 Benito Palacios Sánchez
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

