# NitroDebugger [<img alt="Gitter" src="https://badges.gitter.im/Join Chat.svg" align="right" />](https://gitter.im/pleonex/NitroDebugger?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

<p align="center">
<a href="https://travis-ci.org/pleonex/NitroDebugger"><img alt="Build Status" src="https://travis-ci.org/pleonex/NitroDebugger.svg?branch=master" align="left" /></a>
<a href="http://www.gnu.org/copyleft/gpl.html"><img alt="license" src="https://img.shields.io/badge/license-GPL%20V3-blue.svg?style=flat" /></a>
<a href="https://github.com/fehmicansaglam/progressed.io"><img alt="progressed.io" src="http://progressed.io/bar/10" align="right" /></a>
</p>

<br>
<p align="center"><b>Look into game secrets.</b></p>
<p align="center">
  <img alt="logo" src="https://raw.githubusercontent.com/pleonex/NitroDebugger/master/icon/icon_NitroDebugger.png" height="256" width="256" />
</p>

**NitroDebugger** is a remote debugger for Nintendo DS games. It connects to [**DeSmuME**](http://desmume.org/) emulator with the [*GDB Remote Stub Protocol*](https://sourceware.org/gdb/onlinedocs/gdb/Remote-Protocol.html).


## Usage
To use *NitroDebugger* you need:
* *NitroDebugger*: [Compilation](#Compilation) information below.
* *DeSmuME with GDB Stub*: [Compilation](#DeSmuME-with-GDB-Stub) information below.
* *Nintendo DS game*: **I don't support piracy**. [Here](https://gbatemp.net/threads/nds-backup-tool-wifi.252623/) there is a tutorial to create your own backups.


## Compilation
It has been developed and tested with *mono 3.10.0* in *Fedora 20*.

### Linux
You need to install *git* using your package manager (ie *apt-get*, *yum*, *pacman*...) and the last stable mono version from [here](http://www.mono-project.com/docs/getting-started/install/linux/).
``` shell
# Clone the repository
git clone https://github.com/pleonex/NitroDebugger
cd NitroDebugger
```

Now, you can either open the solution with *MonoDevelop* or compile from the terminal:
``` shell
# Restore NuGet packages
wget http://nuget.org/nuget.exe
mono nuget.exe NitroDebugger.sln

# Compile
xbuild NitroDebugger.sln

# [Optional] Run test
# Install nunit-console from your package manager
nunit-console NitroDebugger.UnitTests/bin/Debug/NitroDebugger.UnitTests.dll
```

### Windows
1. Clone the repository with the [GitHub client](https://windows.github.com/) or download the [zip](https://github.com/pleonex/NitroDebugger/archive/master.zip).
2. Download and install *Xamarin Studio* from [here](http://www.monodevelop.com/download/) and open the solution. It should work with *Visual Studio* and [*SharpDevelop*](http://www.icsharpcode.net/OpenSource/SD/Download/) too.
3. Compile!

## DeSmuME with GDB Stub
You need to compile *DeSmuME* to activate the *GDB Stub* support. Once you have that version you must run it with the `--arm9gdb` argument:
``` shell
desmume --arm9gdb=PORT_NUMBER PATH_TO_GAME.nds
```

### Linux
``` shell
svn checkout svn://svn.code.sf.net/p/desmume/code/trunk desmume
cd desmume
./autogen.sh
./configure --enable-gdb-stub
make
sudo make install
```

### Windows
You need *Visual Studio C++*, it works with the *Express* edition too.

1. Download the source code by clicking the *Download Snapshot* button from [here](https://sourceforge.net/p/desmume/code/HEAD/tree/)
2. Unzip it (you can use [7-zip](http://www.7-zip.org/download.html))
3. Copy `src/windows/defaultconfig/userconfig.h` to `src/windows/userconfig/` and comment out the line `#define GDB_STUB`
4. Open the solution file from `src/windows/` with *Visual Studio* and compile
