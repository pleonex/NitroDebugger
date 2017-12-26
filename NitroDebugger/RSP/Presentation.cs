//
// Presentation.cs
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
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NitroDebugger.RSP
{
	/// <summary>
	/// Represents the Presentation layer.
	/// </summary>
	public class Presentation
	{
		private const int MaxWriteAttemps = 10;
		private const byte PacketSeparator = 0x24;	// '$'

		private Session session;
		private CancellationTokenSource cts;
		private CommandPacket lastCommandSent;

		public Presentation(string hostname, int port)
		{
			this.session = new Session(hostname, port);
			this.ResetCancellationToken();
		}

		public void Close()
		{
			this.session.Close();
		}

		public void SendCommand(CommandPacket command)
		{
            // Clean buffer to avoid misunderstanding an ACK
            this.session.CleanReceiveBuffer();

			this.lastCommandSent = command;
			this.SendData(PacketBinConverter.ToBinary(command));
		}

		private void SendData(byte[] data)
		{
			int count = 0;
			int response;

			do {
				if (count == MaxWriteAttemps)
					throw new ProtocolViolationException("[PRES] Can not send correctly");
				count++;

				this.session.Write(data);
				response = this.session.ReadByte();
			} while (response != RawPacket.Ack);
		}

		public ReplyPacket SendInterrupt()
		{
			this.session.Write(new byte[] { RawPacket.Interrupt });
			return this.ReceiveReply();
		}

		public void SendNack()
		{
			this.session.Write(RawPacket.Nack);
		}

		public ReplyPacket ReceiveReply()
		{
			ReplyPacket response = null;
			int count = 0;

			do {
				if (count == MaxWriteAttemps)
					throw new ProtocolViolationException("[PRES] Can not receive correctly");
				count++;

				response = this.NextReply();
			} while (response == null);

			return response;
		}

		private ReplyPacket NextReply()
		{
			ReplyPacket response = null;
			try {
				// Get data
				byte[] packet = this.session.ReadPacket(PacketSeparator);
				response = PacketBinConverter.FromBinary(packet, lastCommandSent);

				// Send ACK
				this.session.Write(RawPacket.Ack);
			} catch (FormatException) {
				// Error... send NACK
				this.session.Write(RawPacket.Nack);
			}

			return response;
		}

		public void CancelRead(Task taskToCancell)
		{
			try {
				this.cts.Cancel();
				taskToCancell.Wait(this.cts.Token);
			} catch (OperationCanceledException) {
			} catch (AggregateException) {
			}

			this.ResetCancellationToken();
		}

		private void ResetCancellationToken()
		{
			this.cts = new CancellationTokenSource();
			this.session.Cancellation = this.cts.Token;
		}
	}
}

