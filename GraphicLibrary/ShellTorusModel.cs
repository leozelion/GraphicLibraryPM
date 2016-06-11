using SlimDX.Direct3D11;
using SlimDX;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Graphic
{
    //прямоугольная оболочка
    class ShellTorusModelNew : ShellBaseModel
    {
        float d; //смещение от оси вращения
        float radius; //радиус
        float startXAngle; //угловая стартовая координата по X
        float sizeXAngle; //угловой размер по оси X (длина, ширина) в радианах
        float sizeYAngle; //угловой размер по оси Y (длина, ширина) в радианах
        float sizeZ; //размер по оси Z (высота)
        float offset;
        double scaleCoeff;
        string prgfun;

        Dictionary<PointF, float> progibsTable = new Dictionary<PointF, float>(); //словарь прогибов
        float[] progibs; //прогибы
        List<float> setkaX = new List<float>(); //сетка по X
        List<float> setkaY = new List<float>(); //сетка по Y

       //конструктор
        public ShellTorusModelNew(Device Device, float D, float Radius, float StartXAngle, float SizeXAngle, float SizeYAngle, float SizeZ, List<Vector3> Progibs, float Offset, double scaleCoeff1, string prgfun1)
        {
            d = D;
            device = Device;
            radius = Radius;
            startXAngle = StartXAngle;
            sizeXAngle = SizeXAngle;
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

        //создание равномерной сетки
        private List<Vector3> BuildSetka()
        {
            List<Vector3> result = new List<Vector3>();

            int stacksX = 10 + (int)(sizeXAngle * radius / 1); //количество узлов сетки по X
            int stacksY = 10 + (int)(sizeYAngle * radius / 1); //количество узлов сетки по Y
            for (int stackX = 0; stackX < stacksX; stackX++)
                for (int stackY = 0; stackY < stacksY; stackY++)
                    result.Add(new Vector3(startXAngle + stackX * sizeXAngle / (stacksX - 1), -sizeYAngle / 2 + stackY * sizeYAngle / (stacksY - 1), 0));

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
            vertexesMiddle = HelpClassNew.BuildSphere(radius, setkaX, setkaY, d);
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
            HelpClassNew.CalculateNormals(vertexesEdgeFront, stacksZ, setkaX.Count, false);
            HelpClassNew.CalculateNormals(vertexesEdgeBack, stacksZ, setkaX.Count, true);
            HelpClassNew.CalculateNormals(vertexesEdgeLeft, stacksZ, setkaY.Count, true);
            HelpClassNew.CalculateNormals(vertexesEdgeRight, stacksZ, setkaY.Count, false);
            //построение прогибов
            vertexesTopDefected = HelpClassNew.ApplyProgib(vertexesMiddle, progibs, sizeZ / 2 + offset);
            HelpClassNew.CalculateNormals(vertexesTopDefected, setkaX.Count, setkaY.Count, true);
            vertexesBottomDefected = HelpClassNew.ApplyProgib(vertexesMiddle, progibs, -sizeZ / 2 + offset);
            HelpClassNew.CalculateNormals(vertexesBottomDefected, setkaX.Count, setkaY.Count, false);
            vertexesEdgeFrontDefected = HelpClassNew.BuildEdgeY(vertexesTopDefected, vertexesBottomDefected, 0, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeBackDefected = HelpClassNew.BuildEdgeY(vertexesTopDefected, vertexesBottomDefected, setkaY.Count - 1, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeLeftDefected = HelpClassNew.BuildEdgeX(vertexesTopDefected, vertexesBottomDefected, 0, setkaX.Count, setkaY.Count, stacksZ);
            vertexesEdgeRightDefected = HelpClassNew.BuildEdgeX(vertexesTopDefected, vertexesBottomDefected, setkaX.Count - 1, setkaX.Count, setkaY.Count, stacksZ);
            HelpClassNew.CalculateNormals(vertexesEdgeFrontDefected, stacksZ, setkaX.Count, false);
            HelpClassNew.CalculateNormals(vertexesEdgeBackDefected, stacksZ, setkaX.Count, true);
            HelpClassNew.CalculateNormals(vertexesEdgeLeftDefected, stacksZ, setkaY.Count, true);
            HelpClassNew.CalculateNormals(vertexesEdgeRightDefected, stacksZ, setkaY.Count, false);

            //выделение памяти под поверхности
            vbTop = createVertexBuffer(vertexesTop);
            vbTopDefected = createVertexBuffer(vertexesTopDefected);
            vbBottom = createVertexBuffer(vertexesBottom);
            vbBottomDefected = createVertexBuffer(vertexesBottomDefected);
            vbEdgeFront = createVertexBuffer(vertexesEdgeFront);
            vbEdgeFrontDefected = createVertexBuffer(vertexesEdgeFrontDefected);
            vbEdgeBack = createVertexBuffer(vertexesEdgeBack);
            vbEdgeBackDefected = createVertexBuffer(vertexesEdgeBackDefected);
            vbEdgeLeft = createVertexBuffer(vertexesEdgeLeft);
            vbEdgeLeftDefected = createVertexBuffer(vertexesEdgeLeftDefected);
            vbEdgeRight = createVertexBuffer(vertexesEdgeRight);
            vbEdgeRightDefected = createVertexBuffer(vertexesEdgeRightDefected);

            ibTopMiddleBottom = createIndexBuffer(indicesTopMiddleBottom);
            ibEdgeFrontBack = createIndexBuffer(indicesEdgeFrontBack);
            ibEdgeLeftRight = createIndexBuffer(indicesEdgeLeftRight);
        }
    }

    //торообразная оболочка с ребрами жесткости
    public class ShellTorusModellWithEdgesNew : ShellBaseModel
    {
        float d; //смещение от оси вращения
        float radius; //радиус
        float startXAngle; //угловая стартовая координата по X
        float sizeXAngle; //угловой размер по оси X (длина, ширина) в радианах
        float sizeYAngle; //угловой размер по оси Y (длина, ширина) в радианах
        float sizeZ; //размер по оси Z (высота)
        List<Vector3> progibs; //информация о прогибах

        public new float AnimationWeight
        {
            get
            {
                return animationWeight;
            }
            set
            {
                animationWeight = value;
                main.AnimationWeight = value;
                foreach (ShellTorusModelNew item in rebroX)
                    item.AnimationWeight = value;
                foreach (ShellTorusModelNew item in rebroY)
                    item.AnimationWeight = value;

            }
        }
    
        public new bool AutoAnimation
        {
            get
            {
                return autoAnimation;
            }
            set
            {
                autoAnimation = value;
                main.AutoAnimation = value;
                foreach (ShellTorusModelNew item in rebroX)
                    item.AutoAnimation = value;
                foreach (ShellTorusModelNew item in rebroY)
                    item.AutoAnimation = value;
            }
        }

        int nhx; //количество ребер по оси X
        float hwx; //размер ребра X по оси Z (высота)
        float sizeRebroXAngle; //размер ребра X
        int nhy; //количество ребер по оси Y
        float hwy; //размер ребра Y по оси Z (высота)
        float sizeRebroYAngle; //размер ребра Y

        List<ShellTorusModelNew> rebroX = new List<ShellTorusModelNew>();
        List<ShellTorusModelNew> rebroY = new List<ShellTorusModelNew>();
        ShellTorusModelNew main;

        public ShellTorusModellWithEdgesNew()
        {
        }

        public bool CreateModel(Device Device, float D, float Radius, float StartXAngle, float SizeXAngle, float SizeYAngle, float SizeZ, int NHX, int NHY, float HWX, float HWY, float SizeRebroXAngle, float SizeRebroYAngle, ProgibFunctionDelegate ProgibFunction, int dummy, float scaleCoeff, string prgfun)
        {
            try
            {
                #region создание сетки прогибов
                List<Vector3> Progibs = new List<Vector3>();
                List<float> xpoints = new List<float>();
                List<float> ypoints = new List<float>();
                float xstep = SizeXAngle / 180;
                float ystep = SizeYAngle / 180;
                for (float x = 0; x <= SizeXAngle; x += xstep)
                    xpoints.Add(x);
                for (float y = 0; y <= SizeYAngle; y += ystep)
                    ypoints.Add(y);

                float propusk = (SizeXAngle - NHX * SizeRebroXAngle) / (NHX + 1);
                for (int i = 0; i < NHX; i++)
                {
                    float x = (i + 1) * propusk + i * SizeRebroXAngle;
                    if (!xpoints.Contains(x))
                        xpoints.Add(x);
                    x = (i + 1) * propusk + i * SizeRebroXAngle + SizeRebroXAngle / 2;
                    if (!xpoints.Contains(x))
                        xpoints.Add(x);
                    x = (i + 1) * propusk + i * SizeRebroXAngle + SizeRebroXAngle;
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
                for (int i = 0; i < xpoints.Count; i++)
                    xpoints[i] += StartXAngle;

                AssemblyGenerator assgen = new AssemblyGenerator(prgfun);
                foreach (float x in xpoints)
                    foreach (float y in ypoints)
                    {
                        float rfv = (float)(assgen.Function(x, y) * scaleCoeff);
                        if (ProgibFunction != null)
                            Progibs.Add(new Vector3(x, y, rfv));
                        else
                            Progibs.Add(new Vector3(x, y, 0));
                    }
                #endregion

                return CreateModel(Device, D, Radius, StartXAngle, SizeXAngle, SizeYAngle, SizeZ, NHX, NHY, HWX, HWY, SizeRebroXAngle, SizeRebroYAngle, Progibs, scaleCoeff, prgfun);
            }
            catch
            {
                return false;
            }
        
        }
        public bool CreateModel(Device Device, float D, float Radius, float StartXAngle, float SizeXAngle, float SizeYAngle, float SizeZ, int NHX, int NHY, float HWX, float HWY, float SizeRebroXAngle, float SizeRebroYAngle, List<Vector3> Progibs, float scaleCoeff1, string prgfun1)
        {
            try
            {
                device = Device;
                d = D;
                radius = Radius;
                startXAngle = StartXAngle;
                sizeXAngle = SizeXAngle;
                sizeYAngle = SizeYAngle;
                sizeZ = SizeZ;
                nhx = NHX;
                nhy = NHY;
                hwx = HWX;
                hwy = HWY;
                sizeRebroXAngle = SizeRebroXAngle;
                sizeRebroYAngle = SizeRebroYAngle;
                progibs = Progibs;
                device = Device;
                
                if (Progibs == null)
                    return false;

                if (nhx > 0)
                {
                    List<Vector3> progibsRebro = new List<Vector3>();
                    float propusk = (sizeXAngle - nhx * sizeRebroXAngle) / (nhx + 1);
                    if (propusk < 0)
                        propusk = 0;
                    for (int i = 0; i < nhx; i++)
                    {
                        progibsRebro.Clear();
                        for (int k = 0; k < progibs.Count; k++)
                        {
                            if ((progibs[k].X >= startXAngle + (i + 1) * propusk + i * sizeRebroXAngle) && (progibs[k].X <= startXAngle + (i + 1) * propusk + i * sizeRebroXAngle + sizeRebroXAngle))
                            {
                                progibsRebro.Add(new Vector3(progibs[k].X, progibs[k].Y, progibs[k].Z));
                            }
                        }
                        rebroX.Add(new ShellTorusModelNew(device, d, radius, startXAngle, sizeXAngle, sizeYAngle, hwx, progibsRebro, -sizeZ / 2 - hwx / 2, scaleCoeff1, prgfun1));
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
                        rebroY.Add(new ShellTorusModelNew(device, d, radius, startXAngle, sizeXAngle, sizeYAngle, hwy, progibsRebro, -sizeZ / 2 - hwy / 2, scaleCoeff1, prgfun1));
                    }
                }

                main = new ShellTorusModelNew(device, d, radius, startXAngle, sizeXAngle, sizeYAngle, sizeZ, progibs, 0, scaleCoeff1, prgfun1);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public new void Draw(Device device, Matrix matrix)
        {
            //device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
            if (rebroX.Count > 0)
                foreach (ShellTorusModelNew item in rebroX)
                    item.Draw(device, Matrix.Identity);
            if (rebroY.Count > 0)
                foreach (ShellTorusModelNew item in rebroY)
                    item.Draw(device, Matrix.Identity);
            
            main.Draw(device, Matrix.Identity);
            //device.SetRenderState(RenderState.FillMode, FillMode.Solid);
        }
    }    
}

