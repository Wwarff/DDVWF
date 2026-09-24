using DDVWF.Sage.Wld;
namespace DDVWF.Core.Tests;
public sealed class WldMeshTests
{
 [Fact] public void Fragment_type_36_is_mesh()=>Assert.Equal(WldFragmentType.Mesh,(WldFragmentType)0x36u);
 [Fact] public void Old_and_new_wld_versions_are_distinct(){Assert.NotEqual(0x00015500u,0x1000C800u);}

    [Fact]
    public void MeshReader_UsesSageReferenceAndExponentSemantics()
    {
        var payload=new byte[160]; var p=0;
        void U32(uint v){System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(p,4),v);p+=4;}
        void I16(short v){System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(p,2),v);p+=2;}
        void F(float v){U32(unchecked((uint)BitConverter.SingleToInt32Bits(v)));}
        U32(0); U32(uint.MaxValue); U32(uint.MaxValue); p+=8;
        F(0);F(0);F(0); p+=12; F(0); for(var i=0;i<6;i++)F(0);
        I16(1); for(var i=0;i<8;i++)I16(0); I16(-1); I16(1);I16(2);I16(3);
        var doc=new WldDocument{Name="x.wld",StringTable=Array.Empty<byte>(),Fragments=Array.Empty<WldFragment>()};
        var mesh=WldMeshReader.Read(doc,new WldFragment(0,(uint)payload.Length,(uint)WldFragmentType.Mesh,"",0),payload);
        Assert.Equal(-2,mesh.MaterialListIndex); Assert.Equal(-2,mesh.AnimatedVerticesReferenceIndex);
        Assert.Equal(new System.Numerics.Vector3(2,4,6),mesh.Vertices[0]);
    }
}
