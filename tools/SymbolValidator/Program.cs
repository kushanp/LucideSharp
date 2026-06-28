using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

static (Guid Guid, int Age)? GetDllPdbId(string dllPath)
{
    using var fs = File.OpenRead(dllPath);
    using var pe = new PEReader(fs);
    var entry = pe.ReadDebugDirectory().FirstOrDefault(d => d.Type == DebugDirectoryEntryType.CodeView);
    if (entry.Type == 0)
    {
        return null;
    }

    var cv = pe.ReadCodeViewDebugDirectoryData(entry);
    return (cv.Guid, cv.Age);
}

static (Guid Guid, int Age)? GetPdbId(string pdbPath)
{
    using var fs = File.OpenRead(pdbPath);
    var reader = MetadataReaderProvider.FromPortablePdbStream(fs);
    var md = reader.GetMetadataReader();
    var id = md.DebugMetadataHeader.Id;
    if (id.Length < 20)
    {
        return null;
    }

    var guidBytes = id.Take(16).ToArray();
    var age = BitConverter.ToInt32(id.Skip(16).Take(4).ToArray(), 0);
    return (new Guid(guidBytes), age);
}

var temp = Path.Combine(Path.GetTempPath(), "lucide-validate");
Directory.CreateDirectory(temp);

var nupkg = args[0];
var snupkg = args[1];
var failed = false;

using (var zip = ZipFile.OpenRead(nupkg))
{
    foreach (var entry in zip.Entries.Where(x => x.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
    {
        var dllPath = Path.Combine(temp, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(dllPath)!);
        entry.ExtractToFile(dllPath, true);

        var pdbEntryName = entry.FullName.Replace(".dll", ".pdb");
        using var symbolZip = ZipFile.OpenRead(snupkg);
        var pdbEntry = symbolZip.GetEntry(pdbEntryName);
        if (pdbEntry is null)
        {
            Console.WriteLine($"MISSING PDB: {pdbEntryName}");
            failed = true;
            continue;
        }

        var pdbPath = Path.Combine(temp, pdbEntryName.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(pdbPath)!);
        pdbEntry.ExtractToFile(pdbPath, true);

        var dllId = GetDllPdbId(dllPath);
        var pdbId = GetPdbId(pdbPath);
        var match = dllId is not null
            && pdbId is not null
            && dllId.Value.Guid == pdbId.Value.Guid;
        Console.WriteLine($"{entry.FullName}: DLL={dllId?.Guid} PDB={pdbId?.Guid} Match={match}");
        if (!match)
        {
            failed = true;
        }
    }
}

return failed ? 1 : 0;