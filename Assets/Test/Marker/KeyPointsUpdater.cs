using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

public sealed class KeyPointsUpdater : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] RectTransform[] _keyPoints;

    #endregion

    #region Object reference cache

    Marker _marker;
    RectTransform _xform;
    RectTransform _parent;
    UI.Image _panel;
    UI.Text _label;

    #endregion

    #region Private methods

    void SetKeyPoint(RectTransform xform, Vector2 point)
    {
        var rect = _parent.rect;
        var origin = _xform.anchoredPosition;
        var x = point.x * rect.width;
        var y = (1 - point.y) * rect.height;
        xform.anchoredPosition = new Vector2(x, y) - origin;
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _marker = GetComponent<Marker>();
        _xform = GetComponent<RectTransform>();
        _parent = (RectTransform)_xform.parent;
        _panel = GetComponent<UI.Image>();
        _label = GetComponentInChildren<UI.Text>();
    }

    void LateUpdate()
    {
        var detection = _marker.detection;

        // Bounding box position
        var rect = _parent.rect;
        var x = detection.center.x * rect.width;
        var y = (1 - detection.center.y) * rect.height;
        var w = detection.extent.x * rect.width;
        var h = detection.extent.y * rect.height;

        _xform.anchoredPosition = new Vector2(x, y);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        // Label
        _label.text = $"{(int)(detection.score * 100)}%";

        // Panel color
        _panel.color = new Color(1, 0, 0, 0.5f);

        // Key points
        SetKeyPoint(_keyPoints[0], detection.rightEye);
        SetKeyPoint(_keyPoints[1], detection.leftEye);
        SetKeyPoint(_keyPoints[2], detection.nose);
        SetKeyPoint(_keyPoints[3], detection.mouth);
        SetKeyPoint(_keyPoints[4], detection.rightEar);
        SetKeyPoint(_keyPoints[5], detection.leftEar);
    }

    #endregion
}

} // namespace BlazeFace
