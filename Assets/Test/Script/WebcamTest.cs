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
    [SerializeField] Marker _markerPrefab = null;

    #endregion

    #region Internal objects

    FaceDetector _detector;
    Marker[] _markers = new Marker[50];

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Face detector initialization
        _detector = new FaceDetector(_resources);

        // Marker population
        for (var i = 0; i < _markers.Length; i++)
            _markers[i] = Instantiate(_markerPrefab, _previewUI.transform);
    }

    void OnDestroy()
    {
        _detector?.Dispose();
        foreach (var m in _markers) Destroy(m.gameObject);
    }

    void Update()
    {
        _previewUI.texture = _webcam.Texture;

        _detector.ProcessImage
          (_webcam.Texture, _scoreThreshold, _overlapThreshold);

        // Marker update
        var i = 0;

        foreach (var detection in _detector.Detections)
        {
            if (i == _markers.Length) break;
            var marker = _markers[i++];
            marker.detection = detection;
            marker.gameObject.SetActive(true);
        }

        for (; i < _markers.Length; i++)
            _markers[i].gameObject.SetActive(false);
    }

    #endregion
}

} // namespace BlazeFace
