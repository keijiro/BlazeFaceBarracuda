#ifndef _BLAZEFACEBARRACUDA_COMMON_H_
#define _BLAZEFACEBARRACUDA_COMMON_H_

#define MAX_DETECTION (512 + 384)

// Bounding box structure used for storing detection results
struct BoundingBox
{
    float x, y, w, h;
    float score;
    float3 pad;
};

// Common math functions

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
