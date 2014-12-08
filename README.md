**NitroDebugger** is a remote debugger for Nintendo DS games.
[![Build Status](https://travis-ci.org/pleonex/NitroDebugger.svg?branch=master)](https://travis-ci.org/pleonex/NitroDebugger)

By using *GDB Remote Stub* from **DeSmuME** it will allow to pause, continue, set breakpoints, 
view RAM memory, change in runtime code and many other features.

At the moment, it require some patches to be applied to DeSmuME, the files to change
are under *DeSmuMEmod* folder.

It's been developped under *Fedora 20*, *mono 3.10.1* and *monodevelop 5.7*.
* To compile in Linux either open the solution file in monodevelop or run `xbuild NitroDebugger.sln`.
* To compile in Windows either open the solution file in Visual Studio or run `msbuild NitroDebugger.sln`.
