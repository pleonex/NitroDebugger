//
// StructPanel.cs
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
using System.Linq;
using System.Reflection;
using Gtk;
using Mono.Addins;

namespace NitroDebugger.Plugins
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
				else if (value is Enum)
					CreateComponentForEnum(prop, value);
			}
		}

		private void CreateComponentForBool(PropertyInfo prop, bool data)
		{
			string descr = prop.GetCustomAttribute<DataDescription>().Description;
			var checkbox = new CheckButton(descr);
			checkbox.Active = data;

			container.Add(checkbox);
		}

		private void CreateComponentForEnum(PropertyInfo prop, Enum data)
		{
			string descr = prop.GetCustomAttribute<DataDescription>().Description;
			string[] items = Enum.GetNames(data.GetType());

			var subcontainer = new HBox();
			subcontainer.Add(new Label(descr));

			var combo = new ComboBox(items);
			subcontainer.Add(combo);

			container.Add(container);
		}
	}
}

