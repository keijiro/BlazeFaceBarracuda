using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;

namespace BlazeFace
{

public sealed class InferenceTest : MonoBehaviour
{
    [SerializeField] WorkerFactory.Type _workerType;
    [SerializeField] NNModel _model;
    [SerializeField] Texture2D _image;
    [SerializeField] RawImage _uiSource;
    [SerializeField] RawImage _uiLayer1;
    [SerializeField] RawImage _uiLayer2;

    void Start()
    {
        // Input image -> Tensor (1, 128, 128, 3)
        var source = new float[128 * 128 * 3];

        for (var y = 0; y < 128; y++)
        {
            for (var x = 0; x < 128; x++)
            {
                var i = ((127 - y) * 128 + x) * 3;
                var p = _image.GetPixel(x, y);
                source[i + 0] = p.r * 2 - 1;
                source[i + 1] = p.g * 2 - 1;
                source[i + 2] = p.b * 2 - 1;
            }
        }

        // Inference
        var model = ModelLoader.Load(_model);
        using var worker = WorkerFactory.CreateWorker(_workerType, model);

        using (var tensor = new Tensor(1, 128, 128, 3, source))
            worker.Execute(tensor);

        // 16x16 feature map
        var layer1 = new Texture2D(16, 16);
        layer1.filterMode = FilterMode.Point;

        var tensor1 = worker.PeekOutput("Identity");

        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 16; x++)
            {
                var alpha = .0f;

                for (var anchor = 0; anchor < 2; anchor++)
                {
                    var i = (16 * y + x) * 2 + anchor;
                    var s = tensor1[0, 0, i, 0];
                    alpha += 1 / (1 + Mathf.Exp(-s));
                }

                layer1.SetPixel(x, 15 - y, new Color(1, 0, 0, alpha));
            }
        }

        layer1.Apply();

        // 8x8 feature map
        var layer2 = new Texture2D(8, 8);
        layer2.filterMode = FilterMode.Point;

        var tensor2 = worker.PeekOutput("Identity_1");

        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                var alpha = .0f;

                for (var anchor = 0; anchor < 6; anchor++)
                {
                    var i = (8 * y + x) * 6 + anchor;
                    var s = tensor2[0, 0, i, 0];
                    alpha += 1 / (1 + Mathf.Exp(-s));
                }

                layer2.SetPixel(x, 7 - y, new Color(0, 0, 1, alpha));
            }
        }

        layer2.Apply();

        // UI
        _uiSource.texture = _image;
        _uiLayer1.texture = layer1;
        _uiLayer2.texture = layer2;
    }
}

} // namespace BlazeFace
