using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;

namespace BlazeFace {

public sealed class InferenceTest : MonoBehaviour
{
    [SerializeField] Texture2D _image;
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField] RawImage _previewUI;
    [SerializeField] Marker _markerPrefab = null;
    [SerializeField] NNModel _model;

    void Start()
    {
        _previewUI.texture = _image;

        // Input image -> Tensor (1, 128, 128, 3)
        var source = new float[128 * 128 * 3];

        for (var y = 0; y < 128; y++)
        {
            for (var x = 0; x < 128; x++)
            {
                var i = ((127 - y) * 128 + x) * 3;
                var sx = x * _image.width / 128;
                var sy = y * _image.height / 128;
                var p = _image.GetPixel(sx, sy);
                source[i + 0] = p.r * 2 - 1;
                source[i + 1] = p.g * 2 - 1;
                source[i + 2] = p.b * 2 - 1;
            }
        }

        // Inference
        var model = ModelLoader.Load(_model);
        using var worker = WorkerFactory.CreateWorker(model);
        using (var tensor = new Tensor(1, 128, 128, 3, source))
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

                    sx += tensor2[0, 0, i, 0] / 128;
                    sy += tensor2[0, 0, i, 1] / 128;

                    var sw = tensor2[0, 0, i, 2] / 128;
                    var sh = tensor2[0, 0, i, 3] / 128;

                    var box = new BoundingBox(sx, sy, sw, sh);

                    var marker = Instantiate(_markerPrefab, _previewUI.transform);
                    marker.SetAttributes(box, s);
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

                    sx += tensor4[0, 0, i, 0] / 128;
                    sy += tensor4[0, 0, i, 1] / 128;

                    var sw = tensor4[0, 0, i, 2] / 128 / 2;
                    var sh = tensor4[0, 0, i, 3] / 128 / 2;

                    var box = new BoundingBox(sx, sy, sw, sh);

                    var marker = Instantiate(_markerPrefab, _previewUI.transform);
                    marker.SetAttributes(box, s);
                }
            }
        }
    }
}

} // namespace BlazeFace
