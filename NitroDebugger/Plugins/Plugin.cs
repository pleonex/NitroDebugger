//
//  Plugin.cs
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
using System.Collections.ObjectModel;
using Mono.Addins;

namespace NitroDebugger.Plugins
{
	[TypeExtensionPoint]
	public abstract class Plugin : IDisposable
	{
		static List<Plugin> instances = new List<Plugin>();

		protected Plugin()
		{
			instances.Add(this);
		}

		~Plugin()
		{
			this.Dispose(false);
		}

		public static ReadOnlyCollection<Plugin> Instances {
			get { return new ReadOnlyCollection<Plugin>(instances); }
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool freeManagedResourcesAlso)
		{
			instances.Remove(this);
		}
	}
}

