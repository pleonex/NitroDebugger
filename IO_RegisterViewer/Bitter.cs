using System;

namespace IO_RegisterViewer
{
	public abstract class Bitter
	{
		public int Data { get; set; }

		protected Bitter ()
		{
			this.Data = 0;
		}

		protected Bitter (int data)
		{
			this.Data = data;
		}

		/// <summary>
		/// Gets the bits and store them in properties
		/// </summary>
		public abstract void GetBits ();
	}
}

