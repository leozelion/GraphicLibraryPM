using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D11;


namespace GraphicLibrary
{
    class CameraClass
    {
        public Vector3 m_position;
        public Vector3 m_rotation;
        public Matrix m_viewMatrix;
        public Matrix m_worldMatrix;
        public int m_cameraDistance;
        public int CameraDistance { set { m_cameraDistance = value; } }

        private int m_minDist;
        private int m_maxDist;
        private Quaternion m_orientation;

        public CameraClass(int minDist, int maxDist)
        {
            m_orientation = Quaternion.Identity;
            m_cameraDistance = 50;
            //m_position = new Vector3(0.0f);
            //m_rotation = new Vector3(0.0f);
            m_minDist = minDist;
            m_maxDist = maxDist;
        }

        //////////////////////
        // must update !!!
        //////////////////////
        

        public Vector3 RotateModel(Vector3 rotationAxis, float angle)
        {
            Quaternion quaternion = Quaternion.RotationAxis(rotationAxis, angle);
            m_orientation = quaternion * m_orientation;

            return quaternion2Euler(m_orientation);
        }

        public void RotateModel(float yaw, float pitch, float roll)
        {
            Quaternion quaternion = Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
            m_orientation = quaternion * m_orientation;
        }

        public int Zoom(int zoom)
        {
            m_cameraDistance += zoom;
            if (m_cameraDistance <= m_minDist)
                m_cameraDistance = m_minDist;

            return m_cameraDistance;
        }

        public void Render()
        {
            Matrix world_matrix = Matrix.RotationQuaternion(m_orientation);
            Matrix view_matrix = Matrix.LookAtLH(new Vector3(0, 0, -m_cameraDistance), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        }

        private Vector3 quaternion2Euler(Quaternion q)
        {
            return new Vector3(
            (float)Math.Atan2(2 * (q.X * q.Y + q.W * q.Z),
                q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z),
            (float)Math.Asin(-2 * (q.Y * q.Z - q.W * q.X)),
            (float)Math.Atan2(2 * (q.X * q.Z + q.W * q.Y),
                q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z)
            );
        }


    }
}
