using DDVWF.Sage.Wld;namespace DDVWF.Core.Tests;
public sealed class ActorDefinitionTests
{
 [Fact] public void Sage_actor_types_match_source_contract(){Assert.Equal(1,(int)SageActorType.Skeletal);Assert.Equal(2,(int)SageActorType.Static);}
}
