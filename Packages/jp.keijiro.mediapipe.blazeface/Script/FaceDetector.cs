using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace MediaPipe.BlazeFace {

//
// Main face detector class
//
public sealed partial class FaceDetector : System.IDisposable
{
    #region Public methods/properties

    public FaceDetector(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture image, float threshold = 0.75f)
      => RunModel(image, threshold);

    public System.ReadOnlySpan<Detection> Detections
      => _readCache.Cached;

    public GraphicsBuffer DetectionBuffer
      => _output.post2;

    public void SetIndirectDrawCount(GraphicsBuffer drawArgs)
      => GraphicsBuffer.CopyCount(_output.post2, drawArgs, sizeof(uint));

    #endregion

    #region Private objects

    ResourceSet _resources;
    IWorker _worker;
    (Tensor tensor, ComputeTensorData data) _preprocess;
    (GraphicsBuffer post1, GraphicsBuffer post2, GraphicsBuffer count) _output;
    int _size;
    DetectionCache _readCache;

    void AllocateObjects(ResourceSet resources)
    {
        // NN model
        var model = ModelLoader.Load(resources.model);

        // Private objects
        _resources = resources;
        _worker = model.CreateWorker(WorkerFactory.Device.GPU);

        // Input shape
#if BARRACUDA_4_0_0_OR_LATER
        _size = model.inputs[0].shape[2].value;
        var inputShape = model.inputs[0].shape.ToTensorShape();
#else
        _size = model.inputs[0].shape[6];
        var inputShape = new TensorShape(model.inputs[0].shape);
#endif

        // Preprocessing buffers
#if BARRACUDA_4_0_0_OR_LATER
        _preprocess.data = new ComputeTensorData(inputShape, "input", false);
        _preprocess.tensor = TensorFloat.Zeros(inputShape);
        _preprocess.tensor.AttachToDevice(_preprocess.data);
#else
        _preprocess.data = new ComputeTensorData
          (inputShape, "input", ComputeInfo.ChannelsOrder.NHWC, false);
        _preprocess.tensor = new Tensor(inputShape, _preprocess.data);
#endif

        // Output buffers
        _output.post1 = new GraphicsBuffer(GraphicsBuffer.Target.Append, Detection.Max, Detection.Size);
        _output.post2 = new GraphicsBuffer(GraphicsBuffer.Target.Append, Detection.Max, Detection.Size);
        _output.count = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, sizeof(uint));

        // Detection data read cache
        _readCache = new DetectionCache(_output.post2, _output.count);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _preprocess.tensor?.Dispose();
        _preprocess = (null, null);

        _output.post1?.Dispose();
        _output.post2?.Dispose();
        _output.count?.Dispose();
        _output = (null, null, null);
    }

    #endregion

    #region Neural network inference function

    void RunModel(Texture source, float threshold)
    {
        // Reset the compute buffer counters.
        _output.post1.SetCounterValue(0);
        _output.post2.SetCounterValue(0);

        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetInt("_ImageSize", _size);
        pre.SetTexture(0, "_Texture", source);
        pre.SetBuffer(0, "_Tensor", _preprocess.data.buffer);
        pre.Dispatch(0, _size / 8, _size / 8, 1);

        // Run the BlazeFace model.
        _worker.Execute(_preprocess.tensor);

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
        post1.SetBuffer(0, "_Output", _output.post1);
        post1.Dispatch(0, 1, 1, 1);

        post1.SetTexture(1, "_Scores", scores2RT);
        post1.SetTexture(1, "_Boxes", boxes2RT);
        post1.SetBuffer(1, "_Output", _output.post1);
        post1.Dispatch(1, 1, 1, 1);

        // Release the temporary render textures.
        RenderTexture.ReleaseTemporary(scores1RT);
        RenderTexture.ReleaseTemporary(scores2RT);
        RenderTexture.ReleaseTemporary(boxes1RT);
        RenderTexture.ReleaseTemporary(boxes2RT);

        // Retrieve the bounding box count.
        GraphicsBuffer.CopyCount(_output.post1, _output.count, 0);

        // 2nd postprocess (overlap removal)
        var post2 = _resources.postprocess2;
        post2.SetBuffer(0, "_Input", _output.post1);
        post2.SetBuffer(0, "_Count", _output.count);
        post2.SetBuffer(0, "_Output", _output.post2);
        post2.Dispatch(0, 1, 1, 1);

        // Retrieve the bounding box count after removal.
        GraphicsBuffer.CopyCount(_output.post2, _output.count, 0);

        // Cache data invalidation
        _readCache.Invalidate();
    }

    #endregion
}

} // namespace MediaPipe.BlazeFace
