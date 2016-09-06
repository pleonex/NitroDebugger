//
// EngineA.cs
//
// Author:
//       Superfranci99
//
// Copyright (c) 2016 Superfranci99
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
using System.IO;
using NitroDebugger.Plugins;
using NitroDebugger.RSP;

namespace IO_RegisterViewer
{
	public class EngineA : DataStructure
	{
		public override void Write()
		{
			throw new NotImplementedException ();
		}

		public override void Read()
		{
			GdbStream stream = GdbSessionManager.GetDefaultClient().Stream;

			BinaryReader br = new BinaryReader(stream);
			stream.Seek(0x4000000, SeekOrigin.Begin);

			this.Dispcnt  = br.ReadInt32();
			this.Dispstat = br.ReadInt16();
			this.Vcount   = br.ReadInt16();

			this.Engine2dA = new Engine2D();
			this.Engine2dA.Read();

			this.Disp3dcnt      = br.ReadInt16();
			this.Dispcapcnt     = br.ReadInt32();
			this.Disp_mmem_fifo = br.ReadInt32();
			this.Master_bright  = br.ReadInt16();
		}

		public int      Dispcnt        { get; set; }
		public short    Dispstat       { get; set; }
		public short    Vcount         { get; set; }
		public Engine2D Engine2dA      { get; set; }
		public short    Disp3dcnt      { get; set; }
		public int      Dispcapcnt     { get; set; }
		public int      Disp_mmem_fifo { get; set; }
		public short    Master_bright  { get; set; }

		public class Engine2D : DataStructure
		{
			public override void Write()
			{
				throw new NotImplementedException();
			}

			public override void Read()
			{
				GdbStream stream = GdbSessionManager.GetDefaultClient().Stream;

				BinaryReader br = new BinaryReader (stream);

				this.Bg0cnt   = br.ReadInt16();
				this.Bg1cnt   = br.ReadInt16();
				this.Bg2cnt   = br.ReadInt16();
				this.Bg3cnt   = br.ReadInt16();
				this.Bg0hofs  = br.ReadInt16();
				this.Bg0vofs  = br.ReadInt16();
				this.Bg1hofs  = br.ReadInt16();
				this.Bg1vofs  = br.ReadInt16();
				this.Bg2hofs  = br.ReadInt16();
				this.Bg2vofs  = br.ReadInt16();
				this.Bg3hofs  = br.ReadInt16();
				this.Bg3vofs  = br.ReadInt16();
				this.Bg2pa    = br.ReadInt16();
				this.Bg2pb    = br.ReadInt16();
				this.Bg2pc    = br.ReadInt16();
				this.Bg2pd    = br.ReadInt16();
				this.Bg2x     = br.ReadInt32();
				this.Bg2y     = br.ReadInt32();
				this.Bg3pa    = br.ReadInt16();
				this.Bg3pb    = br.ReadInt16();
				this.Bg3pc    = br.ReadInt16();
				this.Bg3pd    = br.ReadInt16();
				this.Bg3x     = br.ReadInt32();
				this.Bg3y     = br.ReadInt32();
				this.Win0h    = br.ReadInt16();
				this.Win1h    = br.ReadInt16();
				this.Win0v    = br.ReadInt16();
				this.Win1v    = br.ReadInt16();
				this.Winin    = br.ReadInt16();
				this.Winout   = br.ReadInt16();
				this.Mosaic   = br.ReadInt16();
				this.NotUsed1 = br.ReadInt16();
				this.Bldcnt   = br.ReadInt16();
				this.Bldalpha = br.ReadInt16();
				this.Bldy     = br.ReadInt16();
				this.NotUsed2 = br.ReadInt16();
			}


			#region Properties

			public short Bg0cnt   { get; set; }
			public short Bg1cnt   { get; set; }
			public short Bg2cnt   { get; set; }
			public short Bg3cnt   { get; set; }
			public short Bg0hofs  { get; set; }
			public short Bg0vofs  { get; set; }
			public short Bg1hofs  { get; set; }
			public short Bg1vofs  { get; set; }
			public short Bg2hofs  { get; set; }
			public short Bg2vofs  { get; set; }
			public short Bg3hofs  { get; set; }
			public short Bg3vofs  { get; set; }
			public short Bg2pa    { get; set; }
			public short Bg2pb    { get; set; }
			public short Bg2pc    { get; set; }
			public short Bg2pd    { get; set; }
			public int   Bg2x     { get; set; }
			public int   Bg2y     { get; set; }
			public short Bg3pa    { get; set; }
			public short Bg3pb    { get; set; }
			public short Bg3pc    { get; set; }
			public short Bg3pd    { get; set; }
			public int   Bg3x     { get; set; }
			public int   Bg3y     { get; set; }
			public short Win0h    { get; set; }
			public short Win1h    { get; set; }
			public short Win0v    { get; set; }
			public short Win1v    { get; set; }
			public short Winin    { get; set; }
			public short Winout   { get; set; }
			public short Mosaic   { get; set; }
			public short NotUsed1 { get; set; }
			public short Bldcnt   { get; set; }
			public short Bldalpha { get; set; }
			public short Bldy     { get; set; }
			public short NotUsed2 { get; set; }

			#endregion
		}

	}
}

