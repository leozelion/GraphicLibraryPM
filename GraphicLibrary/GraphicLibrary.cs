using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D11;
//
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
//
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace Graphic
{
    //коды возврата интерфейсных методов
    public enum ReturnCode
    {
        Success = 0,
        Fail = -1
    };

    #region Форматы Вершин
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformedColoredVertex
    {
        public Vector4 position;
        public int color;

        public static int SizeInBytes
        {
            get { return Marshal.SizeOf(typeof(TransformedColoredVertex)); }
        }

        public static SlimDX.Direct3D9.VertexFormat Format
        {
            get { return SlimDX.Direct3D9.VertexFormat.PositionRhw | SlimDX.Direct3D9.VertexFormat.Diffuse; }
        }

        public TransformedColoredVertex(Vector4 Position, int Color) : this()
        {
            position = Position;
            color = Color;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TransformedTexturedVertex
    {
        public Vector4 position;
        public Vector2 textureCoordinates;

        public static int SizeInBytes
        {
            get { return Marshal.SizeOf(typeof(TransformedTexturedVertex)); }
        }

        public static SlimDX.Direct3D9.VertexFormat Format
        {
            get { return SlimDX.Direct3D9.VertexFormat.PositionRhw | SlimDX.Direct3D9.VertexFormat.Texture1; }
        }

        public TransformedTexturedVertex(Vector4 Position, Vector2 TextureCoordinates) : this()
        {
            position = Position;
            textureCoordinates = TextureCoordinates;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionedTexturedVertex
    {
        public Vector3 position;
        public Vector2 textureCoordinates;

        public static int SizeInBytes
        {
            get { return Marshal.SizeOf(typeof(PositionedTexturedVertex)); }
        }

        public static SlimDX.Direct3D9.VertexFormat Format
        {
            get { return SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Texture1; }
        }

        public PositionedTexturedVertex(Vector3 Position, Vector2 TextureCoordinates) : this()
        {
            position = Position;
            textureCoordinates = TextureCoordinates;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionedColoredNormalVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public int color;
        

        public static int SizeInBytes
        {
            get { return Marshal.SizeOf(typeof(PositionedColoredNormalVertex)); }
        }

        public static SlimDX.Direct3D9.VertexFormat Format
        {
            get { return SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Normal | SlimDX.Direct3D9.VertexFormat.Diffuse; }
        }

        public PositionedColoredNormalVertex(Vector3 Position, int Color, Vector3 Normal) : this()
        {
            position = Position;
            color = Color;
            normal = Normal;
        }
    }
    #endregion

    public delegate float ProgibFunctionDelegate(float x, float y);


    //основной класс
    public partial class GraphicLibrary
    {
        //Устройство рисования Direct3d
        public Device device;

        DeviceContext context;
        SwapChain swapChain;

        //контрол визуализации
        Control ownerControl;

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

        Quaternion orientation = Quaternion.Identity; // Gets the identity Quaternion (0, 0, 0, 1)
        float camera_distance = 50;
        float camera_distance_min = 5;

        //Шейдеры
        Dictionary<string, PixelShader> pixelShaders = new Dictionary<string, PixelShader>();
        Dictionary<string, VertexShader> vertexShaders = new Dictionary<string, VertexShader>();
        ShaderSignature inputSignature;

        //стартовая инициализация
        public ReturnCode Init(Control owner_control)
        {
            lock (lockObject)
            {
                try
                {
                    //инициализация возможна только один раз
                    if (ownerControl != null)
                        return ReturnCode.Fail;

                    //проверка корректности аргументов
                    if (owner_control == null)
                        return ReturnCode.Fail;

                    //сохранение информации о собственнике
                    ownerControl = owner_control;

                    //инициализация графики
                    InitGraphics();

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
            return Surface.FromSwapChain(swapChain, 0); //device.GetBackBuffer(0, 0);
        }

//        private Texture2D CreateTexture(Device device, int width, int height, Func<int, Color> paint)
//        {
//            //initialize a texture
//            Texture2D texture = new Texture2D(device, width, height,0,
//                Usage.RenderTargetOutput,
//                //Usage.None,
//                Format.R8G8B8A8_SNorm,
//                //.A8R8G8B8,
//                Pool.Scratch);
//            //the array holds the color for each pixel in the texture
//            Color[] data = new Color[width * height];
//            for (int pixel = 0; pixel < data.Count(); pixel++)
//            {
//                //the function applies the color according to the specified pixel
//                data[pixel] = paint(pixel);
//            }
//            //set the color
////            texture.Fill(data);

//            return texture;
//        }

        //инициализация и деинициализация графики
        private void InitGraphics()
        {

            var description = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = ownerControl.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, description, out device, out swapChain);
            // create a view of our render target, which is the backbuffer of the swap chain we just created
            RenderTargetView renderTarget;
            using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
                renderTarget = new RenderTargetView(device, resource);

            // setting a viewport is required if you want to actually see anything
            var context = device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, ownerControl.ClientSize.Width, ownerControl.ClientSize.Height);
            context.OutputMerger.SetTargets(renderTarget);
            context.Rasterizer.SetViewports(viewport);



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
            vertexShaders.Clear();
            pixelShaders.Clear();
            string[] resource_names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            for (int i = 0; i < resource_names.Length; i++)
            {
                if (resource_names[i].EndsWith(".fx"))
                {
                    string key = resource_names[i].Substring(resource_names[i].Replace(".fx", "").LastIndexOf('.') + 1).Replace(".fx", "");

                    Stream resource_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource_names[i]);
                    byte[] buffer = new byte[resource_stream.Length];
                    resource_stream.Read(buffer, 0, buffer.Length);

                    ShaderBytecode shader_byte_code;

                    if (resource_names[i].Contains("VertexShader"))
                    { // вершинные шейдеры
                        shader_byte_code = ShaderBytecode.Compile(buffer, "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None);
                        VertexShader vs = new VertexShader(device, shader_byte_code);
                        vertexShaders.Add(key, vs);
                    }
                    else
                    { // пиксельные шейдеры
                        shader_byte_code = ShaderBytecode.Compile(buffer, "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None);
                        PixelShader ps = new PixelShader(device, shader_byte_code);
                        pixelShaders.Add(key, ps);
                    }
                }
            }
        }
        
        private void CloseGraphics()
        {
            foreach (var item in ObjectTable.Objects)
                item.Dispose();
        }

        //завершение модуля
        public ReturnCode Close()
        {
            lock (lockObject)
            {
                try
                {
                    CloseGraphics();

                    return ReturnCode.Success;
                }
                catch
                {
                    return ReturnCode.Fail;
                }
            }
        }

        //изменение размеров у контрола визуализации
        public ReturnCode Resize(Control control)
        {
            lock (lockObject)
            {
                try
                {
                    if (ownerControl != null)
                    {
                        //device.Reset();
                        DeleteAllModels();
                        //CloseGraphics();
                        ownerControl = null;
                        Init(control);
                    }
                    return ReturnCode.Success;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return ReturnCode.Fail;
                }
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

        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        public float Camera_distance
        {
            get { return camera_distance; }
            set { camera_distance = value; }
        }
        
        //зумирование оболочки
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
                    var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0);
                    RenderTargetView renderTarget = new RenderTargetView(device, resource);
                    context.ClearRenderTargetView(renderTarget, backGroundColor);
                    //device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backGroundColor, 1.0f, 10);

                    Matrix world_matrix = Matrix.RotationQuaternion(orientation);
                    Matrix view_matrix = Matrix.LookAtLH(new Vector3(0, 0, -camera_distance), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                    //Matrix perspective_matrix = Matrix.PerspectiveFovLH(
                    //    (float)(Math.PI / 10),
                        
                    //    //device.Viewport.Width / (float)device.Viewport.Height,
                    //    1, 
                    //    1000);
                    var viewport = new Viewport(0.0f, 0.0f, ownerControl.ClientSize.Width, ownerControl.ClientSize.Height);
                    context.OutputMerger.SetTargets(renderTarget);
                    context.Rasterizer.SetViewports(viewport);
                    //device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);

                    context.VertexShader.Set(vertexShaders["VertexShader3"]);
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
                    context.PixelShader.Set(pixelShaders["PixelShader2"]);


                    foreach (ShellBaseModel objectToDraw in objectsToDraw)
                    {
                        objectToDraw.Draw(device, view_matrix);
                    }
                    swapChain.Present(0, PresentFlags.None);
                    //device.Present();
                    return ReturnCode.Success;
                }
                catch (Exception)
                {
                    return ReturnCode.Fail;
                }
            }
        }
    }
}