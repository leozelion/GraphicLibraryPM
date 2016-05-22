// Vertex shader input structure
struct VS_INPUT
{
    float4 Position   : POSITION;
    float4 Color      : COLOR0;
    float4 Normal     : NORMAL;
};


// Vertex shader output structure
struct VS_OUTPUT
{
    float4 Position   : POSITION;
    float4 Color      : COLOR0;
};


// Global variables
float4x4 WorldMatrix;
float4x4 ViewMatrix;
float4x4 ProjMatrix;

// Name: Simple Vertex Shader
// Type: Vertex shader
// Desc: Vertex transformation and texture coord pass-through
//
VS_OUTPUT VS( in VS_INPUT In )
{
    VS_OUTPUT Out;                      //create an output vertex

	Out.Position = mul(In.Position, mul(mul(WorldMatrix, ViewMatrix), ProjMatrix));  //apply vertex transformation

    float4 normal = mul(In.Normal, WorldMatrix);
    normal.w = 0.0;
    normal = normalize(normal);
    float4 light = float4(0, 0, -1, 0);

    float diffuse = pow(dot(normal, light), 1);
	//float diffuse = pow(dot(normal, light) * 0.5f + 0.5f, 1);
    //if (diffuse < 0.5f) diffuse = 0.5f;
    //if (diffuse > 1.0f) diffuse = 1.0f;
	
	Out.Color = diffuse * In.Color;
    
	return Out;                         //return output vertex
}