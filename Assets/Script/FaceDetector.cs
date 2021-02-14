using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace BlazeFace {

public sealed class FaceDetector : System.IDisposable
{
    #region Internal objects

    ResourceSet _resources;
    ComputeBuffer _preBuffer;
    IWorker _worker;
    RenderTexture _preview;

    #endregion

    #region Public constructor

    public FaceDetector(ResourceSet resources)
    {
        _resources = resources;
        _preBuffer = new ComputeBuffer(Config.InputSize, sizeof(float));
        _worker = ModelLoader.Load(_resources.model).CreateWorker();

        _preview = new RenderTexture(16, 16, 0);
        _preview.enableRandomWrite = true;
        _preview.Create();
    }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        _preBuffer?.Dispose();
        _preBuffer = null;

        _worker?.Dispose();
        _worker = null;

        Object.Destroy(_preview);
        _preview = null;
    }

    #endregion

    #region Public accessors

    public RenderTexture PreviewRT => _preview;

    #endregion

    #region Main image processing function

    public void ProcessImage(Texture sourceTexture)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        var imageSize = Config.ImageSize;
        pre.SetTexture(0, "_Texture", sourceTexture);
        pre.SetBuffer(0, "_Tensor", _preBuffer);
        pre.SetInt("_ImageSize", imageSize);
        pre.Dispatch(0, imageSize / 8, imageSize / 8, 1);

        // Run the BlazeFace model.
        using (var tensor = new Tensor(1, imageSize, imageSize, 3, _preBuffer))
            _worker.Execute(tensor);

        var tensor1 = _worker.PeekOutput("Identity");
        var tensor2 = _worker.PeekOutput("Identity_1");
        using var tensor3 = _worker.PeekOutput("Identity_2").Reshape(new TensorShape(1, 512, 16, 1));
        using var tensor4 = _worker.PeekOutput("Identity_3").Reshape(new TensorShape(1, 384, 16, 1));

        var fmt = RenderTextureFormat.RFloat;
        var rt1 = RenderTexture.GetTemporary(512, 1, 0, fmt);
        var rt2 = RenderTexture.GetTemporary(384, 1, 0, fmt);
        var rt3 = RenderTexture.GetTemporary(16, 512, 0, fmt);
        var rt4 = RenderTexture.GetTemporary(16, 384, 0, fmt);

        tensor1.ToRenderTexture(rt1);
        tensor2.ToRenderTexture(rt2);
        tensor3.ToRenderTexture(rt3);
        tensor4.ToRenderTexture(rt4);

        var post = _resources.postprocess;
        post.SetTexture(0, "_Scores1", rt1);
        post.SetTexture(0, "_Scores2", rt2);
        post.SetTexture(0, "_Boxes1", rt3);
        post.SetTexture(0, "_Boxes2", rt4);
        post.SetTexture(0, "_Output", _preview);
        post.Dispatch(0, 1, 1, 1);

        RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);
        RenderTexture.ReleaseTemporary(rt3);
        RenderTexture.ReleaseTemporary(rt4);
    }

    #endregion
}

} // namespace BlazeFace
