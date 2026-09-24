namespace DDVWF.Sage.Wld;

// Proven against EQ Sage v1.8.15 commit e85bb85a19670eb377d876521bb1adf130b4e83a,
// src/lib/s3d/wld/wld.js. Unknown fragment types remain unknown instead of being guessed.
public enum WldFragmentType : uint
{
    BitmapName = 0x03, BitmapInfo = 0x04, FragmentReference = 0x05,
    // 0x06-0x09 are not dispatched by the pinned Sage Wld.processFragment implementation; leave them unknown.
    SkeletonHierarchy = 0x10, SkeletonHierarchyReference = 0x11, TrackDefinition = 0x12, TrackReference = 0x13,
    ActorDefinition = 0x14, ActorInstance = 0x15, ZoneUnknown = 0x16,
    LightSource = 0x1B, LightSourceReference = 0x1C,
    BspTree = 0x21, BspRegion = 0x22, ParticleSprite = 0x26, ParticleSpriteReference = 0x27,
    LightInstance = 0x28, RegionType = 0x29, AmbientLight = 0x2A, LegacyMesh = 0x2C,
    MeshReference = 0x2D, AnimatedVertexReference = 0x2F, Material = 0x30, MaterialList = 0x31,
    VertexColor = 0x32, VertexColorReference = 0x33, ParticleCloud = 0x34, GlobalAmbientLight = 0x35,
    Mesh = 0x36, AnimatedVertices = 0x37
}

public enum WldKind { Zone, ZoneObjects, Lights, Objects, Sky, Characters, Equipment }

public static class WldClassification
{
    public static WldKind Classify(string name)
    {
        var n = name.ToLowerInvariant();
        if (n == "lights.wld") return WldKind.Lights;
        if (n == "objects.wld") return WldKind.ZoneObjects;
        if (n == "sky.wld") return WldKind.Sky;
        if (n.EndsWith("_obj.wld")) return WldKind.Objects;
        if (n.EndsWith("_chr.wld")) return WldKind.Characters;
        if (n.StartsWith("gequip") || n.EndsWith("_amr.s3d")) return WldKind.Equipment;
        return WldKind.Zone;
    }
}
