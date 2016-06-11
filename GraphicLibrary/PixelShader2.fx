float4 PShader(float4 color : COLOR0) : SV_TARGET
{
  float4 result = float4(1, 0, 1, 1);
  return color;
}