Shader "Hidden/BlazeFace/Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Common.hlsl"

    StructuredBuffer<BoundingBox> _Boxes;

    void Vertex(uint vid : SV_VertexID,
                uint iid : SV_InstanceID,
                out float4 position : SV_Position,
                out float4 color : COLOR)
    {
        BoundingBox box = _Boxes[iid];

        // Bounding box vertex
        float x = box.x + box.w * lerp(-0.5, 0.5, vid & 1);
        float y = box.y + box.h * lerp(-0.5, 0.5, vid < 2 || vid == 5);

        // Clip space to screen space
        x =  2 * x - 1;
        y = -2 * y + 1;

        // Aspect ratio compensation
        x = x * _ScreenParams.y / _ScreenParams.x;

        // Vertex attributes
        position = float4(x, y, 1, 1);
        color = box.score;
    }

    float4 Fragment(float4 position : SV_Position,
                    float4 color : COLOR) : SV_Target
    {
        return color;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend One One
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
