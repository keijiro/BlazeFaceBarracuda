using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

public sealed class SunglassesUpdater : MonoBehaviour
{
    #region Private members

    Marker _marker;
    RectTransform _xform;
    RectTransform _parent;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _marker = GetComponent<Marker>();
        _xform = GetComponent<RectTransform>();
        _parent = (RectTransform)_xform.parent;
    }

    void LateUpdate()
    {
        var detection = _marker.detection;

        // Eye key points
        var mid = (detection.leftEye + detection.rightEye) / 2;
        var diff = detection.leftEye - detection.rightEye;

        // Position
        _xform.anchoredPosition = mid * _parent.rect.size;

        // Size
        var size = diff.magnitude * 3 * _parent.rect.width;
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

        // Rotation
        var angle = Vector2.Angle(diff, Vector2.right);
        if (detection.rightEye.y > detection.leftEye.y) angle *= -1;
        _xform.eulerAngles = Vector3.forward * angle;
    }

    #endregion
}

} // namespace BlazeFace
