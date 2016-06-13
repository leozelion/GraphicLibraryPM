using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;


namespace GraphicLibrary
{
    class ModelClass
    {
        struct VertexType
        {
            public Vector4 position;
            public Color4 color;
            public Vector4 normal;
        };

        Buffer m_vertexBuffer, m_indexBuffer;
        public int m_indexCount { get; set; }

        // Initialize the vertex and index buffers.
        public void Initialize(Device device)
        {
            m_indexCount = 3;
            VertexType[] vertices = new VertexType[3];
            short[] indices = new short[3];

            // Load the vertex array with data.
            vertices[0].position = new Vector4(-1.0f, -1.0f, 0.0f, 0.0f);  // Bottom left.
            vertices[0].color = new Color4(1.0f, 0.0f, 0.0f);
            vertices[0].normal = new Vector4(0.0f, 0.0f, -1.0f, 0.0f);

            vertices[1].position = new Vector4(0.0f, 1.0f, 0.0f, 0.0f);  // Top middle.
            vertices[1].color = new Color4(0.0f, 1.0f, 0.0f);
            vertices[1].normal = new Vector4(0.0f, 0.0f, -1.0f, 0.0f);

            vertices[2].position = new Vector4(1.0f, -1.0f, 0.0f, 0.0f);  // Bottom right.
            vertices[2].color = new Color4(0.0f, 0.0f, 1.0f);
            vertices[2].normal = new Vector4(0.0f, 0.0f, -1.0f, 0.0f);

            // Load the index array with data.
            indices[0] = 0;  // Bottom left.
            indices[1] = 1;  // Top middle.
            indices[2] = 2;  // Bottom right.

            DataStream vertexData = new DataStream(System.Runtime.InteropServices.Marshal.SizeOf(new VertexType())*3, true, true);
            vertexData.WriteRange(vertices);
            vertexData.Position = 0;
            m_vertexBuffer = new Buffer(device, vertexData, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(new VertexType()) * 3,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            DataStream indexData = new DataStream(System.Runtime.InteropServices.Marshal.SizeOf(typeof(short))*3, true, true);
            indexData.WriteRange(indices);
            indexData.Position = 0;
            m_indexBuffer = new Buffer(device, indexData, new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                SizeInBytes = sizeof(short) * 3,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });
        }

        // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
        public void Render(DeviceContext context)
        {
            int offset = 0, stride = System.Runtime.InteropServices.Marshal.SizeOf(new VertexType());

            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, stride, offset));

            context.InputAssembler.SetIndexBuffer(m_indexBuffer, Format.R32_UInt, 0);

            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
    }
}
