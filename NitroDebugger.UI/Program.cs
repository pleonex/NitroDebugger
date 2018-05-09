// (c) Copyright, Real-Time Innovations, 2018.
// All rights reserved.
//
// No duplications, whole or partial, manual or electronic, may be made
// without express written permission.  Any such copies, or
// revisions thereof, must display this notice unaltered.
// This code contains trade secrets of Real-Time Innovations, Inc.
using System;
using Xwt;

namespace NitroDebugger.UI
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Initialize();

            using (var mainWindow = new MainWindow()) {
                mainWindow.Show();
                Application.Run();
            }

            Application.Dispose();
        }
    }
}
