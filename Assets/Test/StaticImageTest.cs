using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;

namespace BlazeFace {

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField] Texture2D _image;
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField] RawImage _previewUI;
    [SerializeField] Marker _markerPrefab = null;
    [SerializeField] NNModel _model;

    void Start()
    {
        _previewUI.texture = _image;

        // Model loading
        var model = ModelLoader.Load(_model);
        var size = model.inputs[0].shape[6];

        // Input image -> Tensor (1, size, size, 3)
        var source = new float[size * size * 3];

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var i = ((size - 1 - y) * size + x) * 3;
                var sx = x * _image.width  / size;
                var sy = y * _image.height / size;
                var p = _image.GetPixel(sx, sy);
                source[i + 0] = p.r * 2 - 1;
                source[i + 1] = p.g * 2 - 1;
                source[i + 2] = p.b * 2 - 1;
            }
        }

        // Inference
        using var worker = WorkerFactory.CreateWorker(model);
        using (var tensor = new Tensor(1, size, size, 3, source))
            worker.Execute(tensor);

        // 16x16 feature map
        var tensor1 = worker.PeekOutput("Identity");
        var tensor2 = worker.PeekOutput("Identity_2");

        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 16; x++)
            {
                for (var anchor = 0; anchor < 2; anchor++)
                {
                    var i = (16 * y + x) * 2 + anchor;
                    var s = tensor1[0, 0, i, 0];
                    s = 1 / (1 + Mathf.Exp(-s));
                    if (s < _scoreThreshold) continue;

                    var sx = (x + 0.5f) / 16;
                    var sy = (y + 0.5f) / 16;

                    sx += tensor2[0, 0, i, 0] / size;
                    sy += tensor2[0, 0, i, 1] / size;

                    var sw = tensor2[0, 0, i, 2] / size;
                    var sh = tensor2[0, 0, i, 3] / size;

                    var box = new BoundingBox(sx, sy, sw, sh, s);

                    var marker = Instantiate(_markerPrefab, _previewUI.transform);
                    marker.SetAttributes(box);
                }
            }
        }

        // 8x8 feature map
        var tensor3 = worker.PeekOutput("Identity_1");
        var tensor4 = worker.PeekOutput("Identity_3");

        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                for (var anchor = 0; anchor < 6; anchor++)
                {
                    var i = (8 * y + x) * 6 + anchor;
                    var s = tensor3[0, 0, i, 0];
                    s = 1 / (1 + Mathf.Exp(-s));
                    if (s < _scoreThreshold) continue;

                    var sx = (x + 0.5f) / 8;
                    var sy = (y + 0.5f) / 8;

                    sx += tensor4[0, 0, i, 0] / size;
                    sy += tensor4[0, 0, i, 1] / size;

                    var sw = tensor4[0, 0, i, 2] / size;
                    var sh = tensor4[0, 0, i, 3] / size;

                    var box = new BoundingBox(sx, sy, sw, sh, s);

                    var marker = Instantiate(_markerPrefab, _previewUI.transform);
                    marker.SetAttributes(box);
                }
            }
        }
    }
}

} // namespace BlazeFace
