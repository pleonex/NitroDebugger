//
//  GdbSessionManagerTests.cs
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
	}
}

