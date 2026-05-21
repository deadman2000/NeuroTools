using System.Text;

namespace NeuroTools;

public static class Unpacker
{
    public static void Unpack(string gameDir)
    {
        var exePath = gameDir + @"\NEURO.EXE";
        var dat1Path = gameDir + @"\neuro1.dat";
        var dat2Path = gameDir + @"\neuro2.dat";
        var extractDir = gameDir + @"\extract";

        using var stream = File.OpenRead(exePath);

        var startOffset = 0x462c2;

        stream.Seek(startOffset, SeekOrigin.Begin);

        using (var dat = File.OpenRead(dat1Path))
        {
            ReadTable(stream, dat, extractDir + @"\NEURO1");
        }

        using (var dat = File.OpenRead(dat2Path))
        {
            ReadTable(stream, dat, extractDir + @"\NEURO2");
        }
    }

    static void ReadTable(Stream stream, FileStream datFile, string dir)
    {
        Directory.CreateDirectory(dir);
        var record = new byte[14 + 4 + 4];

        while (true)
        {
            stream.ReadExactly(record);

            var name = Encoding.ASCII.GetString(record, 0, 14).TrimEnd('\0');
            if (name.Length == 0)
                break;

            var offset = BitConverter.ToInt32(record, 14);
            var size = BitConverter.ToInt32(record, 14 + 4);

            datFile.Seek(offset, SeekOrigin.Begin);

            var data = new byte[size];
            datFile.ReadExactly(data);

            var destPath = dir + "\\" + name;
            //File.Delete(destPath);
            //File.WriteAllBytes(destPath, data);

            Console.WriteLine($"{name,-14}{offset,6}{size,6}");

            var ext = Path.GetExtension(name);
            if (IsEncoded(ext))
            {
                var dataOffset = HasHeader(ext) ? 32 : 0;
                data = HuffmanDecoder.Decode(data, dataOffset);
            }

            File.WriteAllBytes(destPath, data);

        }
    }

    private static bool HasHeader(string ext) => ext switch
    {
        ".PIC" or ".IMH" => true,
        _ => false
    };

    private static bool IsEncoded(string ext) => ext switch
    {
        ".BIN" or ".NMC" or ".SAV" => false,
        _ => true
    };
}
