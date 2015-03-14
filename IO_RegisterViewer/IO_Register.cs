using System;
using NitroDebugger.RSP;

namespace IO_RegisterViewer
{
	public interface IO_Register
	{
		void Read(GdbStream stream);
	}
}

