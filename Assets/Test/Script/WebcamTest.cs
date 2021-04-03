using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

public sealed class WebcamTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UI.RawImage _previewUI = null;
    [SerializeField] MarkerBase _markerPrefab = null;

    #endregion

    #region Internal objects

    FaceDetector _detector;
    MarkerBase[] _markers = new MarkerBase[50];

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Face detector initialization
        _detector = new FaceDetector(_resources);

        // Marker populating
        for (var i = 0; i < _markers.Length; i++)
            _markers[i] = Instantiate(_markerPrefab, _previewUI.transform);
    }

    void OnDestroy()
    {
        _detector?.Dispose();
        foreach (var m in _markers) Destroy(m);
    }

    void Update()
    {
        _previewUI.texture = _webcam.Texture;

        _detector.ProcessImage
          (_webcam.Texture, _scoreThreshold, _overlapThreshold);

        // Marker update
        var i = 0;

        foreach (var box in _detector.DetectedFaces)
        {
            if (i == _markers.Length) break;
            _markers[i++].SetAttributes(box);
        }

        for (; i < _markers.Length; i++) _markers[i].Hide();
    }

    #endregion
}

} // namespace BlazeFace
