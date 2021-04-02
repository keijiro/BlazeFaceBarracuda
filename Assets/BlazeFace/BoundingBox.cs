using System.Runtime.InteropServices;

namespace BlazeFace {

//
// Bounding box structure - The layout of this structure must be matched
// with the one defined in Common.hlsl.
//
[StructLayout(LayoutKind.Sequential)]
public readonly struct BoundingBox
{
    public readonly float x, y, w, h;
    public readonly float key0x, key0y;
    public readonly float key1x, key1y;
    public readonly float key2x, key2y;
    public readonly float key3x, key3y;
    public readonly float key4x, key4y;
    public readonly float key5x, key5y;
    public readonly float score;
    public readonly float pad1, pad2, pad3;

    // sizeof(BoundingBox)
    public static int Size = 24 * sizeof(float);

    public BoundingBox(float x, float y, float w, float h, float s)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        key0x = key0y = 0;
        key1x = key1y = 0;
        key2x = key2y = 0;
        key3x = key3y = 0;
        key4x = key4y = 0;
        key5x = key5y = 0;
        score = s;
        pad1 = pad2 = pad3 = 0;
    }
};

} // namespace BlazeFace
