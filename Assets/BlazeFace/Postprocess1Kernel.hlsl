[numthreads(CELLS_IN_ROW, CELLS_IN_ROW, 1)]
void KERNEL_NAME(uint2 id : SV_DispatchThreadID)
{
    for (uint aidx = 0; aidx < ANCHOR_COUNT; aidx++)
    {
        // We're not sure why but the direction of the tensor is flipped, so we
        // read them in the reversed order.
        uint ref_y = (CELLS_IN_ROW - 1 - id.y) * CELLS_IN_ROW +
                     (CELLS_IN_ROW - 1 - id.x);
        ref_y = ref_y * ANCHOR_COUNT + aidx;

        // Confidence score
        float score = Sigmoid(_Scores[uint2(0, ref_y)].x);

        // Bounding box
        float x = _Boxes[uint2(0, ref_y)].x;
        float y = _Boxes[uint2(1, ref_y)].x;
        float w = _Boxes[uint2(2, ref_y)].x;
        float h = _Boxes[uint2(3, ref_y)].x;

        // Output structure
        BoundingBox box;
        box.center = (id + 0.5) / CELLS_IN_ROW + float2(x, y) / _ImageSize;
        box.extent = float2(w, h) / _ImageSize;
        box.keyPoint0 = box.keyPoint1 = box.keyPoint2 = 0;
        box.keyPoint3 = box.keyPoint4 = box.keyPoint5 = 0;
        box.score = score;
        box.pad = 0;

        // Thresholding
        if (score > _Threshold) _Output.Append(box);
    }
}
