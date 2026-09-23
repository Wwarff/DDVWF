using System.Buffers.Binary;
namespace DDVWF.Sage.Wld;

public sealed record SageActorInstance(string ObjectName,SageLocation? Location,float BoundingRadius,float ScaleFactor,string? SoundName,int? VertexColorReference,string UserData);

public static class WldActorReader
{
    public static SageActorInstance ReadActorInstance(WldDocument doc,WldFragment fragment,ReadOnlySpan<byte> file)
    {
        var bytes=file.ToArray(); var p=fragment.PayloadOffset;
        int I32(){var v=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(p));p+=4;return v;} uint U32()=>unchecked((uint)I32());
        float F32()=>BitConverter.Int32BitsToSingle(I32());
        var objectName=doc.ResolveString(I32()).Replace("_ACTORDEF","",StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
        var flags=I32(); _=U32(); SageLocation? location=null;
        if((flags&1)!=0) _=U32();
        if((flags&2)!=0){var x=F32();var y=F32();var z=F32();var rx=F32();var ry=F32();var rz=F32();location=SageLocation.FromSource(x,y,z,rx,ry,rz);_=U32();}
        var radius=(flags&4)!=0?F32():0f; var scale=(flags&8)!=0?F32():0f;
        string? sound=(flags&0x10)!=0?doc.ResolveString(I32()):null;
        int? vertex=(flags&0x100)!=0?unchecked((int)U32())-1:null;
        var len=checked((int)U32()); if(p+len>bytes.Length) throw new InvalidDataException("ActorInstance user data exceeds fragment/file bounds.");
        var user=System.Text.Encoding.Latin1.GetString(bytes.AsSpan(p,len));
        return new(objectName,location,radius,scale,sound,vertex,user);
    }
}
