using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace GraphicLibrary
{
    class ShaderClass
    {
        // const buffer layout for VS
        struct ConstantBufferType
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        };

        private VertexShader m_vertexShader;
        private PixelShader m_pixelShader;
        private InputLayout m_inputLayout;
        private SamplerState m_sampleState;
        private Buffer m_constantBuffer;

        public void InitializeShader(Device device)
        {
            InputElement[] inputElementLayout = new InputElement[3];

            var shader_byte_code = ShaderBytecode.CompileFromFile(@"C:\Users\12642\OneDrive\ДИПЛОМ\GRAPHICLIBRARY\GraphicLibrary\GraphicLibraryPM\GraphicLibrary\VertexShader3.fx", "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            m_vertexShader = new VertexShader(device, shader_byte_code);

            shader_byte_code = ShaderBytecode.CompileFromFile(@"C:\Users\12642\OneDrive\ДИПЛОМ\GRAPHICLIBRARY\GraphicLibrary\GraphicLibraryPM\GraphicLibrary\PixelShader2.fx", "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None);
            m_pixelShader = new PixelShader(device, shader_byte_code);

            //Define first 16 floats as Position, next 16 as Normal, next 16 as Color (4 cords, 4 normal, 4 Color parts)
            inputElementLayout[0] = new InputElement(
                "POSITION", 0,
                Format.R32G32B32A32_Float,
                0,
                0,
                InputClassification.PerVertexData,
                0);
            inputElementLayout[1] = new InputElement(
                "NORMAL", 0,
                Format.R32G32B32A32_Float,
                16,
                0,
                InputClassification.PerVertexData,
                0);
            inputElementLayout[2] = new InputElement(
                "COLOR", 0,
                Format.R32G32B32A32_Float,
                32,
                0,
                InputClassification.PerVertexData,
                0);

            // This call allocates a device resource and stores the input layout in 
            // graphics memory.
            m_inputLayout = new InputLayout(device, shader_byte_code, inputElementLayout);

            // constant buffer description:
            BufferDescription constantBufferDesc = new BufferDescription()
            {
                BindFlags = BindFlags.ConstantBuffer,
                SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(new ConstantBufferType()),
                Usage=ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None, // Warning!
                StructureByteStride = 0,
                OptionFlags = 0
            };

            // create constant buffer (empty)
            m_constantBuffer = new Buffer(device,null, constantBufferDesc);

            // set constant buffer to vertex shader
            device.ImmediateContext.VertexShader.SetConstantBuffer(m_constantBuffer, 0);
        }

        public void Render(DeviceContext context, int indexCount, Matrix world, Matrix view, Matrix projection)
        {
            SetShaderParameters(context, world, view, projection);
            RenderShader(context, indexCount);
        }

        private void SetShaderParameters(DeviceContext context, Matrix world, Matrix view, Matrix projection)
        {
            //

            // create data for buffer
            ConstantBufferType data = new ConstantBufferType() { world = world, view = view, projection = projection };

            // write data to stream
            var dataStream = new DataStream(System.Runtime.InteropServices.Marshal.SizeOf(new ConstantBufferType()), true, true);
            dataStream.Write(data);
            dataStream.Position = 0;

            // update constant buffer from stream (buffer already linked to VertexShader)
            context.UpdateSubresource(new DataBox(0, 0, dataStream), m_constantBuffer, 0);
        }

        private void RenderShader(DeviceContext context, int indexCount)
        {
            context.InputAssembler.InputLayout = m_inputLayout;
            context.VertexShader.Set(m_vertexShader);
            context.PixelShader.Set(m_pixelShader);

            //context.DrawIndexed(indexCount, 0, 0);
            context.Draw(3, 0);
        }

    }
}
