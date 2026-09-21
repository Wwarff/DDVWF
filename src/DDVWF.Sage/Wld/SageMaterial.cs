namespace DDVWF.Sage.Wld;

public enum SageShaderType { Diffuse,Transparent25,Transparent50,Transparent75,TransparentAdditive,TransparentAdditiveUnlit,TransparentMasked,DiffuseSkydome,TransparentSkydome,TransparentAdditiveUnlitSkydome,Invisible,Boundary }

public static class SageMaterial
{
    public static SageShaderType Map(uint parameters,int bitmapReferenceIndex)
    {
        var type=parameters & ~0x80000000u;
        return type switch {
            0x0=>SageShaderType.Boundary,
            0x53 or 0x4B or 0x03=>SageShaderType.Invisible,
            0x01 or 0x14 or 0x15 or 0x553 or 0x12 or 0x31 or 0x02 or 0x1A or 0x07=>SageShaderType.Diffuse,
            0x09=>SageShaderType.Transparent25, 0x05=>SageShaderType.Transparent50, 0x0A=>SageShaderType.Transparent75,
            0x17=>SageShaderType.TransparentAdditive, 0x0B=>SageShaderType.TransparentAdditiveUnlit,
            0x13 or 0x19=>SageShaderType.TransparentMasked, 0x0D=>SageShaderType.DiffuseSkydome,
            0x0F=>SageShaderType.TransparentSkydome, 0x10=>SageShaderType.TransparentAdditiveUnlitSkydome,
            _=>bitmapReferenceIndex==0 ? SageShaderType.Invisible : SageShaderType.Diffuse
        };
    }
}
