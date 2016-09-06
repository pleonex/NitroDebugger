//
// GdbSessionManager.cs
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

