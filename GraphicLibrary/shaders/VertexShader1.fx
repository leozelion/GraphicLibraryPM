// Vertex shader input structure
struct VS_INPUT
{
    float4 Position   : POSITION;
    int Color         : COLOR0;
};


// Vertex shader output structure
struct VS_OUTPUT
{
    float4 Position   : POSITION;
    int Color    : COLOR0;
};

// Global variables
float4x4 WorldViewProj;

// Name: Simple Vertex Shader
// Type: Vertex shader
// Desc: Vertex transformation and texture coord pass-through
//
VS_OUTPUT VS( in VS_INPUT In )
{
    VS_OUTPUT Out;                      //create an output vertex

    Out.Position = mul(In.Position,
                       WorldViewProj);  //apply vertex transformation
    Out.Color  = In.Color;          //copy original texcoords

    return Out;                         //return output vertex
}