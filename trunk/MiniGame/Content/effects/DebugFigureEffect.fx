uniform extern float4x4 ViewProj : WORLDVIEWPROJECTION;
#define BatchSize 120
float4 Params1[BatchSize];
float4 Params2[BatchSize];

struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float4 Color:TEXCOORD1;
    float RadiusSq:TEXCOORD2;
};

VS_OUTPUT Transform(
    float4 Pos  : POSITION, 
    float Index : PSIZE, 
    float2 Text : TEXCOORD0 )
{
   VS_OUTPUT Out;
	Pos.x*=Params1[Index].z*0.5;
	Pos.y*=Params1[Index].w*0.5;
	float4 currPos=float4(0,0,0,1);
	currPos.x=Pos.x*cos(Params2[Index].x)-Pos.y*sin(Params2[Index].x)+Params1[Index].x;
	currPos.y=Pos.x*sin(Params2[Index].x)+Pos.y*cos(Params2[Index].x)+Params1[Index].y;	
    
	Out.Position = mul(currPos,ViewProj);    
    Out.TextureCoordinate = Text;//float2(Text.y,Text.x);   
    float colorComponent;
    Out.Color.r=frac(Params2[Index].w);
    colorComponent=(Params2[Index].w-Out.Color.r)*0.1;
    Out.Color.g=frac(colorComponent);
    colorComponent=(colorComponent-Out.Color.g)*0.1;
    Out.Color.b=frac(colorComponent);
    colorComponent=(colorComponent-Out.Color.b)*0.1;
    Out.Color.a=frac(colorComponent);
    if (Params2[Index].y>0.5)
    Out.RadiusSq=0.25;
    else 
    Out.RadiusSq=1;
    
    
    return Out;
}

float4 PixelShader( VS_OUTPUT vsout ) : COLOR
{   
float4 res;
if (((vsout.TextureCoordinate.x-0.5)*(vsout.TextureCoordinate.x-0.5)+
    (vsout.TextureCoordinate.y-0.5)*(vsout.TextureCoordinate.y-0.5))<vsout.RadiusSq)
	return vsout.Color;
	else return float4(0,0,0,0);
}

technique TransformTechnique
{
    pass P0
    {
        vertexShader = compile vs_2_0 Transform();
        pixelShader = compile ps_2_0 PixelShader();
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = true;
        AlphaTestEnable = true;
        AlphaFunc = Greater;
        AlphaRef = 1;
    }
}