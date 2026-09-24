using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace DDVWF.Sage.Pfs;

// Read path derived from EQ Sage v1.8.15 src/lib/pfs/pfs.js.
public sealed class PfsArchive
{
    private const int FilenameDirectoryCrc = 0x61580ac9;
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, byte[]> Files => _files;

    public static PfsArchive Open(ReadOnlySpan<byte> source)
    {
        var data=source.ToArray();
        if(data.Length<12) throw new InvalidDataException("PFS archive is smaller than its header.");
        var dirOffset=checked((int)U32(data,0));
        if(Encoding.ASCII.GetString(data,4,4)!="PFS ") throw new InvalidDataException("PFS magic mismatch.");
        if(dirOffset<0 || dirOffset+4>data.Length) throw new InvalidDataException("PFS directory offset is outside the archive.");
        var count=checked((int)U32(data,dirOffset));
        var entries=new List<Entry>(count); var filenameEntries=new List<(int Crc,string Name)>();
        for(var i=0;i<count;i++)
        {
            var p=checked(dirOffset+4+i*12); if(p+12>data.Length) throw new InvalidDataException("PFS directory is truncated.");
            var e=new Entry(I32(data,p),checked((int)U32(data,p+4)),checked((int)U32(data,p+8)));
            if(e.Crc!=FilenameDirectoryCrc){entries.Add(e);continue;}
            var filenameData=InflateFile(data,e.Offset,e.Size); var fp=0; var filenameCount=checked((int)ReadU32(filenameData,ref fp));
            for(var j=0;j<filenameCount;j++){var len=checked((int)ReadU32(filenameData,ref fp));if(len<1||fp+len>filenameData.Length)throw new InvalidDataException("PFS filename entry is invalid.");var name=Encoding.ASCII.GetString(filenameData,fp,len-1).ToLowerInvariant();fp+=len;filenameEntries.Add((PfsCrc.Get(name),name));}
        }
        if(filenameEntries.Count==0) throw new InvalidDataException("PFS filename directory is absent.");
        var byCrc=filenameEntries.GroupBy(x=>x.Crc).ToDictionary(g=>g.Key,g=>g.First().Name);
        var archive=new PfsArchive();
        foreach(var e in entries)
            if(byCrc.TryGetValue(e.Crc,out var name)) archive._files[name]=InflateFile(data,e.Offset,e.Size);
        if(archive._files.Count==0 && entries.Count>0) throw new InvalidDataException($"PFS filename directory resolved 0 of {entries.Count} file entries.");
        return archive;
    }

    public byte[]? GetFile(string name)=>_files.TryGetValue(name,out var data)?data:null;

    private static byte[] InflateFile(byte[] data,int offset,int size)
    {
        if(offset<0 || offset>=data.Length) throw new InvalidDataException("PFS block offset is outside archive.");
        using var output=new MemoryStream(size); var p=offset;
        while(output.Length<size)
        {
            if(p+8>data.Length) throw new InvalidDataException("PFS block header is truncated.");
            var compressed=checked((int)U32(data,p)); var inflated=checked((int)U32(data,p+4)); p+=8;
            if(compressed<0 || inflated<0 || p+compressed>data.Length) throw new InvalidDataException("PFS block is invalid.");
            using var input=new MemoryStream(data,p,compressed,false);
            using var zlib=new ZLibStream(input,CompressionMode.Decompress);
            var before=output.Length; zlib.CopyTo(output);
            if(output.Length-before!=inflated) throw new InvalidDataException("PFS zlib block inflated to an unexpected size.");
            p+=compressed;
        }
        if(output.Length!=size) throw new InvalidDataException("PFS file inflated to an unexpected size.");
        return output.ToArray();
    }
    private static uint U32(byte[] b,int p)=>BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));
    private static int I32(byte[] b,int p)=>BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(p,4));
    private static uint ReadU32(byte[] b,ref int p){if(p+4>b.Length)throw new InvalidDataException("PFS data is truncated.");var v=U32(b,p);p+=4;return v;}
    private readonly record struct Entry(int Crc,int Offset,int Size);
}

public static class PfsCrc
{
    private const uint Polynomial=0x04C11DB7;
    private static readonly int[] Table=Build();
    public static int Get(string text)
    {
        if(text.Length==0)return 0;
        var bytes=Encoding.ASCII.GetBytes(text+"\0"); int crc=0;
        foreach(var b in bytes){var index=((crc>>24)^b)&0xff;crc=unchecked((crc<<8)^Table[index]);}
        return crc;
    }
    private static int[] Build()
    {
        var table=new int[256];
        for(var i=0;i<256;i++){var c=unchecked(i<<24);for(var j=0;j<8;j++)c=unchecked((c&int.MinValue)!=0?(c<<1)^(int)Polynomial:c<<1);table[i]=c;}
        return table;
    }
}
