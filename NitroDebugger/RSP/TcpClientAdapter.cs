//
//  TcpClientAdapter.cs
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
using System.IO;
using System.Net.Sockets;

namespace NitroDebugger.RSP
{
	public interface ITcpClient {
		Stream GetStream();
		void Close();
		bool DataAvailable();
	}

	public class TcpClientAdapter : ITcpClient
	{
		private TcpClient wrappedClient;

		public TcpClientAdapter(TcpClient client)
		{
			this.wrappedClient = client;
		}

		public TcpClientAdapter(string hostname, int port)
		{
			this.wrappedClient = new TcpClient(hostname, port);
		}

		public Stream GetStream()
		{
			return wrappedClient.GetStream();
		}

		public void Close()
		{
			wrappedClient.Close();
		}

		public bool DataAvailable()
		{
			return wrappedClient.GetStream().DataAvailable;
		}
	}
}

