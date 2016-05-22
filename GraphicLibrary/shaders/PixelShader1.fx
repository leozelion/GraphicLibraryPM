texture texture0; //текстура с верхним слоем и её сэмплер
sampler2D texSampler0 : register(s0) = sampler_state
{
    Texture = (texture0);
};

float4 PS(float2 Tex : TEXCOORD0) : COLOR0 
{
  //извлечь тексель из текстуры с верхним слоем и вернуть его как результат
  return tex2D(texSampler0, Tex);
}