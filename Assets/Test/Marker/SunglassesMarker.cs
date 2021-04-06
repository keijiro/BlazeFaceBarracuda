using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

public sealed class SunglassesMarker : MarkerBase
{
    #region Object reference cache

    RectTransform _xform;
    RectTransform _parent;

    #endregion

    #region MarkerBase implementation

    public override void SetDetection(in Detection detection) 
    {
        if (_xform == null)
        {
            _xform = GetComponent<RectTransform>();
            _parent = (RectTransform)_xform.parent;
        }

        // Sunglasses position
        var mid = (detection.leftEye + detection.rightEye) / 2;
        var diff = detection.leftEye - detection.rightEye;
        var width = diff.magnitude;
        var angle = Vector2.Angle(diff, Vector2.right);
        if (detection.rightEye.y < detection.leftEye.y) angle *= -1;

        // Transform
        var rect = _parent.rect;
        var x = mid.x * rect.width;
        var y = (1 - mid.y) * rect.height;
        var w = width * 3 * rect.width;
        var h = w / 26 * 5;

        _xform.anchoredPosition = new Vector2(x, y);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        _xform.eulerAngles = Vector3.forward * angle;

        // Enable
        gameObject.SetActive(true);
    }

    public override void Hide()
      => gameObject.SetActive(false);

    #endregion
}

} // namespace BlazeFace
