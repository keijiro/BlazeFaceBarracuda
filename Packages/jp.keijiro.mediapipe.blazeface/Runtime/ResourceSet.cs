using UnityEngine;
using Unity.Barracuda;

namespace MediaPipe.BlazeFace {

//
// ScriptableObject class used to hold references to internal assets
//
[CreateAssetMenu(fileName = "BlazeFace",
                 menuName = "ScriptableObjects/MediaPipe/BlazeFace Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel model;
    public ComputeShader preprocess;
    public ComputeShader postprocess1;
    public ComputeShader postprocess2;
}

} // namespace MediaPipe.BlazeFace
