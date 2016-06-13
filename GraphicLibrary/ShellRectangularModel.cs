using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GraphicLibrary
{
    //прямоугольная оболочка
    class ShellRectangularModelNew : ShellBaseModel
    {
        float radiusX; //радиус по X
        float radiusY; //радиус по Y
        float sizeX; //размер по оси X (длина, ширина)
        float sizeY; //размер по оси Y (длина, ширина)
        float sizeZ; //размер по оси Z (высота)
        float offset;
        double scaleCoeff;
        string prgfun;
        

        Dictionary<PointF, float> progibsTable = new Dictionary<PointF, float>(); //словарь прогибов
        float[] progibs; //прогибы
        List<float> setkaX = new List<float>(); //сетка по X
        List<float> setkaY = new List<float>(); //сетка по Y

        //конструктор
        public ShellRectangularModelNew(Device Device, float RadiusX, float RadiusY, float SizeX, float SizeY, float SizeZ, List<Vector3> Progibs, float Offset, double scaleCoeff1, string prgfun1)
        {
            device = Device;
            radiusX = RadiusX;
            radiusY = RadiusY;
            sizeX = SizeX;
            sizeY = SizeY;
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

            int stacksX = 10 + (int)(sizeX / 1); //количество узлов сетки по X
            int stacksY = 10 + (int)(sizeY / 1); //количество узлов сетки по Y
            for (int stackX = 0; stackX < stacksX; stackX++)
                for (int stackY = 0; stackY < stacksY; stackY++)
                    result.Add(new Vector3(stackX * sizeX / (stacksX - 1), stackY * sizeY / (stacksY - 1), 0));

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
                    progibs[stackY * setkaX.Count + stackX] = progibsTable[new PointF(setkaX[stackX], setkaY[stackY])];
        }

        //построение модели
        private void BuildModel()
        {
            //построение поверхностей без учета прогибов
            vertexesMiddle = HelpClassNew.BuildFromTorus(radiusX, sizeX, radiusY, sizeY, setkaX, setkaY);
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

    //прямоугольная оболочка с ребрами жесткости
    public class ShellRectangularModelWithEdgesNew : ShellBaseModel
    {
        float radiusX; //радиус по X
        float radiusY; //радиус по Y
        float sizeX; //размер по оси X (длина, ширина)
        float sizeY; //размер по оси Y (длина, ширина)
        float sizeZ; //размер по оси Z (высота)
        List<Vector3> progibs; //информация о прогибах

        int nhx; //количество ребер по оси X
        float hwx; //размер ребра X по оси Z (высота)
        float sizeRebroX; //размер ребра X
        int nhy; //количество ребер по оси Y
        float hwy; //размер ребра Y по оси Z (высота)
        float sizeRebroY; //размер ребра Y

        List<ShellRectangularModelNew> rebroX = new List<ShellRectangularModelNew>();
        List<ShellRectangularModelNew> rebroY = new List<ShellRectangularModelNew>();
        ShellRectangularModelNew main;

        public ShellRectangularModelWithEdgesNew()
        {
            
        }

        /*public string convertMath(string prgfun)
        {
            
        }*/

        public bool CreateModel(Device Device, float RA, float RB, float A, float B, float H, int NHX, int NHY, float HWX, float HWY, float SizeRebroX, float SizeRebroY, ProgibFunctionDelegate ProgibFunction, int dummy, float scaleCoeff, string prgfun)
        {
            try
            {
                #region создание сетки прогибов
                List<Vector3> Progibs = new List<Vector3>();
                List<float> xpoints = new List<float>();
                List<float> ypoints = new List<float>();
                float xstep = A / 180;
                float ystep = B / 180;
                for (float x = 0; x <= A; x += xstep)
                    xpoints.Add(x);
                for (float y = 0; y <= B; y += ystep)
                    ypoints.Add(y);

                float propusk = (A - NHX * SizeRebroX) / (NHX + 1);
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
                propusk = (B - NHY * SizeRebroY) / (NHY + 1);
                for (int i = 0; i < NHY; i++)
                {
                    float y = (i + 1) * propusk + i * SizeRebroY;
                    if (!ypoints.Contains(y))
                        ypoints.Add(y);
                    y = (i + 1) * propusk + i * SizeRebroY + SizeRebroY / 2;
                    if (!ypoints.Contains(y))
                        ypoints.Add(y);
                    y = (i + 1) * propusk + i * SizeRebroY + SizeRebroY;
                    if (!ypoints.Contains(y))
                        ypoints.Add(y);
                }
                xpoints.Sort();
                ypoints.Sort();
                
                AssemblyGenerator assgen = new AssemblyGenerator(prgfun);
                foreach (float x in xpoints)
                {
                    foreach (float y in ypoints)
                    {
                        float rfv = (float)(assgen.Function(x, y)*scaleCoeff);
                        if (ProgibFunction != null)
                            Progibs.Add(new Vector3(x, y, rfv));
                        else
                            Progibs.Add(new Vector3(x, y, 0));
                    }
                    
                }
                //int ttyt;
                //ttyt = 123;
                #endregion

                return CreateModel(Device, RA, RB, A, B, H, NHX, NHY, HWX, HWY, SizeRebroX, SizeRebroY, Progibs, scaleCoeff, prgfun);
            }
            catch
            {
                return false;
            }
        }
        public bool CreateModel(Device Device, float RA, float RB, float A, float B, float H, int NHX, int NHY, float HWX, float HWY, float SizeRebroX, float SizeRebroY, List<Vector3> Progibs, float scaleCoeff1, string prgfun1)
        {
            try
            {
                radiusX = RA;
                radiusY = RB;
                sizeX = A;
                sizeY = B;
                sizeZ = H;
                nhx = NHX;
                nhy = NHY;
                hwx = HWX;
                hwy = HWY;
                sizeRebroX = SizeRebroX;
                sizeRebroY = SizeRebroY;
                device = Device;
                progibs = Progibs;
                
                if (Progibs == null)
                    return false;

                #region создание рёбер-оболочек по оси X
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
                        rebroX.Add(new ShellRectangularModelNew(device, radiusX, radiusY, sizeX, sizeY, hwx, progibsRebro, -sizeZ / 2 - hwx / 2,scaleCoeff1,prgfun1));
                    }
                }
                #endregion
                #region Создание рёбер-оболочек по оси Y
                if (nhy > 0)
                {
                    List<Vector3> progibsRebro = new List<Vector3>();

                    float propusk = (sizeY - nhy * sizeRebroY) / (nhy + 1);
                    if (propusk < 0)
                        propusk = 0;
                    for (int i = 0; i < nhy; i++)
                    {
                        progibsRebro.Clear();
                        for (int k = 0; k < progibs.Count; k++)
                        {
                            if ((progibs[k].Y >= (i + 1) * propusk + i * sizeRebroY) && (progibs[k].Y <= (i + 1) * propusk + i * sizeRebroY + sizeRebroY))
                            {
                                progibsRebro.Add(new Vector3(progibs[k].X, progibs[k].Y, progibs[k].Z));
                            }
                        }
                        rebroY.Add(new ShellRectangularModelNew(device, radiusX, radiusY, sizeX, sizeY, hwy, progibsRebro, -sizeZ / 2 - hwy / 2, scaleCoeff1, prgfun1));
                    }
                }
                #endregion

                // создание основной оболочки
                main = new ShellRectangularModelNew(device, radiusX, radiusY, sizeX, sizeY, sizeZ, progibs, 0, scaleCoeff1, prgfun1);

            }
            catch
            {
                return false;
            }
            return true;
        }

        public new void Draw(Device device, Matrix matrix)
        {
            if (rebroX.Count > 0)
                foreach (ShellRectangularModelNew item in rebroX)
                    item.Draw(device, Matrix.Identity);
            if (rebroY.Count > 0)
                foreach (ShellRectangularModelNew item in rebroY)
                    item.Draw(device, Matrix.Identity);
            main.Draw(
                device, 
                //matrix);
                Matrix.Identity);
        }
    }
}

