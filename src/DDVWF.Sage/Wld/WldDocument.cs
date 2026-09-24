using System.Buffers.Binary;
using System.Text;

namespace DDVWF.Sage.Wld;

public sealed record WldFragment(int Index, uint Size, uint RawType, string Name, int PayloadOffset, int EndOffset)
{
    public WldFragmentType? KnownType => Enum.IsDefined(typeof(WldFragmentType), RawType) ? (WldFragmentType)RawType : null;
}

public sealed class WldDocument
{
    private static readonly byte[] XorKey = [0x95,0x3A,0xC5,0x2A,0x95,0x7A,0x95,0x6A];
    public uint Identifier { get; private init; }
    public uint Version { get; private init; }
    public uint BspRegionCount { get; private init; }
    public required string Name { get; init; }
    public required byte[] StringTable { get; init; }
    public required IReadOnlyList<WldFragment> Fragments { get; init; }
    public bool IsNewFormat => Version == 0x1000C800;
    public bool IsOldS3D => Version == 0x00015500;
    public WldKind Kind => WldClassification.Classify(Name);

    public string ResolveString(int reference)
    {
        if (reference >= 0) return string.Empty;
        var start = -reference;
        if (start < 0 || start >= StringTable.Length) return string.Empty;
        var end = Array.IndexOf(StringTable, (byte)0, start);
        if (end < 0) end = StringTable.Length;
        return Encoding.Latin1.GetString(StringTable, start, end - start);
    }

    public static WldDocument Parse(ReadOnlySpan<byte> data, string name)
    {
        if (data.Length < 28) throw new InvalidDataException("WLD header is truncated.");
        var bytes = data.ToArray();
        var p = 0;
        uint U32(){var v=BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(p));p+=4;return v;}
        var identifier=U32(); var version=U32(); var fragmentCount=U32(); var bsp=U32();
        p += 4; var stringBytes=checked((int)U32()); p += 4;
        if (p + stringBytes > bytes.Length) throw new InvalidDataException("WLD string table is truncated.");
        var table=bytes.AsSpan(p,stringBytes).ToArray(); p += stringBytes;
        for(var i=0;i<table.Length;i++) table[i]^=XorKey[i%XorKey.Length];

        string Resolve(int reference)
        {
            if(reference>=0) return string.Empty;
            var start=-reference;
            if(start<0 || start>=table.Length) return string.Empty;
            var end=Array.IndexOf(table,(byte)0,start);
            if(end<0) end=table.Length;
            return Encoding.Latin1.GetString(table,start,end-start);
        }

        var fragments=new List<WldFragment>(checked((int)fragmentCount));
        for(var i=0;i<fragmentCount;i++)
        {
            if(p+12>bytes.Length) throw new InvalidDataException($"WLD fragment {i} header is truncated.");
            var size=BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(p)); p+=4;
            var type=BinaryPrimitives.ReadUInt32LittleEndian(data[p..]); p+=4;
            var original=p;
            var nameRef=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(p)); p+=4;
            var next=checked(original+(int)size);
            fragments.Add(new(i,size,type,Resolve(nameRef),p,next));
            if(next<p || next>bytes.Length) throw new InvalidDataException($"WLD fragment {i} exceeds file bounds.");
            p=next;
        }
        return new WldDocument { Identifier=identifier,Version=version,BspRegionCount=bsp,Name=name,StringTable=table,Fragments=fragments };
    }
}
