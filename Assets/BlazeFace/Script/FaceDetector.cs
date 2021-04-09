using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace MediaPipe.BlazeFace {

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

    #region Public methods

    public FaceDetector(ResourceSet resources)
    {
        _resources = resources;
        AllocateObjects();
    }

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture image, float threshold)
      => RunModel(image, threshold);

    #endregion

    #region Compile-time constants

    // Maximum number of detections. This value must be matched with
    // MAX_DETECTION in Common.hlsl.
    const int MaxDetection = 64;

    #endregion

    #region Private objects

    ResourceSet _resources;
    ComputeBuffer _preBuffer;
    ComputeBuffer _post1Buffer;
    ComputeBuffer _post2Buffer;
    ComputeBuffer _countBuffer;
    IWorker _worker;
    int _size;

    void AllocateObjects()
    {
        var model = ModelLoader.Load(_resources.model);
        _size = model.inputs[0].shape[6]; // Input tensor width

        _preBuffer = new ComputeBuffer(_size * _size * 3, sizeof(float));

        _post1Buffer = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _post2Buffer = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _countBuffer = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);

        _worker = model.CreateWorker();
    }

    void DeallocateObjects()
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

    #region Neural network inference function

    void RunModel(Texture source, float threshold)
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
        post1.SetFloat("_Threshold", threshold);

        post1.SetTexture(0, "_Scores", scores1RT);
        post1.SetTexture(0, "_Boxes", boxes1RT);
        post1.SetBuffer(0, "_Output", _post1Buffer);
        post1.Dispatch(0, 1, 1, 1);

        post1.SetTexture(1, "_Scores", scores2RT);
        post1.SetTexture(1, "_Boxes", boxes2RT);
        post1.SetBuffer(1, "_Output", _post1Buffer);
        post1.Dispatch(1, 1, 1, 1);

        // Release the temporary render textures.
        RenderTexture.ReleaseTemporary(scores1RT);
        RenderTexture.ReleaseTemporary(scores2RT);
        RenderTexture.ReleaseTemporary(boxes1RT);
        RenderTexture.ReleaseTemporary(boxes2RT);

        // Retrieve the bounding box count.
        ComputeBuffer.CopyCount(_post1Buffer, _countBuffer, 0);

        // 2nd postprocess (overlap removal)
        var post2 = _resources.postprocess2;
        post2.SetBuffer(0, "_Input", _post1Buffer);
        post2.SetBuffer(0, "_Count", _countBuffer);
        post2.SetBuffer(0, "_Output", _post2Buffer);
        post2.Dispatch(0, 1, 1, 1);

        // Retrieve the bounding box count after removal.
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
        var count = _countReadCache[0];

        _post2ReadCache = new Detection[count];
        _post2Buffer.GetData(_post2ReadCache, 0, 0, count);

        return _post2ReadCache;
    }

    #endregion
}

} // namespace MediaPipe.BlazeFace
