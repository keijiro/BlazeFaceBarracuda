using UnityEngine;
using UI = UnityEngine.UI;

namespace MediaPipe.BlazeFace {

//
// Real time detection test with webcam input
//
public sealed class WebcamTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [SerializeField, Range(0, 1)] float _threshold = 0.7f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UI.RawImage _previewUI = null;
    [SerializeField] Marker _markerPrefab = null;

    #endregion

    #region Internal objects

    FaceDetector _detector;
    Marker[] _markers = new Marker[16];

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
      => _detector?.Dispose();

    void LateUpdate()
    {
        _previewUI.texture = _webcam.Texture;

        _detector.ProcessImage(_webcam.Texture, _threshold);

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

} // namespace MediaPipe.BlazeFace
