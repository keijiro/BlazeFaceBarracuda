using UnityEngine;
using UI = UnityEngine.UI;

namespace MediaPipe.BlazeFace {

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Texture2D _image = null;
    [SerializeField] WebcamInput _webcam = null;
    [Space]
    [SerializeField, Range(0, 1)] float _threshold = 0.7f;
    [Space]
    [SerializeField] UI.RawImage _previewUI = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Marker _markerPrefab = null;

    #endregion

    #region Private members

    FaceDetector _detector;
    Marker[] _markers = new Marker[16];

    void RunDetector(Texture input)
    {
        // Face detection
        _detector.ProcessImage(input, _threshold);

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

        // UI update
        _previewUI.texture = input;
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Face detector initialization
        _detector = new FaceDetector(_resources);

        // Marker population
        for (var i = 0; i < _markers.Length; i++)
            _markers[i] = Instantiate(_markerPrefab, _previewUI.transform);

        // Static image test: Run the detector once.
        if (_image != null) RunDetector(_image);
    }

    void OnDestroy()
      => _detector?.Dispose();

    void LateUpdate()
    {
        // Webcam test: Run the detector every frame.
        if (_webcam != null) RunDetector(_webcam.Texture);
    }

    #endregion
}

} // namespace MediaPipe.BlazeFace
