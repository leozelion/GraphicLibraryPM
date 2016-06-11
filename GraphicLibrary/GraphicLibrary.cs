using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D11;
//
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;
//


namespace Graphic
{
    //коды возврата интерфейсных методов
    public enum ReturnCode
    {
        Success = 0,
        Fail = -1
    };

    //#region Форматы Вершин
    //[StructLayout(LayoutKind.Sequential)]
    //public struct TransformedColoredVertex
    //{
    //    public Vector4 position;
    //    public int color;

    //    public static int SizeInBytes
    //    {
    //        get { return Marshal.SizeOf(typeof(TransformedColoredVertex)); }
    //    }

    //    public static SlimDX.Direct3D9.VertexFormat Format
    //    {
    //        get { return SlimDX.Direct3D9.VertexFormat.PositionRhw | SlimDX.Direct3D9.VertexFormat.Diffuse; }
    //    }

    //    public TransformedColoredVertex(Vector4 Position, int Color) : this()
    //    {
    //        position = Position;
    //        color = Color;
    //    }
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //public struct TransformedTexturedVertex
    //{
    //    public Vector4 position;
    //    public Vector2 textureCoordinates;

    //    public static int SizeInBytes
    //    {
    //        get { return Marshal.SizeOf(typeof(TransformedTexturedVertex)); }
    //    }

    //    public static SlimDX.Direct3D9.VertexFormat Format
    //    {
    //        get { return SlimDX.Direct3D9.VertexFormat.PositionRhw | SlimDX.Direct3D9.VertexFormat.Texture1; }
    //    }

    //    public TransformedTexturedVertex(Vector4 Position, Vector2 TextureCoordinates) : this()
    //    {
    //        position = Position;
    //        textureCoordinates = TextureCoordinates;
    //    }
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //public struct PositionedTexturedVertex
    //{
    //    public Vector3 position;
    //    public Vector2 textureCoordinates;

    //    public static int SizeInBytes
    //    {
    //        get { return Marshal.SizeOf(typeof(PositionedTexturedVertex)); }
    //    }

    //    public static SlimDX.Direct3D9.VertexFormat Format
    //    {
    //        get { return SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Texture1; }
    //    }

    //    public PositionedTexturedVertex(Vector3 Position, Vector2 TextureCoordinates) : this()
    //    {
    //        position = Position;
    //        textureCoordinates = TextureCoordinates;
    //    }
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //public struct PositionedColoredNormalVertex
    //{
    //    public Vector3 position;
    //    public Vector3 normal;
    //    public int color;
        

    //    public static int SizeInBytes
    //    {
    //        get { return Marshal.SizeOf(typeof(PositionedColoredNormalVertex)); }
    //    }

    //    public static SlimDX.Direct3D9.VertexFormat Format
    //    {
    //        get { return SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Normal | SlimDX.Direct3D9.VertexFormat.Diffuse; }
    //    }

    //    public PositionedColoredNormalVertex(Vector3 Position, int Color, Vector3 Normal) : this()
    //    {
    //        position = Position;
    //        color = Color;
    //        normal = Normal;
    //    }
    //}
    //#endregion

    public delegate float ProgibFunctionDelegate(float x, float y);

    //-----------------------------------------------------------------------------
    // Constant buffer data
    //-----------------------------------------------------------------------------
    public struct ModelViewProjectionConstantBuffer
    {
        public Matrix model;
        public Matrix view;
        public Matrix projection;
    };

    //-----------------------------------------------------------------------------
    // Per-vertex data
    //-----------------------------------------------------------------------------
    public struct VertexPositionColor
    {
        public Vector3 pos;
        public Vector3 color;
    };


    //-----------------------------------------------------------------------------
    // Main Class declaration
    //-----------------------------------------------------------------------------
    public partial class GraphicLibrary
    {
        //-----------------------------------------------------------------------------
        // Direct3D device (Устройство рисования (конвейер) Direct3D)
        // Also Direct3D device context and DXGI swap chain.
        //-----------------------------------------------------------------------------
        public Device m_d3dDevice;
        DeviceContext m_d3dContext;
        SwapChain m_swapChain;

        //-----------------------------------------------------------------------------
        // Direct3D device resources
        //-----------------------------------------------------------------------------
        PixelShader m_pixelShader;
        VertexShader m_vertexShader;
        InputLayout m_inputLayout;
        RenderTargetView m_renderTargetView;

        Buffer m_vertexBuffer;
        Buffer m_indexBuffer;
        Buffer m_constantBuffer;

        //-----------------------------------------------------------------------------
        // Global variables for rendering the cube
        //-----------------------------------------------------------------------------
        ModelViewProjectionConstantBuffer m_constantBufferData;

        //int m_frameCount;
        int m_indexCount;

        //контрол визуализации
        Control m_ownerControl;

        //цвет фона
        Color backGroundColor = Color.LightSkyBlue;
        public Color BackGroundColor
        {
            get
            {
                return backGroundColor;
            }
            set
            {
                backGroundColor = value;
            }
        }

        //объекты рисования
        List<ShellBaseModel> objectsToDraw = new List<ShellBaseModel>();

        //объект синхронизации
        object lockObject = new object();

        // положение камеры и положение модели
        float camera_distance = 50;
        float camera_distance_min = 5;
        public float Camera_distance
        {
            get { return camera_distance; }
            set { camera_distance = value; }
        }
        Quaternion orientation = Quaternion.Identity; // Gets the identity Quaternion (0, 0, 0, 1)
        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        //стартовая инициализация
        public ReturnCode Init(Control control)
        {
            lock (lockObject)
            {
                try
                {
                    //инициализация возможна только один раз
                    if (m_ownerControl != null)
                        return ReturnCode.Fail;

                    //проверка корректности аргументов
                    if (control == null)
                        return ReturnCode.Fail;

                    //сохранение информации о собственнике
                    m_ownerControl = control;

                    //инициализация графики
                    InitializeGraphics();

                    return ReturnCode.Success;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return ReturnCode.Fail;
                }
            }
        }

        public Surface GetSurface()
        {
            //Surface.CreateOffscreenPlain(device,
            //    ownerControl.ClientSize.Width, ownerControl.ClientSize.Height,
            //    Format.A8R8G8B8, Pool.Scratch);
            return Surface.FromSwapChain(m_swapChain, 0); //device.GetBackBuffer(0, 0);
        }

        /*private Texture2D CreateTexture(Device device, int width, int height, Func<int, Color> paint)
        //{
        //    //initialize a texture
        //    Texture2D texture = new Texture2D(device, width, height,0,
        //        Usage.RenderTargetOutput,
        //        //Usage.None,
        //        Format.R8G8B8A8_SNorm,
        //        //.A8R8G8B8,
        //        Pool.Scratch);
        //    //the array holds the color for each pixel in the texture
        //    Color[] data = new Color[width * height];
        //    for (int pixel = 0; pixel < data.Count(); pixel++)
        //    {
        //        //the function applies the color according to the specified pixel
        //        data[pixel] = paint(pixel);
        //    }
        //    //set the color
        //    texture.Fill(data);

        //    return texture;
        //}*/

        //-----------------------------------------------------------------------------
        // Create the Direct3D 11 device and device context.
        //-----------------------------------------------------------------------------
        public void CreateDevice()
        {
            m_d3dDevice = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            m_d3dContext = m_d3dDevice.ImmediateContext;
        }

        //-----------------------------------------------------------------------------
        // Create the swap chain, back buffer and viewport.
        //-----------------------------------------------------------------------------
        public void CreateWindowSizeDependentResources(Control control)
        {
            if (m_swapChain != null)
            {
                // If the swap chain already exists, resize it.
                
               m_swapChain.ResizeBuffers(2, 0, 0, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
                    //2, // Double-buffered swap chain.
                    //0,
                    //0, 
                    //Format.B8G8R8A8_UNorm,
                    //0
                    //);
            }
            else
            {
                // Otherwise, create a new one using the same adapter 
                // as the existing Direct3D device.
                SwapChainDescription swapChainDesc = new SwapChainDescription() {
                    BufferCount = 2,
                    Usage = Usage.RenderTargetOutput,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                    SampleDescription = new SampleDescription(1, 0),
                    Flags = SwapChainFlags.None,
                    SwapEffect = SwapEffect.Sequential
                };
                swapChainDesc.OutputHandle = m_ownerControl.Handle;
                // The swapchain must be created with the Factory instance that was used to create the
                // device.
                m_swapChain = new SwapChain(m_d3dDevice.Factory, m_d3dDevice, swapChainDesc);

                OnWindowSizeChanged(control);
                //SampleDescription sampleDesc = swapChainDesc.SampleDescription;
                //sampleDesc.Count = 1; // No multisampling
                //sampleDesc.Quality = 0;
                //swapChainDesc.SampleDescription = sampleDesc;

                //swapChainDesc.SwapEffect = SwapEffect.Sequential; //required

                // Make sure the resource is flagged for use as a render target.
                //swapChainDesc.Usage = Usage.RenderTargetOutput;
                //swapChainDesc.BufferCount = 2; // Double-buffering

                // Swap chain parameters:
                //ModeDescription modeDescription = swapChainDesc.ModeDescription;
                //modeDescription.Width = 0; // Auto size
                //modeDescription.Height = 0;
                //modeDescription.Format = Format.B8G8R8A8_UNorm;
                //modeDescription.Scaling = DisplayModeScaling.Unspecified; // Back buffer scaling
                //swapChainDesc.ModeDescription = modeDescription;

                // This sequence obtains the DXGI factory.
                // First, the DXGI interface for the Direct3D device:
                //Device1 dxgiDevice = new Device1(m_d3dDevice);

                // Then, the adapter hosting the device;
                //Adapter dxgiAdapter = dxgiDevice.Adapter;

                // Then, the factory that created the adapter interface:
                //Factory1 dxgiFactory = dxgiAdapter.GetParent<Factory1>();

                // Finally, use the factory to create the swap chain interface:
                //m_swapChain = new SwapChain(dxgiFactory, dxgiDevice, swapChainDesc);

                // Ensure that DXGI does not queue more than one frame at a time. This both 
                // reduces latency and ensures that the application will only render 
                // after each VSync, minimizing power consumption.
                //dxgiDevice.MaximumFrameLatency = 1;
            }

            // Get the back buffer resource.
            Texture2D backBuffer = Resource.FromSwapChain<Texture2D>(m_swapChain, 0);

            // Create a render target view on the back buffer.
            m_renderTargetView = new RenderTargetView(m_d3dDevice, backBuffer);

            // Set the rendering viewport to target the entire window.
            Texture2DDescription backBufferDesc = backBuffer.Description;

            Viewport viewport = new Viewport(
                0.0f,
                0.0f,
                backBufferDesc.Width,
                backBufferDesc.Height
                );

            m_d3dContext.Rasterizer.SetViewports(viewport);

            // The perspective matrix depends on the viewport aspect ratio.
            CreatePerspectiveMatrix(
                backBufferDesc.Width,
                backBufferDesc.Height
                );
        }

        //-----------------------------------------------------------------------------
        // Create shaders:
        // Load compiled shader objects.
        //-----------------------------------------------------------------------------
        void CreateShaders()
        {
            using (var shader_byte_code = ShaderBytecode.CompileFromFile("VertexShader3.fx", "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None))
            {
                m_vertexShader = new VertexShader(m_d3dDevice, shader_byte_code);

                // This call allocates a device resource and stores the input layout in 
                // graphics memory.
                m_inputLayout = new InputLayout(m_d3dDevice, shader_byte_code, h_VertexDeclarations.PositionNormalVertexElements);

                // Create constant buffer:
                BufferDescription constantBufferDesc = new BufferDescription();
                constantBufferDesc.BindFlags = BindFlags.ConstantBuffer;
                constantBufferDesc.SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(m_constantBufferData);

                // This call allocates a device resource to store constant data for the 
                // vertex shader. The actual constant buffer data will be sent later.
                m_constantBuffer = new Buffer(m_d3dDevice, constantBufferDesc);
            }

            using (var bytecode = ShaderBytecode.CompileFromFile("PixelShader2.fx", "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None))
                m_pixelShader = new PixelShader(m_d3dDevice, bytecode);


            //string[] resource_names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            //for (int i = 0; i < resource_names.Length; i++)
            //{   
            //    // вершинный шейдер
            //    if (resource_names[i].Contains("VertexShader3"))
            //    {
            //        resource_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource_names[i]);
            //        byte[] buffer = new byte[resource_stream.Length];
            //        resource_stream.Read(buffer, 0, buffer.Length);

            //        shader_byte_code = ShaderBytecode.Compile(buffer, "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            //        vs = new VertexShader(m_d3dDevice, shader_byte_code);


            // пиксельный шейдер
            //if (resource_names[i].Contains("PixelShader2"))
            //{
            //resource_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource_names[i]);
            //byte[] buffer = new byte[resource_stream.Length];
            //resource_stream.Read(buffer, 0, buffer.Length);
            //shader_byte_code = ShaderBytecode.Compile(buffer, "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None);
            //ps = new PixelShader(m_d3dDevice, shader_byte_code);
        }

        //-----------------------------------------------------------------------------
        // Create the cube:
        // Creates the vertex buffer and index buffer.
        // Note that these buffers are device resources.
        //-----------------------------------------------------------------------------
        void CreateCube()
        {
            // Create vertex buffer to store cube geometry:
            VertexPositionColor[] CubeVertices =
            {
                new VertexPositionColor() {pos = new Vector3(-0.5f, -0.5f, -0.5f), color = new Vector3(0.0f, 0.0f, 0.0f)},
                new VertexPositionColor() {pos = new Vector3(-0.5f, -0.5f,  0.5f), color = new Vector3(0.0f, 0.0f, 1.0f)},
                new VertexPositionColor() {pos = new Vector3(-0.5f,  0.5f, -0.5f), color = new Vector3(0.0f, 1.0f, 0.0f)},
                new VertexPositionColor() {pos = new Vector3(-0.5f,  0.5f,  0.5f), color = new Vector3(0.0f, 1.0f, 1.0f)},

                new VertexPositionColor() {pos = new Vector3( 0.5f, -0.5f, -0.5f), color = new Vector3(1.0f, 0.0f, 0.0f)},
                new VertexPositionColor() {pos = new Vector3( 0.5f, -0.5f,  0.5f), color = new Vector3(1.0f, 0.0f, 1.0f)},
                new VertexPositionColor() {pos = new Vector3( 0.5f,  0.5f, -0.5f), color = new Vector3(1.0f, 1.0f, 0.0f)},
                new VertexPositionColor() {pos = new Vector3( 0.5f,  0.5f,  0.5f), color = new Vector3(1.0f, 1.0f, 1.0f)},
            };

            DataStream vertexBufferData = new DataStream(CubeVertices, true, true);
            for (int i = 0; i < CubeVertices.Length; i++)
                vertexBufferData.Write(CubeVertices[i]);
            vertexBufferData.Position = 0;

            // The buffer description tells Direct3D how to create this buffer.
            BufferDescription vertexBufferDesc = new BufferDescription();
            vertexBufferDesc.BindFlags = BindFlags.VertexBuffer;
            vertexBufferDesc.SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(CubeVertices);

            //D3D11_SUBRESOURCE_DATA vertexBufferData = { 0 };
            //vertexBufferData.pSysMem = CubeVertices;
            //vertexBufferData.SysMemPitch = 0;
            //vertexBufferData.SysMemSlicePitch = 0;

            // This call allocates a device resource for the vertex buffer and copies
            // in the data.
            m_vertexBuffer = new Buffer(m_d3dDevice, vertexBufferData, vertexBufferDesc);

            // Create index buffer:
            ushort[] cubeIndices =
            {
                0,2,1, // -x
                1,2,3,

                4,5,6, // +x
                5,7,6,

                0,1,5, // -y
                0,5,4,

                2,6,7, // +y
                2,7,3,

                0,4,6, // -z
                0,6,2,

                1,3,7, // +z
                1,7,5,
            };

            m_indexCount = cubeIndices.Length;

            DataStream indexBufferData = new DataStream(CubeVertices, true, true);
            for (int i = 0; i < cubeIndices.Length; i++)
                indexBufferData.Write(cubeIndices[i]);
            indexBufferData.Position = 0;

            BufferDescription indexBufferDesc = new BufferDescription();
            indexBufferDesc.SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(cubeIndices);
            indexBufferDesc.BindFlags = BindFlags.IndexBuffer;

            // This call allocates a device resource for the index buffer and copies
            // in the data.
            m_indexBuffer = new Buffer(m_d3dDevice, indexBufferData, indexBufferDesc);
        }

        //инициализация и деинициализация графики
        private void InitializeGraphics()
        {
            CreateDeviceResources();
            RenderFrame();
            
            // ...

            //var description = new SwapChainDescription()
            //{
            //    BufferCount = 2,
            //    Usage = Usage.RenderTargetOutput,
            //    OutputHandle = m_ownerControl.Handle,
            //    IsWindowed = true,
            //    ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            //    SampleDescription = new SampleDescription(1, 0),
            //    Flags = SwapChainFlags.AllowModeSwitch,
            //    SwapEffect = SwapEffect.Discard
            //};
            //Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, description, out m_d3dDevice, out m_swapChain);
            // create a view of our render target, which is the backbuffer of the swap chain we just created
            //RenderTargetView renderTarget;
            //using (var resource = Resource.FromSwapChain<Texture2D>(m_swapChain, 0))
            //    renderTarget = new RenderTargetView(m_d3dDevice, resource);

            //// setting a viewport is required if you want to actually see anything
            //var context = m_d3dDevice.ImmediateContext;
            //var viewport = new Viewport(0.0f, 0.0f, m_ownerControl.ClientSize.Width, m_ownerControl.ClientSize.Height);
            //context.OutputMerger.SetTargets(renderTarget);
            //context.Rasterizer.SetViewports(viewport);



            //Инициализация параметров представления
            //PresentParameters presentParams = new PresentParameters();
            //presentParams.Windowed = true;
            //presentParams.SwapEffect = SwapEffect.Discard;
            //presentParams.EnableAutoDepthStencil = true;
            //presentParams.PresentationInterval = PresentInterval.Immediate;
            //presentParams.BackBufferWidth = ownerControl.ClientSize.Width;
            //presentParams.BackBufferHeight = ownerControl.ClientSize.Height;
            //presentParams.DeviceWindowHandle = ownerControl.Handle;

            ////создание девайса
            //device = new Device(new Direct3D(), 0, DeviceType.Hardware, ownerControl.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded, presentParams);
            //device.SetRenderState(RenderState.CullMode, Cull.None);

            //загрузка шейдеров
            //CreateShaders();

        }

        // закрытие модуля
        public ReturnCode Close()
        {
            lock (lockObject)
            {
                try
                {
                    foreach (var item in ObjectTable.Objects)
                        item.Dispose();

                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }

        //изменение размеров у контрола визуализации
        public ReturnCode OnWindowSizeChanged(Control sender)
        {
            // Set the render target to null as a signal to recreate window resources.
            try
            {
                //m_renderTargetView = null;
                CreateWindowSizeDependentResources(sender);
                return ReturnCode.Success;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return ReturnCode.Fail;
            }
        }

        public ReturnCode AddModel(ShellBaseModel model)
        {
            lock (lockObject)
            {
                try
                {
                    objectsToDraw.Add(model);

                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }
        public ReturnCode DeleteAllModels()
        {
            lock (lockObject)
            {
                try
                {
                    objectsToDraw.Clear();

                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }

        //поворот оболочки
        // Кватернион
        public ReturnCode RotateModel(Vector3 rotation_axis, float angle)
        {
            lock (lockObject)
            {
                try
                {
                    Quaternion quaternion = Quaternion.RotationAxis(rotation_axis, angle);
                    orientation = quaternion * orientation;
                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }
        // Углы Эйлера
        public ReturnCode RotateModel(float yaw, float pitch, float roll)
        {
            lock (lockObject)
            {
                try
                {
                    Quaternion quaternion = Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
                    orientation = quaternion * orientation;
                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }

        //зумирование оболочки
        // TrackBar
        public ReturnCode ZoomModel(int zoom)
        {
            lock (lockObject)
            {
                try
                {
                    camera_distance += zoom;
                    if (camera_distance <= camera_distance_min)
                        camera_distance = camera_distance_min;

                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }
        // MouseWheel
        // ???
        public ReturnCode ZoomModel(float zoom)
        {
            lock (lockObject)
            {
                try
                {
                    camera_distance *= zoom;
                    if (camera_distance <= camera_distance_min)
                        camera_distance = camera_distance_min;

                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }
        
        //отрисовка
        public ReturnCode Draw()
        {
            lock (lockObject)
            {
                try
                {
                    var resource = Resource.FromSwapChain<Texture2D>(m_swapChain, 0);
                    RenderTargetView renderTarget = new RenderTargetView(m_d3dDevice, resource);
                    m_d3dContext.ClearRenderTargetView(renderTarget, backGroundColor);
                    //device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backGroundColor, 1.0f, 10);

                    Matrix world_matrix = Matrix.RotationQuaternion(orientation);
                    Matrix view_matrix = Matrix.LookAtLH(new Vector3(0, 0, -camera_distance), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                    //Matrix perspective_matrix = Matrix.PerspectiveFovLH(
                    //    (float)(Math.PI / 10),
                        
                    //    //device.Viewport.Width / (float)device.Viewport.Height,
                    //    1, 
                    //    1000);
                    var viewport = new Viewport(0.0f, 0.0f, m_ownerControl.ClientSize.Width, m_ownerControl.ClientSize.Height);
                    m_d3dContext.OutputMerger.SetTargets(renderTarget);
                    m_d3dContext.Rasterizer.SetViewports(viewport);
                    //device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);

                    m_d3dContext.VertexShader.Set(m_vertexShader);
                    //device.VertexFormat = PositionedColoredNormalVertex.Format;

                    //device.VertexShader = vertexShaders["VertexShader3"];
                    //try
                    //{
                    //    device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("WorldMatrix"), world_matrix);
                    //    device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("ViewMatrix"), view_matrix);
                    //    device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("ProjMatrix"), perspective_matrix);
                    //}
                    //catch { }
                    //device.PixelShader = pixelShaders["PixelShader2"];
                    m_d3dContext.PixelShader.Set(m_pixelShader);


                    foreach (ShellBaseModel objectToDraw in objectsToDraw)
                    {
                        objectToDraw.Draw(m_d3dDevice, view_matrix);
                    }
                    m_swapChain.Present(1, PresentFlags.None);
                    //device.Present();
                    return ReturnCode.Success;
                }
                catch (Exception)
                {
                    return ReturnCode.Fail;
                }
            }
        }


        //-----------------------------------------------------------------------------
        // Create the view matrix.
        //-----------------------------------------------------------------------------
        private void CreateViewMatrix()
        {
            // Create the constant buffer data in system memory.
            Vector3 eye = new Vector3(0.0f, 0.7f, 1.5f);
            Vector3 at = new Vector3(0.0f, -0.1f, 0.0f);
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            m_constantBufferData.view = Matrix.Transpose(Matrix.LookAtRH(eye, at, up));
        }

        //-----------------------------------------------------------------------------
        // Create the perspective matrix.
        //-----------------------------------------------------------------------------
        private void CreatePerspectiveMatrix(float width, float height)
        {
            // Set up the perspective matrix with the correct aspect ratio.
            float aspectRatio = width / height;
            float fovAngleY = 70.0f * (float)Math.PI / 180.0f;

            m_constantBufferData.projection = Matrix.Transpose(
                Matrix.PerspectiveFovRH(
                    fovAngleY,
                    aspectRatio,
                    0.01f,
                    100.0f));
        }

        //-----------------------------------------------------------------------------
        // Create device-dependent resources.
        //-----------------------------------------------------------------------------
        private void CreateDeviceResources()
        {
            // Create the D3D11 device and context.
            CreateDevice();

            // Load shader binaries.
            CreateShaders();

            // Load the geometry for the spinning cube.
           // CreateCube();

            // Create the view matrix.
            CreateViewMatrix();

            // Since the perspective matrix depends on the viewport aspect ratio, it is
            // created by CreateWindowSizeDependentResources().
        }

        //-----------------------------------------------------------------------------
        // Render the cube.
        //-----------------------------------------------------------------------------
        public void RenderFrame()
        {
            //-------------------
            //КОСТЫЛЬ
            OnWindowSizeChanged(m_ownerControl);
            //-------------------

            // This function primarily uses the Direct3D 11 device context. !!!

            // Clear the back buffer.
            m_d3dContext.ClearRenderTargetView(m_renderTargetView, backGroundColor);

            // Set the render target. This starts the drawing operation.
            m_d3dContext.OutputMerger.SetTargets(m_renderTargetView);

            // на будущее ...
            //// Rotate the cube 1 degree per frame.
            //XMStoreFloat4x4(
            //    &m_constantBufferData.model,
            //    XMMatrixTranspose(XMMatrixRotationY(m_frameCount++ * XM_PI / 180.f))
            //    );

            // Copy the updated constant buffer from system memory to video memory.
            DataStream data = new DataStream(System.Runtime.InteropServices.Marshal.SizeOf(m_constantBufferData), true, true);
            data.Write<ModelViewProjectionConstantBuffer>(m_constantBufferData);
            DataBox source = new DataBox(0, 0, data);
            m_d3dContext.UpdateSubresource(source, m_constantBuffer, 0);

            // Send vertex data to the Input Assembler stage.
            m_d3dContext.InputAssembler.SetVertexBuffers(
                0,
                new VertexBufferBinding(
                    m_vertexBuffer,
                    System.Runtime.InteropServices.Marshal.SizeOf(new VertexPositionColor() //{ pos = new Vector3(0, 0, 0), color = new Vector3(0,0,0) }
                    ), // stride
                    0) // no offset
                );

            m_d3dContext.InputAssembler.SetIndexBuffer(
                m_indexBuffer, 
                Format.R16_UInt, 
                0); // no offset

            m_d3dContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            m_d3dContext.InputAssembler.InputLayout = m_inputLayout;

            // Set the vertex shader.
            m_d3dContext.VertexShader.Set(m_vertexShader);

            // Set the vertex shader constant buffer data.
            m_d3dContext.VertexShader.SetConstantBuffer(m_constantBuffer, 0);

            // Set the pixel shader.
            m_d3dContext.PixelShader.Set(m_pixelShader);

            // Draw the cube.
            m_d3dContext.DrawIndexed(
                m_indexCount,
                0,  // start with index 0
                0   // start with vertex 0
                );

            // Present the frame by swapping the back buffer to the screen.
            m_swapChain.Present(1, PresentFlags.None);
        }
    }
}