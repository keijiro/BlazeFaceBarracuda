using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

public sealed class StaticImageTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Texture2D _image;
    [SerializeField, Range(0, 1)] float _threshold = 0.7f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UI.RawImage _previewUI;
    [SerializeField] Marker _markerPrefab = null;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _previewUI.texture = _image;

        using var detector = new FaceDetector(_resources);

        detector.ProcessImage(_image, _threshold);

        foreach (var detection in detector.Detections)
        {
            var marker = Instantiate(_markerPrefab, _previewUI.transform);
            marker.detection = detection;
        }
    }

    #endregion
}

} // namespace BlazeFace
