//
// Function template for Postprocess1 (bounding box aggregation)
//
[numthreads(CELLS_IN_ROW, CELLS_IN_ROW, 1)]
void KERNEL_NAME(uint2 id : SV_DispatchThreadID)
{
    // Scale factor based on the input image size
    float scale = 1 / _ImageSize;

    // Anchor point coordinates
    float2 anchor = (id + 0.5) / CELLS_IN_ROW;

    // Array indices
    uint sidx = (id.y * CELLS_IN_ROW + id.x) * ANCHOR_COUNT;
    uint bidx = sidx * 16;

    for (uint aidx = 0; aidx < ANCHOR_COUNT; aidx++)
    {
        Detection d;
        d.pad = 0;

        // Confidence score
        d.score = Sigmoid(_Scores[sidx++]);

        // Bounding box
        float x = _Boxes[bidx++];
        float y = _Boxes[bidx++];
        float w = _Boxes[bidx++];
        float h = _Boxes[bidx++];

        d.center = VFlip(anchor + float2(x, y) * scale);
        d.extent = float2(w, h) * scale;

        // Key points
        [unroll] for (uint kidx = 0; kidx < 6; kidx++)
        {
            float kx = _Boxes[bidx++];
            float ky = _Boxes[bidx++];
            d.keyPoints[kidx] = VFlip(anchor + float2(kx, ky) * scale);
        }

        // Thresholding
        if (d.score > _Threshold) _Output.Append(d);
    }
}
