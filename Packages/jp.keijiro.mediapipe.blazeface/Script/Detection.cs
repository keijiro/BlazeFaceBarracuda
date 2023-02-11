using System.Runtime.InteropServices;
using UnityEngine;

namespace MediaPipe.BlazeFace {

//
// Detection structure. The layout of this structure must be matched with
// the one defined in Common.hlsl.
//
[StructLayout(LayoutKind.Sequential)]
public readonly struct Detection
{
    // Bounding box
    public readonly Vector2 center;
    public readonly Vector2 extent;

    // Key points
    public readonly Vector2 leftEye;
    public readonly Vector2 rightEye;
    public readonly Vector2 nose;
    public readonly Vector2 mouth;
    public readonly Vector2 leftEar;
    public readonly Vector2 rightEar;

    // Confidence score [0, 1]
    public readonly float score;

    // Padding
    public readonly float pad1, pad2, pad3;

    // sizeof(Detection)
    public const int Size = 20 * sizeof(float);

    // Maximum number of detections
    // This value must be matched with MAX_DETECTION in Common.hlsl.
    public const int Max = 64;
}

} // namespace MediaPipe.BlazeFace
