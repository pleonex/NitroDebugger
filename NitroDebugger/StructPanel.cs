//
//  StructPanel.cs
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
using System.Linq;
using System.Reflection;
using Gtk;
using Mono.Addins;

namespace NitroDebugger
{
	[TypeExtensionPoint]
	public abstract class StructPanel<T> : VisualPlugin
		where T : DataStructure, new()
	{
		VBox container;

		protected StructPanel()
		{
			container = new VBox();
			Data = new T();

			Data.Read();
			CreateComponents();
		}

		public override Container MainContainer {
			get { return container; }
		}

		public T Data {
			get;
			private set;
		}

		private void CreateComponents()
		{
			BindingFlags flags = BindingFlags.GetProperty | BindingFlags.SetProperty;
			var properties = typeof(T).GetProperties(flags)
				.Where(p => p.GetCustomAttribute<DataDescription>() != null);

			foreach (var prop in properties) {
				dynamic value = prop.GetValue(Data);
				if (value is bool)
					CreateComponentForBool(prop, value);
			}
		}

		private void CreateComponentForBool(PropertyInfo prop, bool data)
		{
			string descr = prop.GetCustomAttribute<DataDescription>().Description;
			var checkbox = new CheckButton(descr);
			checkbox.Active = data;

			container.Add(checkbox);
		}
	}
}

