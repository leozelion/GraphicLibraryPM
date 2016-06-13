// Vertex shader input structure
struct VS_INPUT
{
    float4 Position		: POSITION0;
    float4 Normal		: NORMAL0;
	float4 Color		: COLOR0;
    //float4 PositionDefected   : POSITION1;
	//float4 NormalDefected     : NORMAL1;
};

// Vertex shader output structure
struct VS_OUTPUT
{
    float4 Position		: POSITION;
    float4 Color		: COLOR0;
};


// Global variables
//float4 Color;
//float coef;

// Constant buffer
cbuffer ModelViewProjectionConstantBuffer : register(b0)
{
float4x4 WorldMatrix;  // world matrix for object
float4x4 ViewMatrix;   // view matrix
float4x4 ProjMatrix;   // projection matrix
};


// Name: Simple Vertex Shader
// Type: Vertex shader
// Desc: Vertex transformation and texture coord pass-through
//
VS_OUTPUT VShader( in VS_INPUT In )
{
    VS_OUTPUT Out;                      //create an output vertex

	//Out.Position = mul(In.Position - coef * (In.PositionDefected - In.Position), mul(mul(WorldMatrix, ViewMatrix), ProjMatrix));  //apply vertex transformation
	Out.Position = mul(In.Position, mul(mul(WorldMatrix, ViewMatrix), ProjMatrix));  //apply vertex transformation

    //float4 normal = mul(In.Normal - coef * (In.NormalDefected - In.Normal), WorldMatrix);
	float4 normal = mul(In.Normal, WorldMatrix);
	normal.w = 0.0;
    normal = normalize(normal);
    float4 light = float4(0, 0, -1, 0);

    float diffuse = pow(dot(normal, light), 1);
	
	//float diffuse = pow(dot(normal, light) * 0.5f + 0.5f, 1);
    //if (diffuse < 0.2f) diffuse = 0.2f;
    //if (diffuse > 1.0f) diffuse = 1.0f;
	
	Out.Color = diffuse * Color;
    
	return Out;                         //return output vertex
}