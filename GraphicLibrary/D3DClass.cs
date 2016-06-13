using System;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

namespace GraphicLibrary
{
    class D3DClass
    {
        bool m_vsync_enabled;
        SwapChain m_swapChain;
        Device m_device;
        DeviceContext m_deviceContext;
        RenderTargetView m_renderTargetView;
        Texture2D m_depthStencilBuffer;
        //DepthStencilState m_depthStencilState;
        DepthStencilView m_depthStencilView;
        RasterizerState m_rasterState;

        public Matrix m_projectionMatrix;

        public Device Device { get { return m_device; } }
        public DeviceContext Context { get { return m_deviceContext; } }

        public void Initialize(Control control, bool vsync)
        {
            m_vsync_enabled = vsync;

            CreateDeviceResources(control);

            // prevent DXGI handling of alt+enter, prt scrn, etc which doesn't work properly with Winforms
            using (var factory = m_swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(control.Handle, WindowAssociationFlags.IgnoreAll);

            //Generate Depthbuffer and DepthBufferView
            m_depthStencilBuffer = new Texture2D(m_device, new Texture2DDescription()
            {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = control.ClientSize.Width,
                Height = control.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            });

            /*
            m_depthStencilState = DepthStencilState.FromDescription(m_device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    // Stencil operations if pixel is front-facing.
                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    },

                    // Stencil operations if pixel is back-facing.
                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    }
                });

            // Set the depth stencil state.
            m_deviceContext.OutputMerger.DepthStencilState = m_depthStencilState;
            m_deviceContext.OutputMerger.DepthStencilReference = 1;
            */

            // Create the depth stencil view.
            m_depthStencilView = new DepthStencilView(m_device, m_depthStencilBuffer,
                new DepthStencilViewDescription() {
                Format=Format.D16_UNorm,
                Dimension=DepthStencilViewDimension.Texture2D,
                MipSlice=0
                });

            //Define Rasterizer
            m_rasterState = RasterizerState.FromDescription(m_device, new RasterizerStateDescription()
                {
                    // Setup the raster description which will determine how and what polygons will be drawn.
                    CullMode = CullMode.None,//Back,
                    FillMode = FillMode.Solid,
                    IsAntialiasedLineEnabled = true, // false
                    IsFrontCounterclockwise = true, // false
                    IsMultisampleEnabled = true, // false
                    IsDepthClipEnabled = true,
                    IsScissorEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f
                });

            m_deviceContext.Rasterizer.State = m_rasterState;

            // create swapChain, renderTargetView, set ViewPort and ViewMatrix
            CreateWindowSizeDependentResources(control);

            //m_worldMatrix = Matrix.Identity;
        }

        public void OnWindowSizeChanged(Control control)
        {
            m_renderTargetView.Dispose();
            CreateWindowSizeDependentResources(control);
        }

        private void CreateDeviceResources(Control control)
        {
            
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(control.ClientSize.Width,
                    control.ClientSize.Height,
                    new Rational(60, 1),
                    Format.B8G8R8A8_UNorm),
                IsWindowed = true,
                OutputHandle = control.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };
            
            // Create the D3D11 device and context.
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out m_device, out m_swapChain);
            //m_device = new Device(DriverType.Hardware, DeviceCreationFlags.None);

            m_deviceContext = m_device.ImmediateContext;

            // Create the view matrix.
            //CreateViewMatrix();
        }

        public void CreateWindowSizeDependentResources(Control control)
        {
            if (m_swapChain != null)
            {
                // If the swap chain already exists, resize it.
                m_swapChain.ResizeBuffers(2, 0, 0, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            }
            else
            {
                /*
                // Otherwise, create a new one using the same adapter 
                // as the existing Direct3D device.
                SwapChainDescription swapChainDesc = new SwapChainDescription()
                {
                    BufferCount = 2,
                    ModeDescription = new ModeDescription(
                        control.Width,
                        control.Height,
                        new Rational(60, 1),
                        Format.B8G8R8A8_UNorm),
                    IsWindowed = true,
                    OutputHandle = control.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,//Sequential,
                    Flags = SwapChainFlags.None,
                    Usage = Usage.RenderTargetOutput
                };

                // The swapchain must be created with the Factory instance that was used to create the
                // device.
                m_swapChain = new SwapChain(m_device.Factory, m_device, swapChainDesc);
                */
            }

            // Create a render target view on the back buffer.
            Texture2D backBuffer = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(m_swapChain, 0);
            m_renderTargetView = new RenderTargetView(m_device, backBuffer);

            // Set the rendering viewport to target the entire window.
            Texture2DDescription backBufferDesc = backBuffer.Description;

            Viewport viewport = new Viewport(
                0.0f,
                0.0f,
                backBufferDesc.Width,
                backBufferDesc.Height,
                0.0f,
                1.0f
                );
            m_deviceContext.Rasterizer.SetViewports(viewport);

            m_deviceContext.OutputMerger.SetTargets(m_depthStencilView, m_renderTargetView);

            

            // The perspective matrix depends on the viewport aspect ratio.
            m_projectionMatrix = CreatePerspectiveMatrix(backBufferDesc.Width, backBufferDesc.Height);
        }

        private Matrix CreatePerspectiveMatrix(float width, float height)
        {
            // Set up the perspective matrix with the correct aspect ratio.
            float aspectRatio = width / height;
            float fovAngleY = 70.0f * (float)Math.PI / 180.0f; // field of view

            return Matrix.Transpose(
                Matrix.PerspectiveFovRH(
                    fovAngleY,
                    aspectRatio,
                    0.01f,
                    100.0f));
        }

        public void BeginScene(Color4 color)
        {
            // Clear the back buffer.
            m_deviceContext.ClearRenderTargetView(m_renderTargetView, color);

            // Clear the depth buffer.
            m_deviceContext.ClearDepthStencilView(m_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public void EndScene()
        {
            try
            {
                // Present the back buffer to the screen since rendering is complete.
                if (!m_vsync_enabled)
                {
                    // Lock to screen refresh rate.
                    m_swapChain.Present(1, 0);
                }
                else
                {
                    // Present as fast as possible.
                    m_swapChain.Present(0, 0);
                }
            }
            catch (DXGIException)
            {
                MessageBox.Show(m_device.DeviceRemovedReason.Description);
                return;
            }
        }

        public System.IO.Stream GetImageFromSwapChain(ImageFileFormat format)
        {
            var stream = System.IO.Stream.Null;
            var texture = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(m_swapChain, 1);
            Texture2D.SaveTextureToFile(m_deviceContext, texture, format, "graphic_library");
            //Texture2D.ToStream(m_deviceContext, texture, format, stream);
            return stream;
        }
    }
}
