#ifndef _BLAZEFACEBARRACUDA_COMMON_H_
#define _BLAZEFACEBARRACUDA_COMMON_H_

#define MAX_DETECTION 64

// Bounding box structure used for storing object detection results
struct BoundingBox
{
    float2 center;
    float2 extent;
    float2 keyPoints[6];
    float score;
    float3 pad;
};

// Common math functions

float CalculateIOU(in BoundingBox box1, in BoundingBox box2)
{
    float2 p0 = max(box1.center - box1.extent / 2, box2.center - box2.extent / 2);
    float2 p1 = min(box1.center + box1.extent / 2, box2.center + box2.extent / 2);

    float area0 = box1.extent.x * box1.extent.y;
    float area1 = box2.extent.x * box2.extent.y;
    float areaInner = max(0, p1.x - p0.x) * max(0, p1.y - p0.y);

    return areaInner / (area0 + area1 - areaInner);
}

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
