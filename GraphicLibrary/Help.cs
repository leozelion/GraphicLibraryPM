using SlimDX.Direct3D11;
using SlimDX;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Graphic
{
    public class h_VertexDeclarations
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct h_PositionNormalVertex
        {
            public Vector3 position;
            public Vector3 normal;

            public static int SizeInBytes
            {
                get { return Marshal.SizeOf(typeof(h_PositionNormalVertex)); }
            }

            //public static SlimDX.Direct3D9.VertexFormat Format
            //{
            //    get { return SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Normal; }
            //}

            public h_PositionNormalVertex(Vector3 Position, Vector3 Normal)
                : this()
            {
                position = Position;
                normal = Normal;
            }
        }

        public static readonly InputElement[] PositionNormalVertexElements = new InputElement[]
        {
			// base from stream 0
			new InputElement("POSITION", 0, SlimDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, SlimDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),

			// defected from stream 1
			new InputElement("POSITION", 1, SlimDX.DXGI.Format.R32G32B32_Float, 0, 1, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 1, SlimDX.DXGI.Format.R32G32B32_Float, 12, 1, InputClassification.PerVertexData, 0),
		};
    }

    public class HelpClassNew
    {
        static public h_VertexDeclarations.h_PositionNormalVertex[] BuildSphere(float radius, List<float> setkaXAngle, List<float> setkaYAngle, float offset)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[setkaXAngle.Count * setkaYAngle.Count];

            int countY = 0;
            foreach (float yAngle in setkaYAngle)
            {
                int countX = 0;
                foreach (float xAngle in setkaXAngle)
                {
                    float add_x = offset * (float)Math.Sin(yAngle);
                    float add_z = -offset * (float)Math.Cos(yAngle);

                    Vector3 normal = new Vector3((float)(radius * Math.Sin(xAngle) * Math.Sin(yAngle)), (float)(radius * Math.Cos(xAngle)), (float)(-radius * Math.Sin(xAngle) * Math.Cos(yAngle)));
                    Vector3 position = normal + new Vector3(add_x, 0, add_z);
                    normal.Normalize();

                    //position -= new Vector3(0, (float)(radius * Math.Cos(setkaXAngle[setkaXAngle.Count / 2])), (float)(-radius * Math.Sin(setkaXAngle[setkaXAngle.Count / 2])));

                    //result[count] = new VertexDeclarations.PositionNormalVertex(position, normal);
                    //count++;
                    
                    result[(setkaYAngle.Count - 1 - countY) * setkaXAngle.Count + countX] = new h_VertexDeclarations.h_PositionNormalVertex(position, normal);
                    
                    countX++;
                }
                countY++;
            }

            return result;
        }

        static public h_VertexDeclarations.h_PositionNormalVertex[] BuildConus(float conusAngle, float sizeX, float startX, List<float> setkaX, List<float> setkaYAngle)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[setkaX.Count * setkaYAngle.Count];

            //float sizeX = setkaX[setkaX.Count - 1] - setkaX[0];
            //float startX = setkaX[0];

            int countY = 0;
            foreach (float yAngle in setkaYAngle)
            {
                int countX = 0;
                foreach (float x in setkaX)
                {
                    float radiusY = (float)(x * Math.Sin(conusAngle));
                    Vector3 position = new Vector3((float)(x * Math.Cos(conusAngle)), (float)(radiusY * Math.Cos(yAngle)), (float)(radiusY * Math.Sin(-yAngle)));
                    Vector3 center = new Vector3((float)(position.X + radiusY * Math.Tan(conusAngle)), 0, 0);
                    Vector3 normal = position - center;
                    normal.Normalize();
                    position.X -= (startX + sizeX / 2);

                    result[(setkaYAngle.Count - 1 - countY) * setkaX.Count + countX] = new h_VertexDeclarations.h_PositionNormalVertex(position, normal);

                    countX++;
                }
                countY++;
            }

            return result;
        }
        
        static public h_VertexDeclarations.h_PositionNormalVertex[] BuildCylinder(float sizeX, float radiusY, float sizeYAngle, List<float> setkaX, List<float> setkaYAngle)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[setkaX.Count * setkaYAngle.Count];

            int countY = 0;
            foreach (float yAngle in setkaYAngle)
            {
                int countX = 0;
                foreach (float x in setkaX)
                {
                    Vector3 position = new Vector3(x, (float)(radiusY * Math.Cos(yAngle)), (float)(radiusY * Math.Sin(-yAngle)));
                    Vector3 normal = position;
                    normal.X = 0;
                    normal.Normalize();
                    position.Y -= radiusY;
                    position.X -= (sizeX / 2);

                    result[(setkaYAngle.Count - 1 - countY) * setkaX.Count + countX] = new h_VertexDeclarations.h_PositionNormalVertex(position, normal);
                    
                    countX++;
                }
                countY++;
            }

            return result;
        }

        static public h_VertexDeclarations.h_PositionNormalVertex[] BuildFromTorus(float radiusX, float sizeX, float radiusY, float sizeY, List<float> setkaX, List<float> setkaY)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[setkaX.Count * setkaY.Count];

            List<float> psi_angles = new List<float>();
            List<float> phi_angles = new List<float>();

            float r = radiusX;
            float r_length = sizeX;
            int r_stacks = setkaX.Count;
            float R = radiusY - radiusX;
            float R_length = sizeY;
            int R_stacks = setkaY.Count;

            float psi_start = (float)(R_length / (2 * (R + r)));
            for (int i = 0; i < setkaY.Count; i++)
            {
                float psi = setkaY[i] / (R + r) - psi_start;
                psi_angles.Add(psi);
            }
            float phi_start = (float)(r_length / (2 * r));
            for (int i = 0; i < setkaX.Count; i++)
            {
                float phi = phi_start - setkaX[i] / r;
                phi_angles.Add(phi);
            }

            if (radiusX >= radiusY)
            {
                r = radiusY;
                r_length = sizeY;
                r_stacks = setkaY.Count;
                R = radiusX - radiusY;
                R_length = sizeX;
                R_stacks = setkaX.Count;

                psi_angles.Clear();
                psi_start = (float)(R_length / (2 * (R + r)));
                for (int i = 0; i < setkaX.Count; i++)
                {
                    float psi = psi_start - setkaX[i] / (R + r);
                    psi_angles.Add(psi);
                }
                phi_start = (float)(r_length / (2 * r));
                phi_angles.Clear();
                for (int i = 0; i < setkaY.Count; i++)
                {
                    float phi = phi_start - setkaY[i] / r;
                    phi_angles.Add(phi);
                }
            }

            for (int slice = 0; slice < psi_angles.Count; slice++)
            {
                float psi = psi_angles[slice];

                float x = (float)((R + r) * Math.Cos(psi));
                float y = 0;
                float z = (float)((R + r) * Math.Sin(psi));
                Vector3 r_center = new Vector3(x, y, z);
                r_center.Normalize();
                r_center *= R;

                for (int stack = 0; stack < phi_angles.Count; stack++)
                {
                    float phi = phi_angles[stack];

                    x = (float)((R + r * Math.Cos(phi)) * Math.Cos(psi));
                    y = (float)(r * Math.Sin(phi));
                    z = (float)((R + r * Math.Cos(phi)) * Math.Sin(psi));

                    Vector3 point = new Vector3(x, y, z);

                    Vector3 normal = point - r_center;
                    normal.Normalize();

                    if (radiusX < radiusY)
                        result[slice * r_stacks + stack] = new h_VertexDeclarations.h_PositionNormalVertex(point, normal);
                    else
                        result[stack * R_stacks + slice] = new h_VertexDeclarations.h_PositionNormalVertex(point, normal);
                }
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i].position.X -= (R + r);

                float temp = result[i].position.X;
                result[i].position.X = -result[i].position.Y;
                result[i].position.Y = temp;
                temp = result[i].normal.X;
                result[i].normal.X = -result[i].normal.Y;
                result[i].normal.Y = temp;

                if (radiusX >= radiusY)
                {
                    temp = result[i].position.X;
                    result[i].position.X = -result[i].position.Z;
                    result[i].position.Z = temp;
                    temp = result[i].normal.X;
                    result[i].normal.X = -result[i].normal.Z;
                    result[i].normal.Z = temp;
                }
            }

            return result;
        }

        static public h_VertexDeclarations.h_PositionNormalVertex[] ApplyProgib(h_VertexDeclarations.h_PositionNormalVertex[] surface, float[] progibs, float add)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[surface.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = new h_VertexDeclarations.h_PositionNormalVertex(surface[i].position + surface[i].normal * (progibs[i] + add), surface[i].normal);

            return result;
        }

        static public h_VertexDeclarations.h_PositionNormalVertex[] MoveAlongNormal(h_VertexDeclarations.h_PositionNormalVertex[] surface, float move, bool invert_normal)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[surface.Length];

            for (int i = 0; i < result.Length; i++)
            {
                if (invert_normal)
                    result[i] = new h_VertexDeclarations.h_PositionNormalVertex(surface[i].position + surface[i].normal * move, -surface[i].normal);
                else
                    result[i] = new h_VertexDeclarations.h_PositionNormalVertex(surface[i].position + surface[i].normal * move, surface[i].normal);
            }

            return result;
        }

        static public h_VertexDeclarations.h_PositionNormalVertex[] BuildEdgeX(h_VertexDeclarations.h_PositionNormalVertex[] surfaceTop, h_VertexDeclarations.h_PositionNormalVertex[] surfaceBottom, int stackX, int stacksX, int stacksY, int stacksZ)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[stacksY * stacksZ];

            int index = 0;
            for (int stackY = 0; stackY < stacksY; stackY++)
            {
                Vector3 direction = surfaceTop[stackY * stacksX + stackX].position - surfaceBottom[stackY * stacksX + stackX].position;
                float H = direction.Length();
                direction.Normalize();
                for (int stackZ = 0; stackZ < stacksZ; stackZ++)
                {
                    result[index] = new h_VertexDeclarations.h_PositionNormalVertex(surfaceBottom[stackY * stacksX + stackX].position + direction * H * stackZ / (stacksZ - 1), Vector3.Zero);
                    index++;
                }
            }

            return result;
        }

        static public h_VertexDeclarations.h_PositionNormalVertex[] BuildEdgeY(h_VertexDeclarations.h_PositionNormalVertex[] surfaceTop, h_VertexDeclarations.h_PositionNormalVertex[] surfaceBottom, int stackY, int stacksX, int stacksY, int stacksZ)
        {
            h_VertexDeclarations.h_PositionNormalVertex[] result = new h_VertexDeclarations.h_PositionNormalVertex[stacksX * stacksZ];

            int index = 0;
            for (int stackX = 0; stackX < stacksX; stackX++)
            {
                Vector3 direction = surfaceTop[stackY * stacksX + stackX].position - surfaceBottom[stackY * stacksX + stackX].position;
                float H = direction.Length();
                direction.Normalize();
                for (int stackZ = 0; stackZ < stacksZ; stackZ++)
                {
                    result[index] = new h_VertexDeclarations.h_PositionNormalVertex(surfaceBottom[stackY * stacksX + stackX].position + direction * H * stackZ / (stacksZ - 1), Vector3.Zero);
                    index++;
                }
            }

            return result;
        }

        static public void CalculateNormals(h_VertexDeclarations.h_PositionNormalVertex[] surface, int pitch, int height, bool invert_normal)
        {
            //расчёт нормалей: верх
            for (int slice = 1; slice < height - 1; slice++)
            {
                for (int stack = 1; stack < pitch - 1; stack++)
                {
                    Vector3 v = surface[slice * pitch + stack].position;

                    Vector3 V1 = surface[(slice + 1) * pitch + stack + 0].position;
                    Vector3 V2 = surface[(slice + 1) * pitch + stack + 1].position;
                    Vector3 V3 = surface[(slice + 0) * pitch + stack + 1].position;
                    Vector3 V4 = surface[(slice - 1) * pitch + stack + 1].position;
                    Vector3 V5 = surface[(slice - 1) * pitch + stack + 0].position;
                    Vector3 V6 = surface[(slice - 1) * pitch + stack - 1].position;
                    Vector3 V7 = surface[(slice + 0) * pitch + stack - 1].position;
                    Vector3 V8 = surface[(slice + 1) * pitch + stack - 1].position;

                    Vector3 n1 = Vector3.Cross(v - V1, v - V2);
                    Vector3 n2 = Vector3.Cross(v - V2, v - V3);
                    Vector3 n3 = Vector3.Cross(v - V3, v - V4);
                    Vector3 n4 = Vector3.Cross(v - V4, v - V1);
                    Vector3 n5 = Vector3.Cross(v - V5, v - V6);
                    Vector3 n6 = Vector3.Cross(v - V6, v - V7);
                    Vector3 n7 = Vector3.Cross(v - V7, v - V8);
                    Vector3 n8 = Vector3.Cross(v - V8, v - V1);

                    Vector3 n = (n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8) / 8;

                    n.Normalize();
                    if (invert_normal)
                        n = -n;

                    surface[slice * pitch + stack].normal = n;
                    if (slice == 1)
                    {
                        surface[(slice - 1) * pitch + stack].normal = n;
                        if (stack == 1)
                            surface[(slice - 1) * pitch + stack - 1].normal = n;
                        if (stack == pitch - 2)
                            surface[(slice - 1) * pitch + stack + 1].normal = n;
                    }
                    if (slice == height - 2)
                    {
                        surface[(slice + 1) * pitch + stack].normal = n;
                        if (stack == 1)
                            surface[(slice + 1) * pitch + stack - 1].normal = n;
                        if (stack == pitch - 2)
                            surface[(slice + 1) * pitch + stack + 1].normal = n;
                    }
                    if (stack == 1)
                        surface[slice * pitch + stack - 1].normal = n;
                    if (stack == pitch - 2)
                        surface[slice * pitch + stack + 1].normal = n;
                }
            }

        }

        static public short[] CalculateIndexes(int pitch, int height)
        {
            short[] result = new short[(height - 1) * (pitch - 1) * 6];
            int index = 0;
            for (int stackX = 0; stackX < (pitch - 1); stackX++)
            {
                for (int stackY = 0; stackY < (height - 1); stackY++)
                {
                    result[index++] = (short)((stackY + 0) * pitch + stackX);
                    result[index++] = (short)((stackY + 1) * pitch + stackX);
                    result[index++] = (short)((stackY + 0) * pitch + (stackX + 1));

                    result[index++] = (short)((stackY + 0) * pitch + (stackX + 1));
                    result[index++] = (short)((stackY + 1) * pitch + stackX);
                    result[index++] = (short)((stackY + 1) * pitch + (stackX + 1));
                }
            }

            //расчёт индексов: передняя, задняя
            /*indicesEdgeXFrontBack = new int[(heights - 1) * (stacks - 1) * 6];
            indexEdgeXFrontBack = 0;
            for (int stack = 0; stack < (stacks - 1); stack++)
            {
                for (int height = 0; height < (heights - 1); height++)
                {
                    indicesEdgeXFrontBack[indexEdgeXFrontBack++] = (stack + 0) * heights + height;
                    indicesEdgeXFrontBack[indexEdgeXFrontBack++] = (stack + 1) * heights + height;
                    indicesEdgeXFrontBack[indexEdgeXFrontBack++] = (stack + 0) * heights + (height + 1);

                    indicesEdgeXFrontBack[indexEdgeXFrontBack++] = (stack + 0) * heights + (height + 1);
                    indicesEdgeXFrontBack[indexEdgeXFrontBack++] = (stack + 1) * heights + height;
                    indicesEdgeXFrontBack[indexEdgeXFrontBack++] = (stack + 1) * heights + (height + 1);
                }
            }*/


            return result;
        }
    }
}

