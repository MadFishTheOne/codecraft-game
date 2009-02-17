uniform extern float4x4 ViewProj : WORLDVIEWPROJECTION;
uniform extern texture tex;
uniform float4 Color;
uniform float2 Size ;
#define BatchSize 240
float4 Positions[BatchSize];
#define CPlayers 6
float3 PlayerColors[CPlayers];

sampler textureSampler = sampler_state
{
    Texture=<tex>;
    MipFilter=Linear;
    MagFilter=Linear;
    MinFilter=Linear;
};
struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float4 Color:TEXCOORD1;
};

VS_OUTPUT Transform(
    float4 Pos  : POSITION, 
    float Index : PSIZE, 
    float2 Text : TEXCOORD0 )
{
   VS_OUTPUT Out;
	Pos.x*=Size.x*0.5;
	Pos.y*=Size.y*0.5;
	float4 currPos=Pos;
	currPos.x=-Pos.x*cos(Positions[Index].z)-Pos.y*sin(Positions[Index].z)+Positions[Index].x;
	currPos.y=+Pos.x*sin(Positions[Index].z)-Pos.y*cos(Positions[Index].z)+Positions[Index].y;	
    currPos.z=-15000;
    
	Out.Position = mul(currPos,ViewProj);    
    Out.TextureCoordinate =  Text;
    Out.Color=float4(PlayerColors[(int)Positions[Index].w],1);
    
    return Out;
}

float4 PixelShader( VS_OUTPUT vsout ) : COLOR
{
   float4 res=tex2D(textureSampler, vsout.TextureCoordinate);	
   return float4(res.x*0.3,res.y*0.3,res.z*0.3,1);
   

}

technique TransformTechnique
{
    pass P0
    {
        vertexShader = compile vs_2_0 Transform();
        pixelShader = compile ps_2_0 PixelShader();
        ZEnable = true;
        ZWriteEnable = true;
        AlphaBlendEnable = false;
        AlphaTestEnable = true;
        AlphaFunc = Greater;
        AlphaRef = 200;
    }
}