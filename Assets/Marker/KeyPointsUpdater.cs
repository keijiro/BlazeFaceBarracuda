using UnityEngine;
using UI = UnityEngine.UI;

namespace MediaPipe.BlazeFace {

public sealed class KeyPointsUpdater : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] RectTransform[] _keyPoints;

    #endregion

    #region Private members

    Marker _marker;
    RectTransform _xform;
    RectTransform _parent;
    UI.Text _label;

    void SetKeyPoint(RectTransform xform, Vector2 point)
      => xform.anchoredPosition =
           point * _parent.rect.size - _xform.anchoredPosition;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _marker = GetComponent<Marker>();
        _xform = GetComponent<RectTransform>();
        _parent = (RectTransform)_xform.parent;
        _label = GetComponentInChildren<UI.Text>();
    }

    void LateUpdate()
    {
        var detection = _marker.detection;

        // Bounding box center
        _xform.anchoredPosition = detection.center * _parent.rect.size;

        // Bounding box size
        var size = detection.extent * _parent.rect.size;
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        // Key points
        SetKeyPoint(_keyPoints[0], detection.rightEye);
        SetKeyPoint(_keyPoints[1], detection.leftEye);
        SetKeyPoint(_keyPoints[2], detection.nose);
        SetKeyPoint(_keyPoints[3], detection.mouth);
        SetKeyPoint(_keyPoints[4], detection.rightEar);
        SetKeyPoint(_keyPoints[5], detection.leftEar);

        // Label
        _label.text = $"{(int)(detection.score * 100)}%";
    }

    #endregion
}

} // namespace MediaPipe.BlazeFace
