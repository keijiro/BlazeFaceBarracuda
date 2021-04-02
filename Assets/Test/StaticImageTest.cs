using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;

namespace BlazeFace {

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField] Texture2D _image;
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] RawImage _previewUI;
    [SerializeField] Marker _markerPrefab = null;

    void Start()
    {
        _previewUI.texture = _image;

        using var detector = new FaceDetector(_resources);

        // Run the object detector with the webcam input.
        detector.ProcessImage(_image, _scoreThreshold, _overlapThreshold);

        foreach (var box in detector.DetectedFaces)
        {
            var marker = Instantiate(_markerPrefab, _previewUI.transform);
            marker.SetAttributes(box);
        }
    }
}

} // namespace BlazeFace
