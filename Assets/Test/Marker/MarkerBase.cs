namespace BlazeFace {

public abstract class MarkerBase : UnityEngine.MonoBehaviour
{
    public abstract void SetAttributes(in BoundingBox box);
    public abstract void Hide();
}

} // namespace BlazeFace
