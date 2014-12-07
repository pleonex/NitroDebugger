//
//  Presentation.cs
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
using System.Text;

namespace NitroDebugger.RSP
{
	/// <summary>
	/// Represents the Presentation layer.
	/// </summary>
	public class Presentation
	{
		private const int MaxWriteAttemps = 10;

		private Session session;
		private StringBuilder buffer;

		public Presentation(string hostname, int port)
		{
			this.session = new Session(hostname, port);
			this.buffer  = new StringBuilder();
		}

		public void Close()
		{
			this.session.Close();
		}

		public string Break()
		{
			this.session.Write(0x03);
			return this.Read();
		}

		public void Write(string message)
		{
			int count = 0;
			int response;

			do {
				if (count == MaxWriteAttemps)
					throw new Exception("Can not send packet successfully");
				count++;

				Packet packet = new Packet(message);
				byte[] data = packet.GetBinary();
				this.session.Write(data);

				// Get the response
				do
					response = this.session.ReadByte();
				while (response == -1);

				// Check the response is valid
				if (response != Packet.Ack && response != Packet.Nack)
					throw new Exception("Invalid ACK/NACK");

			} while (response != Packet.Ack);
		}

		public string Read()
		{
			do
				this.UpdateBuffer();
			while (buffer.Length == 0);

			string command = null;

			try {
				// Get packet
				Packet response = Packet.FromBinary(buffer);
				command = response.Command;

				// Send ACK
				this.session.Write(Packet.Ack);
			} catch (FormatException ex) {
				// Error... send NACK
				this.session.Write(Packet.Nack);
			}


			return command;
		}

		private void UpdateBuffer()
		{
			while (this.session.DataAvailable) {
				byte[] data = this.session.ReadBytes();
				buffer.AppendFormat("{0}", Encoding.ASCII.GetString(data));
			}
		}
	}
}

