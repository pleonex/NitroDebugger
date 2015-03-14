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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NitroDebugger;
using NitroDebugger.RSP.Packets;
using Libgame;
using System.IO;
using Libgame.IO;

namespace NitroDebugger.RSP
{
	public delegate void BreakExecutionEventHandle(object sender, StopSignal signal);

	/// <summary>
	/// GDB Client. Direct connection to emulator.
	/// Application layer, only packets supported by DeSmuME implemented.
	/// </summary>
	public class GdbClient
	{
		private Task currentTask;

		public GdbClient()
		{
			this.Error = ErrorCode.NoError;
			this.Connection = new ConnectionManager(this);
			this.Execution = new ExecutionManager(this);
			this.Stream = new GdbStream(this);
			this.Root = new GameFile("root", null);
		}

		public GdbClient(string game)
			: this()
		{
			if (FileManager.IsInitialized()) {
				var romStream = new DataStream(game, FileMode.Open, FileAccess.Read);
				var romFile = new GameFile(Path.GetFileName(game), romStream);

				this.Root = romFile;
				FileManager.GetInstance().Root.AddFile(romFile);
			}
		}

		public GameFile Root {
			get;
			private set;
		}

		public ConnectionManager Connection {
			get;
			private set;
		}

		public ExecutionManager Execution {
			get;
			private set;
		}

		public GdbStream Stream {
			get;
			private set;
		}

		public ErrorCode Error {
			get;
			private set;
		}

		internal Presentation Presentation {
			get;
			set;
		}

		internal void CancelPendingTask()
		{
			if (this.currentTask != null) {
				this.Presentation.CancelRead(this.currentTask);
				this.currentTask = null;
			}
		}

		internal T SendCommandWithoutErrorReply<T>(CommandPacket command)
			where T : ReplyPacket
		{
			ReplyPacket reply = this.SafeSending(command, typeof(T), typeof(ErrorReply));
			if (reply == null)
				return null;

			ErrorReply error = reply as ErrorReply;
			if (error != null) {
				this.Error = error.Error;
				return null;
			}

			return reply as T;
		}

		internal void SendCommandWithSignalAsync(CommandPacket cmd)
		{
			this.currentTask = Task.Run(() => {
				StopSignal signal = SendCommandWithSignal(cmd);
				this.Execution.OnBreakExecution(signal);
			});
		}

		internal StopSignal SendCommandWithSignal(CommandPacket cmd)
		{
			ReplyPacket response = this.SafeSending(cmd, typeof(StopSignalReply));
			if (response == null)
				return StopSignal.Unknown;

			return ((StopSignalReply)response).Signal;
		}

		internal ReplyPacket SafeInterruption()
		{
			this.Error = ErrorCode.NoError;
			ReplyPacket response = null;

			try {
				response = this.Presentation.SendInterrupt();
			} catch (SocketException) {
				this.Error = ErrorCode.NetworkError;
			} catch (ProtocolViolationException) {
				this.Error = ErrorCode.ProtocolError;
			}

			if (response != null && !(response is StopSignalReply))
				this.Error = ErrorCode.ProtocolError;

			if (this.Error != ErrorCode.NoError) {
				NetworkError();
				response = null;
			}

			return response;
		}

		private ReplyPacket SafeSending(CommandPacket command, params Type[] validReplyTypes)
		{
			this.Error = ErrorCode.NoError;
			ReplyPacket response = null;

			try {
				this.Presentation.SendCommand(command);

				if (validReplyTypes.Length > 0)
					response = this.Presentation.ReceiveReply();
			} catch (SocketException) {
				this.Error = ErrorCode.NetworkError;
			} catch (ProtocolViolationException) {
				this.Error = ErrorCode.ProtocolError;
			}

			if (response != null && !ValidateType(response, validReplyTypes))
				this.Error = ErrorCode.ProtocolError;

			if (this.Error != ErrorCode.NoError) {
				NetworkError();
				response = null;
			}

			return response;
		}

		private bool ValidateType(ReplyPacket reply, Type[] validReplyTypes)
		{
			return validReplyTypes.Contains(reply.GetType());
		}

		private void NetworkError()
		{
			this.Connection.Disconnect();
			this.Connection.OnLostConnection(EventArgs.Empty);
		}
	}
}

