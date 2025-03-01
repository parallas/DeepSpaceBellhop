#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float MaskBlend;
float FrameBlend;

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};
float GameTime;
float4 BackBufferResolution;
float4 ScreenResolution;

Texture2D MaskTexture;
sampler2D MaskTextureSampler = sampler_state {
    Texture = <MaskTexture>;
    AddressU = WRAP;
    AddressV = WRAP;
};

Texture2D LastFrameTexture;
sampler2D LastFrameTextureSampler = sampler_state {
    Texture = <LastFrameTexture>;
    AddressU = WRAP;
    AddressV = WRAP;
};

#include "voronoi.hlsl"
#include "keijiro_simplex3d.hlsl"

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float4 col = tex2D(SpriteTextureSampler, uv).rgba * input.Color.rgba;
    float4 maskSample = tex2D(MaskTextureSampler, uv * BackBufferResolution.xy);
    maskSample = lerp(maskSample, float4(1, 1, 1, 1), 0.85f);
    maskSample = lerp(float4(1, 1, 1, 1), maskSample, MaskBlend);
    float3 maskColored = maskSample.rgb * col.rgb;
    float3 blurSample = lerp(maskColored, tex2D(LastFrameTextureSampler, uv).rgb, 0.2f * FrameBlend);
    float3 finalSample = blurSample;
    return float4(finalSample, 1);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
