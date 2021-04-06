namespace BlazeFace {

public abstract class MarkerBase : UnityEngine.MonoBehaviour
{
    public abstract void SetDetection(in Detection detection);
    public abstract void Hide();
}

} // namespace BlazeFace
