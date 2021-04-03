using System.Runtime.InteropServices;
using UnityEngine;

namespace BlazeFace {

//
// Bounding box structure - The layout of this structure must be matched
// with the one defined in Common.hlsl.
//
[StructLayout(LayoutKind.Sequential)]
public readonly struct BoundingBox
{
    public readonly Vector2 center;
    public readonly Vector2 extent;
    public readonly Vector2 rightEye;
    public readonly Vector2 leftEye;
    public readonly Vector2 nose;
    public readonly Vector2 mouth;
    public readonly Vector2 rightEar;
    public readonly Vector2 leftEar;
    public readonly float score;
    public readonly float pad1, pad2, pad3;

    // sizeof(BoundingBox)
    public static int Size = 20 * sizeof(float);
};

} // namespace BlazeFace
