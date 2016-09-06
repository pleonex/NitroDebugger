//
// GdbSessionManagerTests.cs
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
using NitroDebugger.RSP;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class GdbSessionManagerTests
	{
		[TearDown]
		public void Reset()
		{
			GdbSessionManager.Clear();
		}

		[Test]
		public void AddAndGetAClient()
		{
			var client = new GdbClient();
			GdbSessionManager.AddClient(40, client);
			Assert.AreSame(client, GdbSessionManager.GetClient(40));
		}

		[Test]
		public void TryToAddTwiceWithSameIndex()
		{
			var client = new GdbClient();
			GdbSessionManager.AddClient(30, client);
			Assert.Throws<ArgumentException>(()=>GdbSessionManager.AddClient(30, client));
		}

		[Test]
		public void TryToAddTwoClientWithSameIndex()
		{
			var client1 = new GdbClient();
			var client2 = new GdbClient();
			GdbSessionManager.AddClient(0, client1);
			Assert.Throws<ArgumentException>(() => GdbSessionManager.AddClient(0, client2));
		}

		[Test]
		public void TryToGetANonExistingClient()
		{
			Assert.Throws<KeyNotFoundException>(() => GdbSessionManager.GetClient(30));
		}

		[Test]
		public void AddAndRemoveClient()
		{
			var client = new GdbClient();
			GdbSessionManager.AddClient(1, client);
			GdbSessionManager.RemoveClient(1);
			Assert.Throws<KeyNotFoundException>(() => GdbSessionManager.GetClient(1));
		}

		[Test]
		public void TryToRemoveANonExistingClient()
		{
			Assert.Throws<KeyNotFoundException>(() => GdbSessionManager.RemoveClient(0));
		}

		[Test]
		public void RemoveAllSessions()
		{
			var client = new GdbClient();
			GdbSessionManager.AddClient(1, client);
			GdbSessionManager.Clear();
			Assert.Throws<KeyNotFoundException>(() => GdbSessionManager.GetClient(1));
		}

		[Test]
		public void GetDefaultForIndexZero()
		{
			var client = new GdbClient();
			GdbSessionManager.AddClient(0, client);
			Assert.AreSame(client, GdbSessionManager.GetDefaultClient());
		}

		[Test]
		public void ReturnsDefaultOnlyForIndexZero()
		{
			var client = new GdbClient();
			GdbSessionManager.AddClient(1, client);
			Assert.Throws<KeyNotFoundException>(() => GdbSessionManager.GetDefaultClient());
		}

		[Test]
		public void AddDefaultGetsDefault()
		{
			var client = new GdbClient();
			GdbSessionManager.AddClient(client);
			Assert.AreSame(client, GdbSessionManager.GetDefaultClient());
		}
	}
}

