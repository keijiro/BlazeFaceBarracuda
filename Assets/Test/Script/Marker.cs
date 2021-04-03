using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

public sealed class Marker : MonoBehaviour
{
    RectTransform _xform;
    RectTransform _parent;
    UI.Image _panel;
    UI.Text _label;

    public void SetAttributes(in BoundingBox box)
    {
        if (_xform == null)
        {
            _xform = GetComponent<RectTransform>();
            _parent = (RectTransform)_xform.parent;
            _panel = GetComponent<UI.Image>();
            _label = GetComponentInChildren<UI.Text>();
        }

        // Bounding box position
        var rect = _parent.rect;
        var x = box.x * rect.width;
        var y = (1 - box.y) * rect.height;
        var w = box.w * rect.width;
        var h = box.h * rect.height;

        _xform.anchoredPosition = new Vector2(x, y);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        // Label
        _label.text = $"{(int)(box.score * 100)}%";

        // Panel color
        _panel.color = new Color(1, 0, 0, 0.5f);

        // Enable
        gameObject.SetActive(true);
    }

    public void Hide()
      => gameObject.SetActive(false);
}

} // namespace BlazeFace
