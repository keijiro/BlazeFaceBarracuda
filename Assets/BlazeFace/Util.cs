using Unity.Barracuda;
using UnityEngine;

namespace BlazeFace {

static class IWorkerExtensions
{
    public static RenderTexture
      CopyOutputToTempRT(this IWorker worker, string name, int w, int h)
    {
        var fmt = RenderTextureFormat.RFloat;
        var shape = new TensorShape(1, h, w, 1);
        var rt = RenderTexture.GetTemporary(w, h, 0, fmt);
        using (var tensor = worker.PeekOutput(name).Reshape(shape))
            tensor.ToRenderTexture(rt);
        return rt;
    }
}

} // namespace BlazeFace
