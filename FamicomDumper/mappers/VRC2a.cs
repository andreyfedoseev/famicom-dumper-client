﻿class VRC2a : IMapper
{
    public string Name { get => "VRC2a"; }
    public int Number { get => 22; }
    public byte Submapper { get => 0; }
    public string UnifName { get => null; }
    public int DefaultPrgSize { get => 256 * 1024; }
    public int DefaultChrSize { get => 256 * 1024; }

    public void DumpPrg(IFamicomDumperConnection dumper, List<byte> data, int size)
    {
        var banks = size / 0x2000;

        dumper.WriteCpu(0x9001, 0); // disable swap mode
        for (var bank = 0; bank < banks; bank++)
        {
            Console.Write($"Reading PRG bank #{bank}/{banks}... ");
            dumper.WriteCpu(0x8000, (byte)bank); // PRG Select 0
            data.AddRange(dumper.ReadCpu(0x8000, 0x2000));
            Console.WriteLine("OK");
        }
    }

    public void DumpChr(IFamicomDumperConnection dumper, List<byte> data, int size)
    {
        var banks = size / 0x400;

        for (var bank = 0; bank < banks; bank++)
        {
            Console.Write($"Reading CHR bank #{bank}/{banks}... ");
            dumper.WriteCpu(0xB000, (byte)((bank >> 1) & 0x0F)); // CHR Select 0 low
            dumper.WriteCpu(0xB002, (byte)(bank >> 5)); // CHR Select 0 high
            data.AddRange(dumper.ReadPpu(0x0000, 0x0400));
            Console.WriteLine("OK");
        }
    }

    public void EnablePrgRam(IFamicomDumperConnection dumper)
    {
        throw new NotSupportedException("PRG RAM is not supported by this mapper");
    }

    public MirroringType GetMirroring(IFamicomDumperConnection dumper)
    {
        return MirroringType.MapperControlled;
    }
}
