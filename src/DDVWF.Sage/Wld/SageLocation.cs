namespace DDVWF.Sage.Wld;

public readonly record struct SageLocation(float X,float Y,float Z,float RotateX,float RotateY,float RotateZ)
{
    // Exact EQ Sage v1.8.15 Location semantics. Source RotateZ is intentionally ignored by Sage.
    public static SageLocation FromSource(float x,float y,float z,float rotateX,float rotateY,float rotateZ)
    {
        const float modifier=360f/512f;
        return new(x,y,z,0f,-rotateX*modifier,rotateY*modifier);
    }
}
