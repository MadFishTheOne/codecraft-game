uniform extern float4x4 ViewProj : WORLDVIEWPROJECTION;
uniform extern texture tex;
float Detalization;//count of billboards in one blow

#define BatchSize 240
float4 Params[BatchSize];

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
    float T:TEXCOORD1;
};
VS_OUTPUT Transform(
    float4 Pos  : POSITION, 
    float Index : PSIZE, 
    float2 Text : TEXCOORD0 )
{
   VS_OUTPUT Out;
   
   float angle=frac(Index/Detalization)*6.28;
   
   float t=min(max(0,Params[Index].w-frac(angle*23.13467))*2,1);
   
   float currDist=pow(t,2)*(frac(angle*8.13)+1)*0.4;//time 0..1   
   
	Pos.x*=Params[Index].z*currDist;
	Pos.y*=Params[Index].z*currDist;
	float4 currPos=Pos;
	
	//angle=0;
	currPos.x=Pos.x*cos(angle)+(Pos.y)*sin(angle)+Params[Index].x;
	
	currPos.y=-Pos.x*sin(angle)+(Pos.y)*cos(angle)+Params[Index].y;	
    
	Out.Position = mul(currPos,ViewProj);    
    Out.TextureCoordinate =  Text; 
    Out.T=pow(t,1.00);
    return Out;
}

float4 PixelShader( VS_OUTPUT vsout ) : COLOR
{
	float4 res=tex2D(textureSampler, vsout.TextureCoordinate);	
    return float4(res.x,res.y,res.z,res.w*(1-vsout.T)*15.5);
}

technique TransformTechnique
{
    pass P0
    {
        vertexShader = compile vs_2_0 Transform();
        pixelShader = compile ps_2_0 PixelShader();
        ZEnable = true;
        ZWriteEnable = true;
        AlphaBlendEnable = true;
        AlphaTestEnable = true;
        AlphaFunc = Greater;
        AlphaRef = 1;
    }
}