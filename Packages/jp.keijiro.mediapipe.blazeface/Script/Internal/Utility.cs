using Unity.Barracuda;
using UnityEngine;
using System;

namespace MediaPipe.BlazeFace {

#region Extension methods

static class ComputeShaderExtensions
{
    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);
        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;
        compute.Dispatch(kernel, x, y, z);
    }
}

static class IWorkerExtensions
{
    public static ComputeBuffer PeekOutputBuffer
      (this IWorker worker, string tensorName)
      => ((ComputeTensorData)worker.PeekOutput(tensorName).tensorOnDevice).buffer;
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
