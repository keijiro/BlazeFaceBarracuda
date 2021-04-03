#ifndef _BLAZEFACEBARRACUDA_COMMON_H_
#define _BLAZEFACEBARRACUDA_COMMON_H_

#define MAX_DETECTION 64

// Bounding box structure used for storing object detection results
struct BoundingBox
{
    float2 center;
    float2 extent;
    float2 keyPoint0;
    float2 keyPoint1;
    float2 keyPoint2;
    float2 keyPoint3;
    float2 keyPoint4;
    float2 keyPoint5;
    float score;
    float3 pad;
};

// Common math functions

float CalculateIOU(BoundingBox box1, BoundingBox box2)
{
    float x0 = max(box1.center.x - box1.extent.x / 2, box2.center.x - box2.extent.x / 2);
    float x1 = min(box1.center.x + box1.extent.x / 2, box2.center.x + box2.extent.x / 2);
    float y0 = max(box1.center.y - box1.extent.y / 2, box2.center.y - box2.extent.y / 2);
    float y1 = min(box1.center.y + box1.extent.y / 2, box2.center.y + box2.extent.y / 2);

    float area0 = box1.extent.x * box1.extent.y;
    float area1 = box2.extent.x * box2.extent.y;
    float areaInner = max(0, x1 - x0) * max(0, y1 - y0);

    return areaInner / (area0 + area1 - areaInner);
}

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
