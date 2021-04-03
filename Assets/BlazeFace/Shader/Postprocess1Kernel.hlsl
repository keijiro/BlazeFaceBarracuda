[numthreads(CELLS_IN_ROW, CELLS_IN_ROW, 1)]
void KERNEL_NAME(uint2 id : SV_DispatchThreadID)
{
    // We have to read the input tensor in the reversed order.
    uint base_y = (CELLS_IN_ROW - 1 - id.y) * CELLS_IN_ROW +
                  (CELLS_IN_ROW - 1 - id.x);
    base_y *= ANCHOR_COUNT;

    // Anchor point
    float2 anchor = (id + 0.5) / CELLS_IN_ROW;

    for (uint aidx = 0; aidx < ANCHOR_COUNT; aidx++)
    {
        uint ref_y = base_y + aidx;

        // Confidence score
        float score = Sigmoid(_Scores[uint2(0, ref_y)]);

        // Bounding box data
        float x = _Boxes[uint2(0, ref_y)];
        float y = _Boxes[uint2(1, ref_y)];
        float w = _Boxes[uint2(2, ref_y)];
        float h = _Boxes[uint2(3, ref_y)];

        // Output structure
        BoundingBox box;
        box.center = anchor + float2(x, y) / _ImageSize;
        box.extent = float2(w, h) / _ImageSize;
        box.score = score;
        box.pad = 0;

        // Key points
        [unroll] for (uint kidx = 0; kidx < 6; kidx++)
        {
            float kx = _Boxes[uint2(4 + 2 * kidx + 0, ref_y)];
            float ky = _Boxes[uint2(4 + 2 * kidx + 1, ref_y)];
            box.keyPoints[kidx] = anchor + float2(kx, ky) / _ImageSize;
        }

        // Thresholding
        if (score > _Threshold) _Output.Append(box);
    }
}
