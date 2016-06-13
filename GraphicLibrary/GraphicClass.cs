using SlimDX;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.Direct3D11;

namespace GraphicLibrary
{
    class GraphicClass
    {
        D3DClass m_D3D;
        CameraClass m_Camera;
        ModelClass m_Model;
        ShaderClass m_Shader;

        public Color4 BackgroundColor { get; set; }

        public void Initialize(Control control, int cam_minDist, int cam_maxDist, Color4 color)
        {
            BackgroundColor = color;

            m_D3D = new D3DClass();
            m_D3D.Initialize(control, false);

            m_Camera = new CameraClass(cam_minDist, cam_maxDist);
            m_Camera.CameraDistance = 20;

            m_Model = new ModelClass();
            m_Model.Initialize(m_D3D.Device);

            m_Shader = new ShaderClass();
            m_Shader.InitializeShader(m_D3D.Device);
        }

        public void Frame()
        {
            // rotation here

            Render();
        }

        private void Render()
        {
            Matrix worldMatrix, viewMatrix, projectionMatrix;

            // Clear the buffers with color to begin the scene.
            m_D3D.BeginScene(BackgroundColor);

            // Generate the view matrix based on the camera's position.
            m_Camera.Render();

            // Get the world, view, and projection matrices from the camera and d3d objects.
            viewMatrix = m_Camera.m_viewMatrix;
            worldMatrix = m_Camera.m_worldMatrix;
            projectionMatrix = m_D3D.m_projectionMatrix;

            // rotation here

            // Put the model vertex and index buffers on the graphics pipeline to prepare them for drawing.
            m_Model.Render(m_D3D.Context);

            // Render the model using the shader.
            m_Shader.Render(m_D3D.Context, m_Model.m_indexCount, worldMatrix, viewMatrix, projectionMatrix);

            // Present the rendered scene to the screen.
            m_D3D.EndScene();
        }

        public int ZoomModel(int zoom)
        {
            int dist = m_Camera.Zoom(zoom);
            Render();
            return dist;
        }

        public void RotateModel(Control control, Point start, Point stop, out float Teta, out float Phi, out float Psi)
        {
            Size clientSize = control.Size;

            float startX = 1 - start.X / (clientSize.Width / 2f); // переводим X из [0..W] в [-1..1]
            float startY = start.Y / (clientSize.Height / 2f) - 1; // то же самое
            float startZ = (float)Math.Sqrt(Math.Abs(1 - startX * startX - startY * startY)); // получаем расстояние от 0 до (x,y)
                                                                                              // как будто в квадрате со стороной = 2
            float stopX = 1 - stop.X / (clientSize.Width / 2f);
            float stopY = stop.Y / (clientSize.Height / 2f) - 1;
            float stopZ = (float)Math.Sqrt(Math.Abs(1 - stopX * stopX - stopY * stopY));

            // получили вектора трёхмерного пространства,
            // где Z - радиус сферы, которая получается из окружности вектора (start,stop)
            Vector3 startV = new Vector3(startX, startY, startZ);
            Vector3 stopV = new Vector3(stopX, stopY, stopZ);
            startV.Normalize();
            stopV.Normalize();

            Vector3 rotationAxis = Vector3.Cross(startV, stopV); // векторное произведение
                                                                 // получили вектор, перпендикулярный плоскости, образуемой векторами startV, stopV
            rotationAxis.Normalize();

            float dot = Vector3.Dot(startV, stopV); // скалярное произведение
                                                    // т.к. векторы нормализованные, получаем чистый cos() угла между ними
            if (dot > 1.0f) dot = 1.0f; // подстраховка..
            if (dot < -1.0f) dot = -1.0f; // ..не более чем
            float rotationAngle = (float)Math.Acos(dot);

            Vector3 ypr = m_Camera.RotateModel(rotationAxis, rotationAngle);
            Teta = ypr.X;
            Phi = ypr.Y;
            Psi = ypr.Z;

            Render();
        }

        public void RotateModel(float psi, float phi, float teta)
        {
            m_Camera.RotateModel(psi, phi, teta);
            Render();
        }

        public void SetCameraDistance(int value)
        {
            m_Camera.CameraDistance = value;
        }

        public System.IO.Stream GetImage(ImageFileFormat format)
        {
            return m_D3D.GetImageFromSwapChain(format);
        }

        internal void Resize(Control control)
        {
            //m_D3D.OnWindowSizeChanged(control);
        }
    }
}
