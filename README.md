# Client (PC-software) for Famicom Dumper/Programmer

This is the client for the Famicom Dumper/Programmer hardware:
- [https://github.com/ClusterM/famicom-dumper-writer](https://github.com/ClusterM/famicom-dumper-writer) - my own dumper project
- [https://github.com/postal2201/8-bit-DumpShield](https://github.com/postal2201/8-bit-DumpShield) - Arduino MEGA2560 Shield

## Requirements

This application developed using .NET 5.0, so it can be run on Windows (x86, x64, arm), Linux (x64, arm) and OSX (x64). You need either install the [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) framework or use the self-contained version.


## Features

It can be used to:
- Dump Famicom/NES cartridges using C# scripts to describe any mapper, also it's bundled with scripts for some popular mappers
- Reverse engineer unknown mappers using C# scripts
- Read/write battery-backed PRG RAM to transfer game saves
- Read/write Famicom Disk System cards
- (Re)write ultra-cheap COOLBOY cartridges using both soldering (for old revisions) and soldering-free (new ones) versions, also it supports both COOLBOY (with $600x registers) and COOLBOY2 aka MINDKIDS (with $500x registers)
- (Re)write [COOLGIRL](https://github.com/ClusterM/coolgirl-famicom-multicard) cartridges
- Test hardware in cartridges
- Do everything described above over the network
- Access dumper from your C#, C++, Dart, Go, Java, Kotlin, Node, Objective-C, PHP, Python, Ruby, etc. code using gRPC


## Usage

It's a command-line application.

Usage: **famicom-dumper \<command\> [options]**

Available commands:
- **list-mappers** - list available mappers to dump
- **dump** - dump cartridge
- **server** - start gRPC server
- **script** - execute C# script specified by --cs-file option
- **reset** - simulate reset (M2 goes to Z-state for a second)
- **dump-fds** - dump FDS card using RAM adapter and FDS drive
- **write-fds** - write FDS card using RAM adapter and FDS drive
- **read-prg-ram** - read PRG RAM (battery backed save if exists)
- **write-prg-ram** - write PRG RAM
- **write-coolboy** - write COOLBOY cartridge
- **write-coolgirl** - write COOLGIRL cartridge
- **info-coolboy** - show information about COOLBOY's flash memory
- **info-coolgirl** - show information about COOLGIRL's flash memory

Available options:
- **--port** <*com*> - serial port of dumper or serial number of dumper device, default - **auto**
- **--tcp-port** <*port*> - TCP port for gRPC communication, default - **26673**
- **--host** <*host*> - enable gRPC client and connect to the specified host
- **--mappers** <*directory*> - directory to search mapper scripts
- **--mapper** <*mapper*> - number, name or path to C# script of mapper for dumping, default - **0 (NROM)**
- **--file** <*output.nes*> - output/input filename (.nes, .fds, .png or .sav)
- **--prg-size** <*size*> - size of PRG memory to dump, you can use "K" or "M" suffixes
- **--chr-size** <*size*> - size of CHR memory to dump, you can use "K" or "M" suffixes
- **--battery** - set "battery" flag in ROM header after dumping
- **--cs-file** <*C#_file*> - execute C# script from file
- **--reset** - simulate reset first
- **--unif-name** <*name*> - internal ROM name for UNIF dumps
- **--unif-author** <*name*> - author of dump for UNIF dumps
- **--fds-sides** - number of FDS sides to dump (default 1)
- **--fds-no-header** - do not add header to output file during FDS dumping
- **--fds-dump-hidden** - try to dump hidden files during FDS dumping (used for some copy-protected games)
- **--bad-sectors** <*bad_sectors*> - comma separated list of bad sectors for COOLBOY/COOLGIRL writing
- **--ignore-bad-sectors** - ignore bad sectors while writing COOLBOY/COOLGIRL
- **--sound** - play sound when done or error occured
- **--verify** - verify COOLBOY/COOLGIRL/FDS after writing
- **--lock** - write-protect COOLBOY/COOLGIRL sectors after writing


## Mapper script files
Mapper script files are stored in the "mappers" directory. By default it's:
- <*current_directory*>/mappers
- /usr/share/famicom-dumper/mappers (on *nix systems)

Also you can specify it using **--mappers** option.

When you specify a mapper number or name, the application compiles the scripts to find a matching one.

Mapper scripts are written in C# language. Each script must contain class (any name allowed) that impliments [IMapper](FamicomDumper/IMapper.cs) interface.
```C#
    public interface IMapper
    {
        /// <summary>
        /// Name of the mapper
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Number of the mapper to spore in the iNES header (-1 if none)
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Number of submapper (0 if none)
        /// </summary>
        byte Submapper { get; }

        /// <summary>
        /// Name of the mapper to store in UNIF container (null if none)
        /// </summary>
        string UnifName { get; }

        /// <summary>
        /// Default PRG size to dump (in bytes)
        /// </summary>
        int DefaultPrgSize { get; }

        /// <summary>
        /// Default CHR size to dump (in bytes)
        /// </summary>
        int DefaultChrSize { get; }

        /// <summary>
        /// This method will be called to dump PRG
        /// </summary>
        /// <param name="dumper">FamicomDumperConnection object to access cartridge</param>
        /// <param name="data">This list must be filled with dumped PRG data</param>
        /// <param name="size">Size of PRG to dump requested by user (in bytes)</param>
        void DumpPrg(IFamicomDumperConnection dumper, List<byte> data, int size = 0);

        /// <summary>
        /// This method will be called to dump CHR
        /// </summary>
        /// <param name="dumper">FamicomDumperConnection object to access cartridge</param>
        /// <param name="data">This list must be filled with dumped CHR data</param>
        /// <param name="size">Size of CHR to dump requested by user (in bytes)</param>
        void DumpChr(IFamicomDumperConnection dumper, List<byte> data, int size = 0);

        /// <summary>
        /// This method will be called to enable PRG RAM
        /// </summary>
        /// <param name="dumper"></param>
        void EnablePrgRam(IFamicomDumperConnection dumper);

        /// <summary>
        /// This method must return mirroring type, it can call dumper.GetMirroring() if it's fixed
        /// </summary>
        /// <param name="dumper">FamicomDumperConnection object to access cartridge</param>
        /// <returns></returns>
        NesFile.MirroringType GetMirroring(IFamicomDumperConnection dumper);
    }
```

FamicomDumperConnection implements [IFamicomDumperConnection](FamicomDumperConnection/IFamicomDumperConnection.cs) interface:
```C#
    public interface IFamicomDumperConnection : IDisposable
    {
        /// <summary>
        /// Simulate reset (M2 goes to Z-state for a second)
        /// </summary>
        void Reset();

        /// <summary>
        /// Read single byte from CPU (PRG) bus
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>Data from CPU (PRG) bus</returns>
        byte ReadCpu(ushort address);
        
        /// <summary>
        /// Read data from CPU (PRG) bus
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <returns>Data from CPU (PRG) bus</returns>
        byte[] ReadCpu(ushort address, int length);

        /// <summary>
        /// Read single byte from PPU (CHR) bus
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>Data from PPU (CHR) bus</returns>
        byte ReadPpu(ushort address);

        /// <summary>
        /// Read data from PPU (CHR) bus
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <returns>Data from PPU (CHR) bus</returns>
        byte[] ReadPpu(ushort address, int length);

        /// <summary>
        /// Write data to CPU (PRG) bus
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="data">Data to write, address will be incremented after each byte</param>
        void WriteCpu(ushort address, params byte[] data);

        /// <summary>
        /// Write data to PPU (CHR) bus
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="data">Data to write, address will be incremented after each byte</param>
        void WritePpu(ushort address, params byte[] data);

        /// <summary>
        /// Write blocks to Famicom Disk System card
        /// </summary>
        /// <param name="blockNumbers">Block numbers to write (zero-based)</param>
        /// <param name="blocks">Raw blocks data</param>
        void WriteFdsBlocks(byte[] blockNumbers, byte[][] blocks);

        /// <summary>
        /// Write single block to Famicom Disk System card
        /// </summary>
        /// <param name="blockNumbers">Block number to write (zero-based)</param>
        /// <param name="block">Block data</param>
        void WriteFdsBlocks(byte blockNumber, byte[] block);

        /// <summary>
        /// Read raw mirroring values (CIRAM A10 pin states for different states of PPU A10 and A11)
        /// </summary>
        /// <returns>Values of CIRAM A10 pin for $2000-$23FF, $2400-$27FF, $2800-$2BFF and $2C00-$2FFF</returns>
        bool[] GetMirroringRaw();

        /// <summary>
        /// Read decoded current mirroring mode
        /// </summary>
        /// <returns>Current mirroring</returns>
        NesFile.MirroringType GetMirroring();
    }
```

Check "mappers" directory for examples.


## Other scripts

You can run custom C# scripts to interact with dumper and cartridge. It's usefull for reverse engineering. Each script must contain class (any name allowed) that contains **void Run(IFamicomDumperConnection dumper)** method. This method will be executed if --csfile option is specified. Also you can use [NesFile](https://github.com/ClusterM/nes-containers/blob/master/NesFile.cs) and [UnifFile](https://github.com/ClusterM/nes-containers/blob/master/UnifFile.cs) containers.

You can run script alone like this:
```
famicom-dumper script --csfile DemoScript.cs
```
Or execute it before main action like this:
```
famicom-dumper dump --mapper MMC3 --file game.nes --csfile DemoScript.cs
```

So you can write your own code to interact with dumper object and read/write data from/to cartridge. This dumper object can be even on another PC (read below)! Check "scripts" directory for example scripts.


## gRPC

You can start this application as gRPC server on one PC:
```
famicom-dumper server --port COM14
```

And dump cartridge over network using another PC:
```
famicom-dumper dump --mapper CNROM --file game.nes --host example.com
```

It's useful if you want to reverse engineer cartridge of your remote friend. You can use all commands and scripts to interact with remote dumper just like it's connected locally.

Also you can use gRPC to access dumper from other applications or your own code written in C#, C++, Dart, Go, Java, Kotlin, Node, Objective-C, PHP, Python, Ruby, etc. languages. Use [Dumper.proto](https://github.com/ClusterM/famicom-dumper-client/blob/master/FamicomDumperConnection/Dumper.proto) to automatically generate client code.


## Examples

Dump NROM-cartridge using dumper on port "COM14" to file "game.nes". PRG and CHR sizes are default.
~~~~
 > famicom-dumper dump --port COM14 --mapper nrom --file game.nes
 Dumper initialization... OK
 Using mapper: #0 (NROM)
 Dumping...
 PRG memory size: 32K
 Dumping PRG... OK
 CHR memory size: 8K
 Dumping CHR... OK
 Mirroring: Horizontal
 Saving to game.nes... OK
~~~~

Dump MMC1-cartridge (iNES mapper #1) using dumper with serial number "A9Z1A0WD". PRG size is 128 kilobytes, CHR size is 128 kilobytes too.
~~~~
>famicom-dumper dump --port A9Z1A0WD --mapper 1 --prg-size 128K --chr-size 128K --file game.nes
Dumper initialization... OK
Using mapper: #1 (MMC1)
Dumping...
PRG memory size: 128K
Reading PRG bank #0... OK
Reading PRG bank #1... OK
Reading PRG bank #2... OK
Reading PRG bank #3... OK
...
Saving to game.nes... OK
~~~~

Dump 32K of PRG and 8K of CHR as simple NROM cartridge but execute C# script first:
~~~~
>famicom-dumper dump --port COM14 --mapper 0 --prg-size 32K --chr-size 8K --file game.nes --csfile init.cs"
Dumper initialization... OK
Compiling init.cs...
Running init.Run()...
Dumping...
PRG memory size: 32K
Dumping PRG... OK
CHR memory size: 8K
Dumping CHR... OK
Mirroring: Horizontal
Saving to game.nes... OK
~~~~

Dump 32MBytes of COOLBOY cartridge using C# script and save it as UNIF file with some extra info:
~~~~
>famicom-dumper dump --port COM14 --mapper mappers\coolboy.cs --prg-size 32M --file coolboy.unf --unifname "COOLBOY 400-IN-1" --unifauthor "John Smith"
Dumper initialization... OK
Using mapper: COOLBOY
Dumping...
PRG memory size: 32768K
Reading PRG banks #0/0 and #0/1...
Reading PRG banks #0/2 and #0/3...
Reading PRG banks #0/4 and #0/5...
...
Saving to coolboy.unf... OK
~~~~

Read battery-backed save from MMC1 cartridge:
~~~~
>famicom-dumper read-prg-ram --port COM14 --mapper mmc1 --file "zelda.sav"
Dumper initialization... OK
Using mapper: #1 (MMC1)
Reading PRG-RAM... OK
~~~~

Write battery-backed save back to MMC1 cartridge:
~~~~
>famicom-dumper write-prg-ram --port COM14 --mapper mmc1 --file "zelda_hacked.sav"
Dumper initialization... OK
Using mapper: #1 (MMC1)
Writing PRG-RAM... OK
~~~~

Rewrite ultracheap chinese COOLBOY cartridge and play sound when it's done:
~~~~
>famicom-dumper write-coolboy --port COM14 --file "CoolBoy 400-in-1 (Alt Version, 403 games)(Unl)[U][!].nes" --sound
Dumper initialization... OK
Reset... OK
Erasing sector... OK
Writing 1/2048 (0%, 00:00:02/00:40:53)...
...
~~~~

Also you can write [COOLGIRL](https://github.com/ClusterM/coolgirl-famicom-multicard) cartridges:
~~~~
>famicom-dumper write-coolgirl --file multirom.unf --port COM14
Dumper initialization... OK
Reset... OK
Erasing sector... OK
Writing bank #0/114 (0%, 00:00:02/00:00:02)... OK
Writing bank #1/114 (0%, 00:00:02/00:00:02)... OK
Writing bank #2/114 (1%, 00:00:02/00:00:02)... OK
Writing bank #3/114 (2%, 00:00:02/00:00:02)... OK
Erasing sector... OK
Writing bank #4/114 (3%, 00:00:03/00:00:29)... OK
...
~~~~

Dump two-sided Famicom Disk System card:
~~~~
>famicom-dumper dump-fds --fds-sides 2 --file game.fds
Autodetected virtual serial port: COM13
Dumper initialization... OK
Reading disk... Done
Disk info block:
 Game name: MET
 Manufacturer code: Nintendo
 Game type: normal disk
 Game version: 2
 Disk number: 0
 Disk side: A
 Actual disk side: $FF
 Disk type: FMS
 Manufacturing date: 1986.09.09
 Country code: Japan
 Disk writer serial number: $0961
 Disk rewrite count: 0
 Price code: $FF
Number of non-hidden files: 15
...
Please remove disk card... OK
Please set disk card, side #2...
...
Saing to game.fds... OK
~~~~

Write Famicom Disk System card and verify written data:
~~~
>famicom-dumper write-fds --verify --file "Super Mario Brothers 2 (Japan).fds"
Autodetected virtual serial port: COM6
Dumper initialization... OK
Please set disk card, side #1... OK
Disk info block:
 Game name: SMB
 Manufacturer code: Nintendo
 Game type: normal disk
 Game version: 0
 Disk number: 0
 Disk side: A
 Actual disk side: A
 Disk type: FMS
 Manufacturing date: 1986.07.23
 Country code: Japan
 Disk writer serial number: $FFFF
 Disk rewrite count: 0
 Price code: $00
Number of non-hidden files: 8
Number of hidden files: 0
Total blocks to write: 18
Writing block(s): 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17... OK
Starting verification process
Reading disk... Done
Verification successful.
~~~

Start server on TCP port 9999 and let other person to dump cartridge over network:
~~~~
>famicom-dumper server --tcp-port 9999
Autodetected virtual serial port: COM13
Dumper initialization... OK
Listening port 9999, press Ctrl-C to stop
~~~~

Connect to remote dumper using TCP port 9999 and execute C# script:
~~~~
famicom-dumper script --csfile DemoScript.cs --host clusterrr.com --tcp-port 9999
Dumper initialization... OK
Compiling DemoScript.cs...
Running DemoScript.Run()...
~~~~

## Download

You can always download latest version at [https://github.com/ClusterM/famicom-dumper-client/releases](https://github.com/ClusterM/famicom-dumper-client/releases)


## Donation and contact

E-mail and PayPal: clusterrr@clusterrr.com
