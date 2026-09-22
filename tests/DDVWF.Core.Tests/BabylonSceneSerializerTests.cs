using System.Numerics;using DDVWF.Core.Zone;using DDVWF.Renderer.Babylon;
namespace DDVWF.Core.Tests;
public sealed class BabylonSceneSerializerTests
{
 [Fact]public void Serializer_emits_only_resolved_render_assets(){var z=new CompleteZone{ShortName="z"};var id=Guid.NewGuid();z.Add(new(id,ZoneEntityKind.WorldGeometry,"m",new(1,2,3),1,null,"w#m"));z.Add(new(Guid.NewGuid(),ZoneEntityKind.StaticObject,"missing",new(),1,null,"missing"));var asset=new RenderMesh("m",[new("p",[new(new(1,2,3),Vector3.UnitY,Vector2.Zero)],[0],"t.dds",false)],true);var json=BabylonSceneSerializer.Serialize(z,new Dictionary<string,RenderMesh>{{"w#m",asset}});Assert.Contains(id.ToString(),json);Assert.DoesNotContain("missing",json);Assert.Contains("t.dds",json);}
}
