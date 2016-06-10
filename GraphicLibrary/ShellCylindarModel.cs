using SlimDX.Direct3D9;
using SlimDX;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Graphic
{
    //прямоугольная оболочка
    class ShellCylindarModelNew : ShellBaseModel
    {
        float radiusY; //радиус по Y
        float sizeX; //размер по оси X (длина, ширина)
        float sizeYAngle; //угловой размер по оси Y (длина, ширина) в радианах
        float sizeZ; //размер по оси Z (высота)
        float offset;
        double scaleCoeff;
        string prgfun;

        Dictionary<PointF, float> progibsTable = new Dictionary<PointF, float>(); //словарь прогибов
        float[] progibs; //прогибы
        List<float> setkaX = new List<float>(); //сетка по X
        List<float> setkaY = new List<float>(); //сетка по Y

        h_VertexDeclarations.h_PositionNormalVertex[] vertexesMiddle;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesTop;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesBottom;
        short[] indicesTopMiddleBottom;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeFront;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeBack;
        short[] indicesEdgeFrontBack;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeLeft;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeRight;
        short[] indicesEdgeLeftRight;

        h_VertexDeclarations.h_PositionNormalVertex[] vertexesTopDefected;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesBottomDefected;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeFrontDefected;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeBackDefected;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeLeftDefected;
        h_VertexDeclarations.h_PositionNormalVertex[] vertexesEdgeRightDefected;

        Device device;
        VertexBuffer vbTop;
        VertexBuffer vbTopDefected;
        VertexBuffer vbBottom;
        VertexBuffer vbBottomDefected;
        VertexBuffer vbEdgeFront;
        VertexBuffer vbEdgeFrontDefected;
        VertexBuffer vbEdgeBack;
        VertexBuffer vbEdgeBackDefected;
        VertexBuffer vbEdgeLeft;
        VertexBuffer vbEdgeLeftDefected;
        VertexBuffer vbEdgeRight;
        VertexBuffer vbEdgeRightDefected;
        IndexBuffer ibTopMiddleBottom;
        IndexBuffer ibEdgeFrontBack;
        IndexBuffer ibEdgeLeftRight;

        //коэф-т анимации
        float direction = 1;
        float animationWeight = 0;
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
        bool autoAnimation = false;
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

        public override void SetAnimationWeight(float value)
        {
            AnimationWeight = value;
        }
        public override void SetAutoAnimation(bool value)
        {
            AutoAnimation = value;
        }

        int ticks;

       //конструктор
        public ShellCylindarModelNew(Device Device, float RadiusY, float SizeX, float SizeYAngle, float SizeZ, List<Vector3> Progibs, float Offset, double scaleCoeff1, string prgfun1)
        {
            device = Device;
            radiusY = RadiusY;
            sizeX = SizeX;
            sizeYAngle = SizeYAngle;
            sizeZ = SizeZ;
            offset = Offset;
            scaleCoeff = scaleCoeff1;
            prgfun = prgfun1;

            if (Progibs == null)
                Progibs = BuildSetka();

            BuildProgibStructure(Progibs);

            BuildModel();
        }

        //отрисовка
        public override void Draw(Device device, Matrix matrix)
        {
            if (autoAnimation)
            {
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

            device.VertexDeclaration = new VertexDeclaration(device, h_VertexDeclarations.PositionNormalVertexElements);
            device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("coef"), animationWeight);
            //верхняя поверхность
            device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("Color"), new float[4] { 0.9f, 0.9f, 0.9f, 1.0f });
            device.SetStreamSource(0, vbTop, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.SetStreamSource(1, vbTopDefected, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.Indices = ibTopMiddleBottom;
            device.BeginScene();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexesTop.Length, 0, indicesTopMiddleBottom.Length / 3);
            device.EndScene();
            //нижняя поверхность
            device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("Color"), new float[4] { 0.7f, 0.7f, 0.7f, 1.0f });
            device.SetStreamSource(0, vbBottom, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.SetStreamSource(1, vbBottomDefected, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.Indices = ibTopMiddleBottom;
            device.BeginScene();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexesBottom.Length, 0, indicesTopMiddleBottom.Length / 3);
            device.EndScene();
            //боковые грани
            device.VertexShader.Function.ConstantTable.SetValue(device, new EffectHandle("Color"), new float[4] { 0.3f, 0.3f, 0.3f, 1 });
            device.SetStreamSource(0, vbEdgeFront, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.SetStreamSource(1, vbEdgeFrontDefected, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.Indices = ibEdgeFrontBack;
            device.BeginScene();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexesEdgeFront.Length, 0, indicesEdgeFrontBack.Length / 3);
            device.EndScene();
            device.SetStreamSource(0, vbEdgeBack, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.SetStreamSource(1, vbEdgeBackDefected, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.Indices = ibEdgeFrontBack;
            device.BeginScene();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexesEdgeBack.Length, 0, indicesEdgeFrontBack.Length / 3);
            device.EndScene();
            device.SetStreamSource(0, vbEdgeLeft, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.SetStreamSource(1, vbEdgeLeftDefected, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.Indices = ibEdgeLeftRight;
            device.BeginScene();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexesEdgeLeft.Length, 0, indicesEdgeLeftRight.Length / 3);
            device.EndScene();
            device.SetStreamSource(0, vbEdgeRight, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.SetStreamSource(1, vbEdgeRightDefected, 0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes);
            device.Indices = ibEdgeLeftRight;
            device.BeginScene();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexesEdgeRight.Length, 0, indicesEdgeLeftRight.Length / 3);
            device.EndScene();
        }

        //создание равномерной сетки
        private List<Vector3> BuildSetka()
        {
            List<Vector3> result = new List<Vector3>();

            int stacksX = 10 + (int)(sizeX / 1); //количество узлов сетки по X
            int stacksY = 10 + (int)(sizeYAngle * radiusY / 1); //количество узлов сетки по Y
            for (int stackX = 0; stackX < stacksX; stackX++)
                for (int stackY = 0; stackY < stacksY; stackY++)
                    result.Add(new Vector3(stackX * sizeX / (stacksX - 1), - sizeYAngle / 2 + stackY * sizeYAngle / (stacksY - 1), 0));

            return result;
        }

        //создание сетки
        private void BuildProgibStructure(List<Vector3> Progibs)
        {
            //создание словаря прогибов и сетки
            for (int i = 0; i < Progibs.Count; i++)
            {
                if (!progibsTable.ContainsKey(new PointF(Progibs[i].X, Progibs[i].Y)))
                {
                    progibsTable.Add(new PointF(Progibs[i].X, Progibs[i].Y), Progibs[i].Z);
                    if (!setkaX.Contains(Progibs[i].X))
                        setkaX.Add(Progibs[i].X);
                    if (!setkaY.Contains(Progibs[i].Y))
                        setkaY.Add(Progibs[i].Y);
                }
            }
            setkaX.Sort();
            setkaY.Sort();

            //сохранение прогибов
            progibs = new float[setkaX.Count * setkaY.Count];
            for (int stackX = 0; stackX < setkaX.Count; stackX++)
                for (int stackY = 0; stackY < setkaY.Count; stackY++)
                    progibs[(setkaY.Count - 1 - stackY) * setkaX.Count + stackX] = progibsTable[new PointF(setkaX[stackX], setkaY[stackY])];
        }

        //построение модели
        private void BuildModel()
        {
            //построение поверхностей без учета прогибов
            vertexesMiddle = HelpClassNew.BuildCylinder(sizeX, radiusY, sizeYAngle, setkaX, setkaY);
            vertexesTop = HelpClassNew.MoveAlongNormal(vertexesMiddle, sizeZ / 2 + offset, false);
            vertexesBottom = HelpClassNew.MoveAlongNormal(vertexesMiddle, -sizeZ / 2 + offset, true);
            //расчёт индексов
            indicesTopMiddleBottom = HelpClassNew.CalculateIndexes(setkaX.Count, setkaY.Count);
            //построение боковых граней
            int stacksZ = 3;// 10 + (int)(sizeZ / 10); //количество узлов сетки по Z
            vertexesEdgeFront = HelpClassNew.BuildEdgeY(vertexesTop, vertexesBottom, 0, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeBack = HelpClassNew.BuildEdgeY(vertexesTop, vertexesBottom, setkaY.Count - 1, setkaX.Count, setkaY.Count, stacksZ);
            indicesEdgeFrontBack = HelpClassNew.CalculateIndexes(stacksZ, setkaX.Count);
            vertexesEdgeLeft = HelpClassNew.BuildEdgeX(vertexesTop, vertexesBottom, 0, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeRight = HelpClassNew.BuildEdgeX(vertexesTop, vertexesBottom, setkaX.Count - 1, setkaX.Count, setkaY.Count, stacksZ);
            indicesEdgeLeftRight = HelpClassNew.CalculateIndexes(stacksZ, setkaY.Count);
            HelpClassNew.CalculateNormals(vertexesEdgeFront, stacksZ, setkaX.Count, true);
            HelpClassNew.CalculateNormals(vertexesEdgeBack, stacksZ, setkaX.Count, false);
            HelpClassNew.CalculateNormals(vertexesEdgeLeft, stacksZ, setkaY.Count, false);
            HelpClassNew.CalculateNormals(vertexesEdgeRight, stacksZ, setkaY.Count, true);
            //построение прогибов
            vertexesTopDefected = HelpClassNew.ApplyProgib(vertexesMiddle, progibs, sizeZ / 2 + offset);
            HelpClassNew.CalculateNormals(vertexesTopDefected, setkaX.Count, setkaY.Count, false);
            vertexesBottomDefected = HelpClassNew.ApplyProgib(vertexesMiddle, progibs, -sizeZ / 2 + offset);
            HelpClassNew.CalculateNormals(vertexesBottomDefected, setkaX.Count, setkaY.Count, true);
            vertexesEdgeFrontDefected = HelpClassNew.BuildEdgeY(vertexesTopDefected, vertexesBottomDefected, 0, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeBackDefected = HelpClassNew.BuildEdgeY(vertexesTopDefected, vertexesBottomDefected, setkaY.Count - 1, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeLeftDefected = HelpClassNew.BuildEdgeX(vertexesTopDefected, vertexesBottomDefected, 0, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeRightDefected = HelpClassNew.BuildEdgeX(vertexesTopDefected, vertexesBottomDefected, setkaX.Count - 1, setkaX.Count, setkaY.Count, stacksZ);
            HelpClassNew.CalculateNormals(vertexesEdgeFrontDefected, stacksZ, setkaX.Count, true);
            HelpClassNew.CalculateNormals(vertexesEdgeBackDefected, stacksZ, setkaX.Count, false);
            HelpClassNew.CalculateNormals(vertexesEdgeLeftDefected, stacksZ, setkaY.Count, false);
            HelpClassNew.CalculateNormals(vertexesEdgeRightDefected, stacksZ, setkaY.Count, true);

            //выделение памяти под поверхности
            vbTop = CreateVertexBuffer(vertexesTop);
            vbTopDefected = CreateVertexBuffer(vertexesTopDefected);
            vbBottom = CreateVertexBuffer(vertexesBottom);
            vbBottomDefected = CreateVertexBuffer(vertexesBottomDefected);
            vbEdgeFront = CreateVertexBuffer(vertexesEdgeFront);
            vbEdgeFrontDefected = CreateVertexBuffer(vertexesEdgeFrontDefected);
            vbEdgeBack = CreateVertexBuffer(vertexesEdgeBack);
            vbEdgeBackDefected = CreateVertexBuffer(vertexesEdgeBackDefected);
            vbEdgeLeft = CreateVertexBuffer(vertexesEdgeLeft);
            vbEdgeLeftDefected = CreateVertexBuffer(vertexesEdgeLeftDefected);
            vbEdgeRight = CreateVertexBuffer(vertexesEdgeRight);
            vbEdgeRightDefected = CreateVertexBuffer(vertexesEdgeRightDefected);

            ibTopMiddleBottom = new IndexBuffer(device, sizeof(ushort) * indicesTopMiddleBottom.Length, Usage.None, Pool.Default, true);
            DataStream dr = ibTopMiddleBottom.Lock(0, sizeof(ushort) * indicesTopMiddleBottom.Length, LockFlags.None);
            dr.WriteRange(indicesTopMiddleBottom);
            ibTopMiddleBottom.Unlock();

            ibEdgeFrontBack = new IndexBuffer(device, sizeof(ushort) * indicesEdgeFrontBack.Length, Usage.None, Pool.Default, true);
            dr = ibEdgeFrontBack.Lock(0, sizeof(ushort) * indicesEdgeFrontBack.Length, LockFlags.None);
            dr.WriteRange(indicesEdgeFrontBack);
            ibEdgeFrontBack.Unlock();

            ibEdgeLeftRight = new IndexBuffer(device, sizeof(ushort) * indicesEdgeLeftRight.Length, Usage.None, Pool.Default, true);
            dr = ibEdgeLeftRight.Lock(0, sizeof(ushort) * indicesEdgeLeftRight.Length, LockFlags.None);
            dr.WriteRange(indicesEdgeLeftRight);
            ibEdgeLeftRight.Unlock();
        }

        private VertexBuffer CreateVertexBuffer(h_VertexDeclarations.h_PositionNormalVertex[] surface)
        {
            VertexBuffer result = new VertexBuffer(device, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes * surface.Length, Usage.None, VertexFormat.None, Pool.Default);
            DataStream dr = result.Lock(0, h_VertexDeclarations.h_PositionNormalVertex.SizeInBytes * surface.Length, LockFlags.None);
            dr.WriteRange(surface);
            result.Unlock();
            return result;
        }
    }

    //прямоугольная оболочка с ребрами жесткости
    public class ShellCylindarModelWithEdgesNew : ShellBaseModel
    {
        float radiusY; //радиус по Y
        float sizeX; //размер по оси X (длина, ширина)
        float sizeYAngle; //угловой размер по оси Y (длина, ширина) в радианах
        float sizeZ; //размер по оси Z (высота)
        List<Vector3> progibs; //информация о прогибах
        Device device;

        float animationWeight = 0;
        public float AnimationWeight
        {
            get
            {
                return animationWeight;
            }
            set
            {
                animationWeight = value;
                main.AnimationWeight = value;
                foreach (ShellCylindarModelNew item in rebroX)
                    item.AnimationWeight = value;
                foreach (ShellCylindarModelNew item in rebroY)
                    item.AnimationWeight = value;

            }
        }
        bool autoAnimation = false;
        public bool AutoAnimation
        {
            get
            {
                return autoAnimation;
            }
            set
            {
                autoAnimation = value;
                main.AutoAnimation = value;
                foreach (ShellCylindarModelNew item in rebroX)
                    item.AutoAnimation = value;
                foreach (ShellCylindarModelNew item in rebroY)
                    item.AutoAnimation = value;
            }
        }

        public override void SetAnimationWeight(float value)
        {
            AnimationWeight = value;
        }
        public override void SetAutoAnimation(bool value)
        {
            AutoAnimation = value;
        }
        
        int nhx; //количество ребер по оси X
        float hwx; //размер ребра X по оси Z (высота)
        float sizeRebroX; //размер ребра X
        int nhy; //количество ребер по оси Y
        float hwy; //размер ребра Y по оси Z (высота)
        float sizeRebroYAngle; //размер ребра Y

        List<ShellCylindarModelNew> rebroX = new List<ShellCylindarModelNew>();
        List<ShellCylindarModelNew> rebroY = new List<ShellCylindarModelNew>();
        ShellCylindarModelNew main;

        public ShellCylindarModelWithEdgesNew()
        {
        }
        public bool CreateModel(Device Device, float RadiusY, float SizeX, float SizeYAngle, float SizeZ, int NHX, int NHY, float HWX, float HWY, float SizeRebroX, float SizeRebroYAngle, ProgibFunctionDelegate ProgibFunction, int dummy, float scaleCoeff, string prgfun)
        {
            try
            {
                #region создание сетки прогибов
                List<Vector3> Progibs = new List<Vector3>();
                List<float> xpoints = new List<float>();
                List<float> ypoints = new List<float>();
                float xstep = SizeX / 180;
                float ystep = SizeYAngle / 180;
                for (float x = 0; x <= SizeX; x += xstep)
                    xpoints.Add(x);
                for (float y = 0; y <= SizeYAngle; y += ystep)
                    ypoints.Add(y);

                float propusk = (SizeX - NHX * SizeRebroX) / (NHX + 1);
                for (int i = 0; i < NHX; i++)
                {
                    float x = (i + 1) * propusk + i * SizeRebroX;
                    if (!xpoints.Contains(x))
                        xpoints.Add(x);
                    x = (i + 1) * propusk + i * SizeRebroX + SizeRebroX / 2;
                    if (!xpoints.Contains(x))
                        xpoints.Add(x);
                    x = (i + 1) * propusk + i * SizeRebroX + SizeRebroX;
                    if (!xpoints.Contains(x))
                        xpoints.Add(x);
                }
                propusk = (SizeYAngle - NHY * SizeRebroYAngle) / (NHY + 1);
                for (int i = 0; i < NHY; i++)
                {
                    float y = (i + 1) * propusk + i * SizeRebroYAngle;
                    if (!ypoints.Contains(y))
                        ypoints.Add(y);
                    y = (i + 1) * propusk + i * SizeRebroYAngle + SizeRebroYAngle / 2;
                    if (!ypoints.Contains(y))
                        ypoints.Add(y);
                    y = (i + 1) * propusk + i * SizeRebroYAngle + SizeRebroYAngle;
                    if (!ypoints.Contains(y))
                        ypoints.Add(y);
                }
                xpoints.Sort();
                ypoints.Sort();

                for (int i = 0; i < ypoints.Count; i++)
                    ypoints[i] -= (SizeYAngle / 2);

                AssemblyGenerator assgen = new AssemblyGenerator(prgfun);
                foreach (float x in xpoints)
                    foreach (float y in ypoints)
                    {
                        float rfv = (float)(assgen.Function(x, y + (SizeYAngle / 2)) * scaleCoeff);
                        if (ProgibFunction != null)
                            Progibs.Add(new Vector3(x, y, rfv));
                        else
                            Progibs.Add(new Vector3(x, y, 0));
                    }
                #endregion

                return CreateModel(Device, RadiusY, SizeX, SizeYAngle, SizeZ, NHX, NHY, HWX, HWY, SizeRebroX, SizeRebroYAngle, Progibs, scaleCoeff, prgfun);
            }
            catch
            {
                return false;
            }
        }
        public bool CreateModel(Device Device, float RadiusY, float SizeX, float SizeYAngle, float SizeZ, int NHX, int NHY, float HWX, float HWY, float SizeRebroX, float SizeRebroYAngle, List<Vector3> Progibs, float scaleCoeff1, string prgfun1)
        {
            try
            {
                radiusY = RadiusY;
                sizeX = SizeX;
                sizeYAngle = SizeYAngle;
                sizeZ = SizeZ;
                nhx = NHX;
                nhy = NHY;
                hwx = HWX;
                hwy = HWY;
                sizeRebroX = SizeRebroX;
                sizeRebroYAngle = SizeRebroYAngle;
                progibs = Progibs;
                device = Device;

                if (Progibs == null)
                    return false;

                if (nhx > 0)
                {
                    List<Vector3> progibsRebro = new List<Vector3>();
                    float propusk = (sizeX - nhx * sizeRebroX) / (nhx + 1);
                    if (propusk < 0)
                        propusk = 0;
                    for (int i = 0; i < nhx; i++)
                    {
                        progibsRebro.Clear();
                        for (int k = 0; k < progibs.Count; k++)
                        {
                            if ((progibs[k].X >= (i + 1) * propusk + i * sizeRebroX) && (progibs[k].X <= (i + 1) * propusk + i * sizeRebroX + sizeRebroX))
                            {
                                progibsRebro.Add(new Vector3(progibs[k].X, progibs[k].Y, progibs[k].Z));
                            }
                        }
                        rebroX.Add(new ShellCylindarModelNew(device, radiusY, sizeX, sizeYAngle, hwx, progibsRebro, -sizeZ / 2 - hwx / 2, scaleCoeff1, prgfun1));
                    }
                }
                if (nhy > 0)
                {
                    List<Vector3> progibsRebro = new List<Vector3>();

                    float propusk = (sizeYAngle - nhy * sizeRebroYAngle) / (nhy + 1);
                    if (propusk < 0)
                        propusk = 0;
                    for (int i = 0; i < nhy; i++)
                    {
                        progibsRebro.Clear();
                        for (int k = 0; k < progibs.Count; k++)
                        {
                            if ((progibs[k].Y >= -SizeYAngle / 2 + (i + 1) * propusk + i * sizeRebroYAngle) && (progibs[k].Y <= -SizeYAngle / 2 + (i + 1) * propusk + i * sizeRebroYAngle + sizeRebroYAngle))
                            {
                                progibsRebro.Add(new Vector3(progibs[k].X, progibs[k].Y, progibs[k].Z));
                            }
                        }
                        rebroY.Add(new ShellCylindarModelNew(device, radiusY, sizeX, sizeYAngle, hwy, progibsRebro, -sizeZ / 2 - hwy / 2, scaleCoeff1, prgfun1));
                    }
                }

                main = new ShellCylindarModelNew(device, radiusY, sizeX, sizeYAngle, sizeZ, progibs, 0, scaleCoeff1, prgfun1);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override void Draw(Device device, Matrix matrix)
        {
            //device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
            if (rebroX.Count > 0)
                foreach (ShellCylindarModelNew item in rebroX)
                    item.Draw(device, Matrix.Identity);
            if (rebroY.Count > 0)
                foreach (ShellCylindarModelNew item in rebroY)
                    item.Draw(device, Matrix.Identity);
            
            main.Draw(device, Matrix.Identity);
            //device.SetRenderState(RenderState.FillMode, FillMode.Solid);
        }
    }
}

