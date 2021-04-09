//
// Function template for Postprocess1 (bounding box aggregation)
//
[numthreads(CELLS_IN_ROW, CELLS_IN_ROW, 1)]
void KERNEL_NAME(uint2 id : SV_DispatchThreadID)
{
    // Scale factor based on the input image size
    float scale = 1 / _ImageSize;

    // Corresponding row number in the input texture
    uint row0 = (id.y * CELLS_IN_ROW + id.x) * ANCHOR_COUNT;

    // Anchor point coordinates
    float2 anchor = (CELLS_IN_ROW - 0.5 - id) / CELLS_IN_ROW;

    for (uint ai = 0; ai < ANCHOR_COUNT; ai++)
    {
        Detection d;
        d.pad = 0;

        // Row number of this anchor
        uint row = row0 + ai;

        // Confidence score
        d.score = Sigmoid(_Scores[uint2(0, row)]);

        // Bounding box
        float x = _Boxes[uint2(0, row)];
        float y = _Boxes[uint2(1, row)];
        float w = _Boxes[uint2(2, row)];
        float h = _Boxes[uint2(3, row)];

        d.center = VFlip(anchor + float2(x, y) * scale);
        d.extent = float2(w, h) * scale;

        // Key points
        [unroll] for (uint ki = 0; ki < 6; ki++)
        {
            float kx = _Boxes[uint2(4 + 2 * ki + 0, row)];
            float ky = _Boxes[uint2(4 + 2 * ki + 1, row)];
            d.keyPoints[ki] = VFlip(anchor + float2(kx, ky) * scale);
        }

        // Thresholding
        if (d.score > _Threshold) _Output.Append(d);
    }
}
