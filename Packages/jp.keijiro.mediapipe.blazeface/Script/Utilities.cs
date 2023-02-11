using Unity.Barracuda;
using UnityEngine;
using System;

namespace MediaPipe.BlazeFace {

#region Extension methods

static class IWorkerExtensions
{
    //
    // Retrieves an output tensor from a NN worker and returns it as a
    // temporary render texture. The caller must release it using
    // RenderTexture.ReleaseTemporary.
    //
    public static RenderTexture
      CopyOutputToTempRT(this IWorker worker, string name, int w, int h)
    {
        var fmt = RenderTextureFormat.RFloat;
        var rt = RenderTexture.GetTemporary(w, h, 0, fmt);
#if BARRACUDA_4_0_0_OR_LATER
        var shape = new TensorShape(1, 1, h, w);
        using (var tensor = (TensorFloat)worker.PeekOutput(name).ShallowReshape(shape))
            TensorToRenderTexture.ToRenderTexture(tensor, rt);
#else
        var shape = new TensorShape(1, h, w, 1);
        using (var tensor = worker.PeekOutput(name).Reshape(shape))
            tensor.ToRenderTexture(rt);
#endif
        return rt;
    }
}

#endregion

#region GPU to CPU readback helpers

sealed class DetectionCache
{
    public DetectionCache(GraphicsBuffer array, GraphicsBuffer count)
      => _source = (array, count);

    public ReadOnlySpan<Detection> Cached
      => Read();

    public void Invalidate()
      => _isCached = false;

    (GraphicsBuffer array, GraphicsBuffer count) _source;

    (Detection[] array, int[] count) _cache
      = (new Detection[Detection.Max], new int[1]);

    bool _isCached;

    ReadOnlySpan<Detection> Read()
    {
        if (!_isCached)
        {
            _source.count.GetData(_cache.count, 0, 0, 1);
            _source.array.GetData(_cache.array, 0, 0, _cache.count[0]);
            _isCached = true;
        }
        return new ReadOnlySpan<Detection>(_cache.array, 0, _cache.count[0]);
    }
}

#endregion

} // namespace MediaPipe.BlazeFace
