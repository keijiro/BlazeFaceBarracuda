#ifndef _BLAZEFACEBARRACUDA_COMMON_H_
#define _BLAZEFACEBARRACUDA_COMMON_H_

// Maximum number of detections. This value must be matched with MaxDetection
// in FaceDetector.cs.
#define MAX_DETECTION 64

// Detection structure: The layout of this structure must be matched with the
// one defined in Detection.cs
struct Detection
{
    float2 center;
    float2 extent;
    float2 keyPoints[6];
    float score;
    float3 pad;
};

// Common math functions

float2 VFlip(float2 p)
{
    return float2(p.x, 1 - p.y);
}

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

float CalculateIOU(in Detection d1, in Detection d2)
{
    float area0 = d1.extent.x * d1.extent.y;
    float area1 = d2.extent.x * d2.extent.y;

    float2 p0 = max(d1.center - d1.extent / 2, d2.center - d2.extent / 2);
    float2 p1 = min(d1.center + d1.extent / 2, d2.center + d2.extent / 2);
    float areaInner = max(0, p1.x - p0.x) * max(0, p1.y - p0.y);

    return areaInner / (area0 + area1 - areaInner);
}

#endif
