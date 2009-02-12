uniform extern float4x4 ViewProj : WORLDVIEWPROJECTION;
uniform extern texture tex,tex2;
#define BatchSize 120
float4 Positions[BatchSize];
float3 Params[BatchSize];

sampler textureSampler = sampler_state
{
    Texture=<tex>;
    MipFilter=Linear;
    MagFilter=Linear;
    MinFilter=Linear;
};

sampler textureSampler2 = sampler_state
{
    Texture=<tex2>;
    MipFilter=Linear;
    MagFilter=Linear;
    MinFilter=Linear;
};
struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 TextureCoordinate : TEXCOORD0;
     float2 AlphaAndTexNumber: TEXCOORD1;
};

VS_OUTPUT Transform(
    float4 Pos  : POSITION, 
    float Index : PSIZE, 
    float2 Text : TEXCOORD0 )
{
   VS_OUTPUT Out;
	
	float4 currPos=Pos;    
    
    float3 Start=float3( Positions[Index].x,Positions[Index].y,0);
    float3 End=float3( Positions[Index].z,Positions[Index].w,0);
    
    float3 center=(Start+End)*0.5;
    Matrix p;
	float3 x;
	float3 y = (End - Start);	
	
	x=  cross(float3(0,0,-1) ,y);
	float3 z = cross(x,y);

	x= normalize(x)*Params[Index].z;
	y = normalize(y)*distance(Start,End);
	z = normalize(z);
	p._m00 = x.x;   p._m10 =y.x;    p._m20 = z.x;  p._m30 = center.x;
	p._m01 = x.y;   p._m11 = y.y;   p._m21 = z.y;  p._m31 = center.y;
	p._m02 = x.z;   p._m12 = y.z;   p._m22 = z.z;  p._m32 = center.z;
	
	
	p._m03 =   0;   p._m13 =   0;   p._m23 =    0; p._m33 =    1;
	Out.Position = mul(currPos,p);    
	Out.Position = mul(Out.Position,ViewProj);    
    Out.TextureCoordinate =  Text;
    Out.AlphaAndTexNumber.x=1-Params[Index].y;
    Out.AlphaAndTexNumber.y=Params[Index].x;
    
    return Out;
}

float4 PixelShader( VS_OUTPUT vsout ) : COLOR
{
 float4 res;
 if (vsout.AlphaAndTexNumber.y<0)
  res= tex2D(textureSampler, vsout.TextureCoordinate);	
 else
  res= tex2D(textureSampler2, vsout.TextureCoordinate);	
 return float4(res.x,res.y,res.z,res.w*vsout.AlphaAndTexNumber.x*10);
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