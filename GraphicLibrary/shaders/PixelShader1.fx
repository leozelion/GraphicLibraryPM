texture texture0; //�������� � ������� ����� � � �������
sampler2D texSampler0 : register(s0) = sampler_state
{
    Texture = (texture0);
};

float4 PS(float2 Tex : TEXCOORD0) : COLOR0 
{
  //������� ������� �� �������� � ������� ����� � ������� ��� ��� ���������
  return tex2D(texSampler0, Tex);
}