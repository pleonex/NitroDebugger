//
//  GdbSessionManager.cs
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
using System.Collections.Generic;

namespace NitroDebugger.RSP
{
	public static class GdbSessionManager
	{
		const int DefaultIndex = 0;
		static int LastIndex = 0;
		static Dictionary<int, GdbClient> clientes = new Dictionary<int, GdbClient>();

		public static GdbClient GetDefaultClient()
		{
			if (!clientes.ContainsKey(DefaultIndex))
				throw new KeyNotFoundException("There is not any default client");

			return clientes[DefaultIndex];
		}

		public static GdbClient GetClient(int index)
		{
			if (!clientes.ContainsKey(index))
				throw new KeyNotFoundException("This client has not been created");

			return clientes[index];
		}

		public static int AddClient(GdbClient client)
		{
			clientes.Add(LastIndex++, client);
			return LastIndex - 1;
		}

		public static void AddClient(int index, GdbClient client)
		{
			if (clientes.ContainsKey(index))
				throw new ArgumentException("There is already a client with this index");

			clientes.Add(index, client);
		}

		public static void RemoveClient(int index)
		{
			if (!clientes.ContainsKey(index))
				throw new KeyNotFoundException("This client has not been created");

			clientes.Remove(index);
		}

		public static void Clear()
		{
			clientes.Clear();
		}
	}
}

