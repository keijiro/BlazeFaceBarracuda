using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

sealed class Marker : MonoBehaviour
{
    RectTransform _parent;
    RectTransform _xform;
    UI.Image _panel;
    UI.Text _label;

    void Start()
    {
        _xform = GetComponent<RectTransform>();
        _parent = (RectTransform)_xform.parent;
        _panel = GetComponent<UI.Image>();
        _label = GetComponentInChildren<UI.Text>();
    }

    public void SetAttributes(in BoundingBox box)
    {
        if (_xform == null) Start();

        var rect = _parent.rect;

        // Bounding box position
        var x = box.x * rect.width;
        var y = (1 - box.y) * rect.height;
        var w = box.w * rect.width;
        var h = box.h * rect.height;

        _xform.anchoredPosition = new Vector2(x, y);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        // Label (class name + score)
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
