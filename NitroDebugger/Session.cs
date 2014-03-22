//
//  Session.cs
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
using System.Net;
using System.Net.Sockets;

namespace NitroDebugger
{
	/// <summary>
	/// Represents the presentation layer.
	/// </summary>
	public class Session
	{
		private UdpClient client;
		private IPEndPoint endPoint;

		public Session(string address, int port)
		{
			this.client = new UdpClient();
			this.endPoint = new IPEndPoint(IPAddress.Parse(address), port);
			this.client.Connect(endPoint);
		}

		public void Close()
		{
			this.client.Close();
		}

		public void Write(string message)
		{
			Packet packet = new Packet(message);
			byte[] data = packet.GetBinary();
			this.client.Send(data, data.Length);
		}

		public string Read()
		{
			IPEndPoint origin = new IPEndPoint(IPAddress.Any, 0);
			byte[] data = this.client.Receive(ref origin);
			return Packet.FromBinary(data).Command;
		}
	}
}

