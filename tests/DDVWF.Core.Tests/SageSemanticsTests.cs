using DDVWF.Sage.Wld;
using DDVWF.Core.Zone;
namespace DDVWF.Core.Tests;

public sealed class SageSemanticsTests
{
    [Fact] public void Location_matches_Sage_axis_rotation_semantics()
    {
        var l=SageLocation.FromSource(1,2,3,128,64,999);
        Assert.Equal(1,l.X); Assert.Equal(2,l.Y); Assert.Equal(3,l.Z);
        Assert.Equal(0,l.RotateX); Assert.Equal(-90,l.RotateY); Assert.Equal(45,l.RotateZ);
    }

    [Theory]
    [InlineData(0x0,SageShaderType.Boundary)]
    [InlineData(0x01,SageShaderType.Diffuse)]
    [InlineData(0x05,SageShaderType.Transparent50)]
    [InlineData(0x09,SageShaderType.Transparent25)]
    [InlineData(0x0A,SageShaderType.Transparent75)]
    [InlineData(0x13,SageShaderType.TransparentMasked)]
    [InlineData(0x17,SageShaderType.TransparentAdditive)]
    [InlineData(0x53,SageShaderType.Invisible)]
    public void Material_map_matches_Sage(uint value,SageShaderType expected)=>Assert.Equal(expected,SageMaterial.Map(value,1));
    [Theory]
    [InlineData("wt_water",RegionSemantic.Water)]
    [InlineData("lan_lava",RegionSemantic.Lava)]
    [InlineData("drp_pvp",RegionSemantic.Pvp)]
    [InlineData("sln_block",RegionSemantic.WaterBlockLos)]
    [InlineData("vwn_cold",RegionSemantic.FreezingWater)]
    public void Region_tags_match_Sage_semantics(string tag,RegionSemantic expected)
        => Assert.Contains(expected,WldBspReader.ClassifyTag(tag).Semantics);

    [Fact] public void Referenced_zoneline_matches_Sage_encoding()
    {
        var data=WldBspReader.ClassifyTag("drntp00255000042");
        Assert.Contains(RegionSemantic.Zoneline,data.Semantics);
        Assert.Equal(42,data.ZoneLineReference);
    }
    [Fact] public void Light_source_preserves_Sage_frame_state_levels_and_colors()
    {
        using var ms=new MemoryStream();using var bw=new BinaryWriter(ms);
        bw.Write((uint)0x17);bw.Write((uint)1);bw.Write((uint)0);bw.Write((uint)250);bw.Write(0.75f);bw.Write(0.1f);bw.Write(0.2f);bw.Write(0.3f);
        var s=WldLightReader.ReadSource(new WldFragment(0,(uint)ms.Length,(uint)WldFragmentType.LightSource,"",0),ms.ToArray());
        Assert.Equal((uint)250,s.Sleep);Assert.Equal(0.75f,s.Levels[0]);Assert.Equal(0.2f,s.Colors[0].Y);
    }

    [Fact] public void Regional_ambient_matches_Sage_flags_count_regions_payload()
    {
        using var ms=new MemoryStream();using var bw=new BinaryWriter(ms);
        bw.Write((uint)0x40);bw.Write((uint)3);bw.Write(2);bw.Write(7);bw.Write(11);
        var a=WldLightReader.ReadAmbient(new WldFragment(0,(uint)ms.Length,(uint)WldFragmentType.AmbientLight,"",0),ms.ToArray());
        Assert.Equal((uint)0x40,a.Flags);Assert.Equal(new[]{2,7,11},a.Regions);
    }

    [Fact] public void Global_ambient_preserves_Sage_bgra_byte_order()
    {
        var g=WldLightReader.ReadGlobalAmbient(new WldFragment(0,4,(uint)WldFragmentType.GlobalAmbientLight,"",0),new byte[]{3,2,1,4});
        Assert.Equal((byte)1,g.Red);Assert.Equal((byte)2,g.Green);Assert.Equal((byte)3,g.Blue);Assert.Equal((byte)4,g.Alpha);
    }

    [Theory]
    [InlineData(0x09,SageShaderType.Transparent25)]
    [InlineData(0x05,SageShaderType.Transparent50)]
    [InlineData(0x0A,SageShaderType.Transparent75)]
    [InlineData(0x17,SageShaderType.TransparentAdditive)]
    [InlineData(0x0B,SageShaderType.TransparentAdditiveUnlit)]
    public void Transparent_shader_kinds_remain_distinct_for_Babylon_consumption(uint value,SageShaderType expected)
        => Assert.Equal(expected,SageMaterial.Map(value,1));

    [Theory]
    [InlineData(1,0,99,0,"hum")]
    [InlineData(1,1,42,0,"huf")]
    [InlineData(24,2,1,0,"fis")]
    [InlineData(1,0,99,12,"hum01")]
    public void Npc_model_selection_matches_pinned_Sage_race_gender_then_texture_variation(int race,int gender,int npcModel,int texture,string expected)
    {
        var provider=new DDVWF.Sage.SageClientZoneProvider();
        Assert.Equal(expected,provider.ResolveNpcModelName(race,gender,npcModel,texture));
    }

    [Fact] public void Bsp_split_and_combined_region_semantics_match_pinned_Sage_contract()
    {
        using var ms=new MemoryStream();using var bw=new BinaryWriter(ms);
        bw.Write((uint)1);bw.Write(1f);bw.Write(0f);bw.Write(0f);bw.Write(0f);bw.Write(1);bw.Write(0);bw.Write(0);
        var node=WldBspReader.ReadTree(new WldFragment(0,(uint)ms.Length,(uint)WldFragmentType.BspTree,"",0),ms.ToArray()).Single();
        Assert.Equal(new System.Numerics.Vector3(1,0,0),node.Normal);Assert.Equal(1,node.RegionId);
        var classified=WldBspReader.ClassifyTag("wtntp00255000042");Assert.Equal(new[]{RegionSemantic.Water,RegionSemantic.Zoneline},classified.Semantics);Assert.Equal(42,classified.ZoneLineReference);
    }

}

