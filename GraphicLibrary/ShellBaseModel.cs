using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;
using System.Collections.Generic;

namespace Graphic
{
    public abstract class ShellBaseModel
    {
        //public abstract void SetAutoAnimation(bool value);
        //public abstract void SetAnimationWeight(float value);

    protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesMiddle;
    protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesTop;
    protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesBottom;
    protected   short[] indicesTopMiddleBottom;
    protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeFront;
    protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeBack;
    protected   short[] indicesEdgeFrontBack;
    protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeLeft;
    protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeRight;
        protected short[] indicesEdgeLeftRight;
        
     protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesTopDefected;
     protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesBottomDefected;
     protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeFrontDefected;
     protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeBackDefected;
     protected   h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeLeftDefected;
        protected h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeRightDefected;

      protected  Buffer vbTop;
      protected  Buffer vbTopDefected;
      protected  Buffer vbBottom;
      protected  Buffer vbBottomDefected;
      protected  Buffer vbEdgeFront;
      protected  Buffer vbEdgeFrontDefected;
      protected  Buffer vbEdgeBack;
      protected  Buffer vbEdgeBackDefected;
      protected  Buffer vbEdgeLeft;
      protected  Buffer vbEdgeLeftDefected;
      protected  Buffer vbEdgeRight;
      protected  Buffer vbEdgeRightDefected;
      protected  Buffer ibTopMiddleBottom;
      protected  Buffer ibEdgeFrontBack;
        protected Buffer ibEdgeLeftRight;

        public Dictionary<int, string> animateProgFunMass;

        protected Device device;

        protected Buffer createVertexBuffer(h_VertexDeclarations.h_PositionNormalVertex[] surface)
        {
            
            DataStream vertexBufferData = new DataStream(h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes * surface.Length, true, true);
            vertexBufferData.WriteRange<h_VertexDeclarations.h_PositionNormalVertex>(surface);
            vertexBufferData.Position = 0;
            return new Buffer(
                device, 
                vertexBufferData, 
                h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes * surface.Length, 
                ResourceUsage.Default, 
                BindFlags.VertexBuffer, 
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 
                0);
        }

        protected Buffer createIndexBuffer(short[] surface)
        {
            DataStream indexBufferData = new DataStream(sizeof(ushort) * surface.Length, true, true);
            indexBufferData.WriteRange(surface);
            indexBufferData.Position = 0;
            return new Buffer(
                device,
                indexBufferData,
                sizeof(ushort) * surface.Length,
                ResourceUsage.Default,
                BindFlags.VertexBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
        }

        //отрисовка
        public void Draw(Device device, Matrix matrix)
        {
            if (autoAnimation)
            {
                //if(base.animateProgFunMass!= null)
                //{
                //    BuildModel();
                //}
                animationWeight += (2 * direction - 1) * 0.005f;
                if (animationWeight >= 1)
                {
                    animationWeight = 1;
                    /*if (ticks == 50)
                    {
                        direction = 0;
                        ticks = 0;
                    }
                    else
                    {
                        ticks++;
                        direction = 0.5f;
                    }*/
                }
                if (animationWeight <= 0)
                {
                    animationWeight = 0;
                    if (ticks == 50)
                    {
                        direction = 1;
                        ticks = 0;
                    }
                    else
                    {
                        ticks++;
                        direction = 0.5f;
                    }
                }
            }

            DeviceContext d3dContext = device.ImmediateContext;
            int normal_size = h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes;
            d3dContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;


            //device.VertexDeclaration = new VertexDeclaration(device, h_VertexDeclarations.PositionNormalVertexElements);

            //
            //??? 
            //Effect fx = new Effect(device,new SlimDX.D3DCompiler.ShaderBytecode())
            //fx.

            //device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("coef"), animationWeight);

            //верхняя поверхность
            //
            //??? 
            //device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("Color"), new float[4] { 1.0f, 1.0f, 1.0f, 1.0f });

            d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vbTop, normal_size, 0));
            d3dContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(vbTopDefected, normal_size, 0));
            d3dContext.InputAssembler.SetIndexBuffer(ibTopMiddleBottom, Format.R16_UInt, 0);
            d3dContext.DrawIndexed(indicesTopMiddleBottom.Length, 0, 0);

            //нижняя поверхность
            //
            //??? 
            //device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("Color"), new float[4] { 0.7f, 0.7f, 0.7f, 1.0f });

            d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vbBottom, normal_size, 0));
            d3dContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(vbBottomDefected, normal_size, 0));
            d3dContext.InputAssembler.SetIndexBuffer(ibTopMiddleBottom, Format.R16_UInt, 0);
            d3dContext.DrawIndexed(indicesTopMiddleBottom.Length, 0, 0);

            //боковые грани
            //
            //??? 
            //device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("Color"), new float[4] { 0.3f, 0.3f, 0.3f, 1 });

            d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vbEdgeFront, normal_size, 0));
            d3dContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(vbEdgeFrontDefected, normal_size, 0));
            d3dContext.InputAssembler.SetIndexBuffer(ibEdgeFrontBack, Format.R16_UInt, 0);
            d3dContext.DrawIndexed(indicesEdgeFrontBack.Length, 0, 0);

            d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vbEdgeBack, normal_size, 0));
            d3dContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(vbEdgeBackDefected, normal_size, 0));
            d3dContext.InputAssembler.SetIndexBuffer(ibEdgeFrontBack, Format.R16_UInt, 0);
            d3dContext.DrawIndexed(indicesEdgeFrontBack.Length, 0, 0);

            d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vbEdgeLeft, normal_size, 0));
            d3dContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(vbEdgeLeftDefected, normal_size, 0));
            d3dContext.InputAssembler.SetIndexBuffer(ibEdgeLeftRight, Format.R16_UInt, 0);
            d3dContext.DrawIndexed(indicesEdgeLeftRight.Length, 0, 0);

            d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vbEdgeRight, normal_size, 0));
            d3dContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(vbEdgeRightDefected, normal_size, 0));
            d3dContext.InputAssembler.SetIndexBuffer(ibEdgeLeftRight, Format.R16_UInt, 0);
            d3dContext.DrawIndexed(indicesEdgeLeftRight.Length, 0, 0);

        }

        //коэф-т анимации
        protected float direction = 1;
        protected float animationWeight = 0;
        public float AnimationWeight
        {
            get
            {
                return animationWeight;
            }
            set
            {
                animationWeight = value;
            }
        }
       protected bool autoAnimation = false;
        public bool AutoAnimation
        {
            get
            {
                return autoAnimation;
            }
            set
            {
                autoAnimation = value;
                if (value == true)
                {
                    animationWeight = 0;
                }
            }
        }

        public void SetAnimationWeight(float value)
        {
            AnimationWeight = value;
        }
        public void SetAutoAnimation(bool value)
        {
            AutoAnimation = value;
        }

        protected int ticks;
    }
}
