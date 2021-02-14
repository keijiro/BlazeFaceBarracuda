using UnityEngine;
using UI = UnityEngine.UI;

namespace BlazeFace {

sealed class WebcamTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UI.RawImage _previewUI = null;
    [SerializeField] UI.RawImage _overlayUI = null;

    #endregion

    #region Internal objects

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;

    FaceDetector _detector;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Texture allocation
        _webcamRaw = new WebCamTexture();
        _webcamBuffer = new RenderTexture(1080, 1080, 0);

        _webcamRaw.Play();
        _previewUI.texture = _webcamBuffer;

        // Face detector initialization
        _detector = new FaceDetector(_resources);

        _overlayUI.texture = _detector.PreviewRT;
    }

    void OnDisable()
    {
        _detector?.Dispose();
        _detector = null;
    }

    void OnDestroy()
    {
        if (_webcamRaw != null) Destroy(_webcamRaw);
        if (_webcamBuffer != null) Destroy(_webcamBuffer);
    }

    void Update()
    {
        // Check if the webcam is ready (needed for macOS support)
        if (_webcamRaw.width <= 16) return;

        // Input buffer update with aspect ratio correction
        var vflip = _webcamRaw.videoVerticallyMirrored;
        var aspect = (float)_webcamRaw.height / _webcamRaw.width;
        var scale = new Vector2(aspect, vflip ? -1 : 1);
        var offset = new Vector2((1 - aspect) / 2, vflip ? 1 : 0);
        Graphics.Blit(_webcamRaw, _webcamBuffer, scale, offset);

        // Run the object detector with the webcam input.
        _detector.ProcessImage(_webcamBuffer);
    }

    #endregion
}

} // namespace BlazeFace
