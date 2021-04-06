using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace BlazeFace {

//
// Main face detector class
//
public sealed class FaceDetector : System.IDisposable
{
    #region Public accessors

    public ComputeBuffer DetectionBuffer
      => _post2Buffer;

    public void SetIndirectDrawCount(ComputeBuffer drawArgs)
      => ComputeBuffer.CopyCount(_post2Buffer, drawArgs, sizeof(uint));

    public IEnumerable<Detection> Detections
      => _post2ReadCache ?? UpdatePost2ReadCache();

    #endregion

    #region Public constructor

    public FaceDetector(ResourceSet resources)
    {
        _resources = resources;

        var model = ModelLoader.Load(_resources.model);
        _size = model.inputs[0].shape[6];

        _preBuffer = new ComputeBuffer(_size * _size * 3, sizeof(float));

        _post1Buffer = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _post2Buffer = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _countBuffer = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);

        _worker = model.CreateWorker();
    }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        _preBuffer?.Dispose();
        _preBuffer = null;

        _post1Buffer?.Dispose();
        _post1Buffer = null;

        _post2Buffer?.Dispose();
        _post2Buffer = null;

        _countBuffer?.Dispose();
        _countBuffer = null;

        _worker?.Dispose();
        _worker = null;
    }

    #endregion

    #region Public methods

    public void ProcessImage
      (Texture sourceTexture, float scoreThreshold, float overlapThreshold)
        => RunModel(sourceTexture, scoreThreshold, overlapThreshold);

    #endregion

    #region Private constants and variables

    const int MaxDetection = 64;

    ResourceSet _resources;
    ComputeBuffer _preBuffer;
    ComputeBuffer _post1Buffer;
    ComputeBuffer _post2Buffer;
    ComputeBuffer _countBuffer;
    IWorker _worker;
    int _size;

    #endregion

    #region Inference function

    void RunModel(Texture source, float minScore, float maxOverlap)
    {
        // Reset the compute buffer counters.
        _post1Buffer.SetCounterValue(0);
        _post2Buffer.SetCounterValue(0);

        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetInt("_ImageSize", _size);
        pre.SetTexture(0, "_Texture", source);
        pre.SetBuffer(0, "_Tensor", _preBuffer);
        pre.Dispatch(0, _size / 8, _size / 8, 1);

        // Run the BlazeFace model.
        using (var tensor = new Tensor(1, _size, _size, 3, _preBuffer))
            _worker.Execute(tensor);

        // Output tensors -> Temporary render textures
        var scores1RT = _worker.CopyOutputToTempRT("Identity"  ,  1, 512);
        var scores2RT = _worker.CopyOutputToTempRT("Identity_1",  1, 384);
        var  boxes1RT = _worker.CopyOutputToTempRT("Identity_2", 16, 512);
        var  boxes2RT = _worker.CopyOutputToTempRT("Identity_3", 16, 384);

        // 1st postprocess (bounding box aggregation)
        var post1 = _resources.postprocess1;
        post1.SetFloat("_ImageSize", _size);
        post1.SetFloat("_Threshold", minScore);

        post1.SetTexture(0, "_Scores", scores1RT);
        post1.SetTexture(0, "_Boxes", boxes1RT);
        post1.SetBuffer(0, "_Output", _post1Buffer);
        post1.Dispatch(0, 1, 1, 1);

        post1.SetTexture(1, "_Scores", scores2RT);
        post1.SetTexture(1, "_Boxes", boxes2RT);
        post1.SetBuffer(1, "_Output", _post1Buffer);
        post1.Dispatch(1, 1, 1, 1);

        // Release temporary render textures.
        RenderTexture.ReleaseTemporary(scores1RT);
        RenderTexture.ReleaseTemporary(scores2RT);
        RenderTexture.ReleaseTemporary(boxes1RT);
        RenderTexture.ReleaseTemporary(boxes2RT);

        // Bounding box count
        ComputeBuffer.CopyCount(_post1Buffer, _countBuffer, 0);

        // 2nd postprocess (overlap removal)
        var post2 = _resources.postprocess2;
        post2.SetFloat("_Threshold", maxOverlap);
        post2.SetBuffer(0, "_Input", _post1Buffer);
        post2.SetBuffer(0, "_Count", _countBuffer);
        post2.SetBuffer(0, "_Output", _post2Buffer);
        post2.Dispatch(0, 1, 1, 1);

        // Bounding box count after removal
        ComputeBuffer.CopyCount(_post2Buffer, _countBuffer, 0);

        // Read cache invalidation
        _post2ReadCache = null;
    }

    #endregion

    #region GPU to CPU readback

    Detection[] _post2ReadCache;
    int[] _countReadCache = new int[1];

    Detection[] UpdatePost2ReadCache()
    {
        _countBuffer.GetData(_countReadCache, 0, 0, 1);
        var buffer = new Detection[_countReadCache[0]];
        _post2Buffer.GetData(buffer, 0, 0, buffer.Length);
        return buffer;
    }

    #endregion
}

} // namespace BlazeFace
