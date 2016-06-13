using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using SlimDX;
using System.IO;

namespace GraphicLibrary
{
    public partial class Form1 : Form
    {
        //графический модуль
        //GraphicLibrary moduleGraphicLibrary;
        GraphicClass m_Graphic;

        //оболочка
        //ShellBaseModel shell;

        //положение курсора мыши
        Point mousePosition;

        //поток отрисовки
        Thread threadDrawing;

        //флаг выхода из приложения
        bool doExit = false;

        //коэф - т масштабирования прогиба (чтобы прогиб был заметен на глаз)
        float scaleCoef = 1.0f;

        //перевод из радиан в градусы
        const float RADIANS = (float)Math.PI / 180;

        public AssemblyGenerator.FunctionDelegate progibSeFunc;

        //конструктор
        public Form1()
        {
            m_Graphic = new GraphicClass();
            InitializeComponent();
            m_Graphic.Initialize(splitContainer1.Panel2, trackBar1.Minimum, trackBar1.Maximum, Color.LightSkyBlue);
            /*AssemblyGenerator assgen = new AssemblyGenerator("0");*/
            //подписка на событие колеса мыши (для зумирования модели)
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);

            //параметры по умолчанию для коэф-та визуализации
            textBox14.Text = "1";
            
            //параметры по умолчанию для прямоугольной оболочки
            textBoxRxPlane.Text = "10";
            textBoxRyPlane.Text = "10";
            textBoxSizeXPlane.Text = "10";
            textBoxSizeYPlane.Text = "10";
            textBoxSizeHPlane.Text = "1";

            //параметры по умолчанию для конической оболочки
            textBoxAngleCone.Text = "1";
            textBoxStartXCone.Text = "1";
            textBoxSizeXCone.Text = "10";
            textBoxSizeYCone.Text = "3";
            textBoxSizeHCone.Text = "1";

            //параметры по умолчанию для сферической оболочки
            textBoxRadiusSpheric.Text = "10";
            textBoxStartAngleSpheric.Text = "0.3";
            textBoxSizeXSpheric.Text = "1";
            textBoxSizeYSpheric.Text = "1";
            textBoxSizeHSpheric.Text = "1";

            //параметры по умолчанию для торообразной оболочки
            textBoxRadiusTorus.Text = "10";
            textBoxStartAngleTorus.Text = "-1";
            textBoxSizeXTorus.Text = "1";
            textBoxSizeYTorus.Text = "3";
            textBoxSizeHTorus.Text = "1";
            textBoxOffsetTorus.Text = "10";

            //параметры по умолчанию для цилиндрической оболочки
            textBoxRyCylindric.Text = "10";
            textBoxRxCylindric.Text = "10";
            textBoxSizeYCylindric.Text = "1";
            textBoxSizeHCylindric.Text = "1";

            //параметры для рёбер по оси X
            textBoxEdgesXCount.Text = "0";
            textBoxEdgesXSize.Text = "1";
            textBoxEdgesXh.Text = "1";
            //параметры для рёбер по оси Y
            textBoxEdgesYCount.Text = "0";
            textBoxEdgesYSize.Text = "1";
            textBoxEdgesYh.Text = "1";

            //подключение графического модуля
            //moduleGraphicLibrary = new GraphicLibrary();
            //ReturnCode rc = moduleGraphicLibrary.Init(this.splitContainer1.Panel2);
            //Console.WriteLine(rc);
            //if (rc != ReturnCode.Success)
            //{
            //    MessageBox.Show("Ошибка при подключении графического модуля");
            //    return;
            //}


            ////создание оболочек
            //CreateShells();

            //создание потока отрисовки
            threadDrawing = new Thread(new ThreadStart(threadDrawingEntryPoint));
            threadDrawing.Start();
            //m_Graphic.Frame();
            
        }

        /*
        /// Круглая кнопка
        private void roundButton_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

            System.Drawing.Drawing2D.GraphicsPath buttonPath =
                new System.Drawing.Drawing2D.GraphicsPath();

            // Set a new rectangle to the same size as the button's 
            // ClientRectangle property.
            System.Drawing.Rectangle newRectangle = roundButton.ClientRectangle;

            // Decrease the size of the rectangle.
            newRectangle.Inflate(-10, -10);

            // Draw the button's border.
            //e.Graphics.DrawEllipse(System.Drawing.Pens.Black, newRectangle);
            //e.Graphics.DrawArc(System.Drawing.Pens.Black, newRectangle, 0, 90);
            e.Graphics.DrawPie(System.Drawing.Pens.Transparent, newRectangle, 0, -90);
            // Increase the size of the rectangle to include the border.
            newRectangle.Inflate(1, 1);

            // Create a circle within the new rectangle.
            //buttonPath.AddEllipse(newRectangle);
            //buttonPath.AddArc(newRectangle,0,90);
            buttonPath.AddPie(newRectangle, 0, -90);

            // Set the button's Region property to the newly created 
            // circle region.
            roundButton.Region = new System.Drawing.Region(buttonPath);

        } */

        //вспомогательная функция для преобразования строкового значения в число с плавающей точкой
        bool GetValue(string value, ref float result)
        {
            return float.TryParse(value, out result);
            //try
            //{
            //    result = Convert.ToSingle(value); 
            //    return true;
            //}
            //catch
            //{
            //    if (value.Contains(','))
            //    {
            //        try
            //        {
            //            result = Convert.ToSingle(value.Replace(',', '.'));
            //            return true;
            //        }
            //        catch
            //        {
            //            return false;
            //        }
            //    }
            //    else
            //    {
            //        try
            //        {
            //            result = Convert.ToSingle(value.Replace('.', ','));
            //            return true;
            //        }
            //        catch
            //        {
            //            return false;
            //        }
            //    }
            //}
        }

        //создание оболочек для отображения
        bool CreateShells()
        {
            try
            {
                //moduleGraphicLibrary.DeleteAllModels();

                float radius_x = 0;
                float radius_y = 0;
                float size_x = 0;
                float size_y = 0;
                float height = 0;
                float conus_angle = 0;
                float start_x = 0;
                float d = 0;

                _ProgFun = textBoxProgFun.Text;

                float nhx = 0; //количество ребер по оси X
                float hwx = 0; //толщина ребра по оси X
                float sizeRebroX = 0; //размер ребра по оси X в процентах
                float nhy = 0; //количество ребер по оси Y
                float hwy = 0; //толщина ребра по оси Y
                float sizeRebroY = 0; //размер ребра по оси Y в процентах

                if (!GetValue(textBox14.Text, ref scaleCoef))
                {
                    MessageBox.Show("Ошибка в поле <коэф-т визуализации>");
                    return false;
                }

                if (!GetValue(textBoxEdgesXCount.Text, ref nhx))
                {
                    MessageBox.Show("Ошибка в поле <количество рёбер по оси X>");
                    return false;
                }
                if (!GetValue(textBoxEdgesYCount.Text, ref nhy))
                {
                    MessageBox.Show("Ошибка в поле <количество рёбер по оси Y>");
                    return false;
                }
                if (!GetValue(textBoxEdgesXSize.Text, ref sizeRebroX))
                {
                    MessageBox.Show("Ошибка в поле <размер ребра по оси X>");
                    return false;
                }
                sizeRebroX /= 100;
                if (!GetValue(textBoxEdgesYSize.Text, ref sizeRebroY))
                {
                    MessageBox.Show("Ошибка в поле <размер ребра по оси Y>");
                    return false;
                }
                sizeRebroY /= 100;
                if (!GetValue(textBoxEdgesXh.Text, ref hwx))
                {
                    MessageBox.Show("Ошибка в поле <толщина ребра по оси X>");
                    return false;
                }
                if (!GetValue(textBoxEdgesYh.Text, ref hwy))
                {
                    MessageBox.Show("Ошибка в поле <толщина ребра по оси Y>");
                    return false;
                }

                if (tabControl1.SelectedIndex == 0)
                {
                    //shell = new ShellRectangularModelWithEdgesNew();
                    if (!GetValue(textBoxRxPlane.Text, ref radius_x))
                    {
                        MessageBox.Show("Ошибка в поле <радиус по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxRyPlane.Text, ref radius_y))
                    {
                        MessageBox.Show("Ошибка в поле <радиус по оси Y>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeXPlane.Text, ref size_x))
                    {
                        MessageBox.Show("Ошибка в поле <размер по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeYPlane.Text, ref size_y))
                    {
                        MessageBox.Show("Ошибка в поле <размер по оси Y>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeHPlane.Text, ref height))
                    {
                        MessageBox.Show("Ошибка в поле <толщина оболочки>");
                        return false;
                    }

                    //if (!((ShellRectangularModelWithEdgesNew)(shell)).CreateModel(moduleGraphicLibrary.m_d3dDevice, radius_x, radius_y, size_x, size_y, height, (int)nhy, (int)nhx, hwy, hwx, sizeRebroY * size_x, sizeRebroX * size_y, Function, 0, (float)Convert.ToDouble(textBox14.Text),_ProgFun))
                    {
                        MessageBox.Show("Ошибка при создании прямоугольной оболочки");
                        return false;
                    }
                }

                if (tabControl1.SelectedIndex == 1)
                {
                    //shell = new ShellCylindarModelWithEdgesNew();
                    if (!GetValue(textBoxRyCylindric.Text, ref radius_y))
                    {
                        MessageBox.Show("Ошибка в поле <радиус по оси Y>");
                        return false;
                    }
                    if (!GetValue(textBoxRxCylindric.Text, ref size_x))
                    {
                        MessageBox.Show("Ошибка в поле <размер по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeYCylindric.Text, ref size_y))
                    {
                        MessageBox.Show("Ошибка в поле <угловой размер по оси Y>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeHCylindric.Text, ref height))
                    {
                        MessageBox.Show("Ошибка в поле <толщина оболочки>");
                        return false;
                    }

                    //if (!((ShellCylindarModelWithEdgesNew)(shell)).CreateModel(moduleGraphicLibrary.m_d3dDevice, radius_y, size_x, size_y, height, (int)nhy, (int)nhx, hwy, hwx, sizeRebroY * size_x, sizeRebroX * size_y, Function, 0, (float)Convert.ToDouble(textBox14.Text), _ProgFun))
                    {
                        MessageBox.Show("Ошибка при создании цилиндрической оболочки");
                        return false;
                    }                    
                }
                if (tabControl1.SelectedIndex == 2)
                {
                    //shell = new ShellConusModelWithEdgesNew();
                    if (!GetValue(textBoxAngleCone.Text, ref conus_angle))
                    {
                        MessageBox.Show("Ошибка в поле <угол конусности>");
                        return false;
                    }
                    if (!GetValue(textBoxStartXCone.Text, ref start_x))
                    {
                        MessageBox.Show("Ошибка в поле <начальная координата по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeXCone.Text, ref size_x))
                    {
                        MessageBox.Show("Ошибка в поле <размер по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeYCone.Text, ref size_y))
                    {
                        MessageBox.Show("Ошибка в поле <угловой размер по оси Y>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeHCone.Text, ref height))
                    {
                        MessageBox.Show("Ошибка в поле <толщина оболочки>");
                        return false;
                    }

                    //if (!((ShellConusModelWithEdgesNew)(shell)).CreateModel(moduleGraphicLibrary.m_d3dDevice, conus_angle, start_x, size_x, size_y, height, (int)nhy, (int)nhx, hwy, hwx, sizeRebroY * size_x, sizeRebroX * size_y, Function, 0, (float)Convert.ToDouble(textBox14.Text), _ProgFun))
                    {
                        MessageBox.Show("Ошибка при создании конической оболочки");
                        return false;
                    }                    
                }
                if (tabControl1.SelectedIndex == 3)
                {
                    //shell = new ShellSphereModellWithEdgesNew();
                    if (!GetValue(textBoxRadiusSpheric.Text, ref radius_x))
                    {
                        MessageBox.Show("Ошибка в поле <радиус>");
                        return false;
                    }
                    if (!GetValue(textBoxStartAngleSpheric.Text, ref start_x))
                    {
                        MessageBox.Show("Ошибка в поле <угловая начальная координата по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeXSpheric.Text, ref size_x))
                    {
                        MessageBox.Show("Ошибка в поле <угловой размер по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeYSpheric.Text, ref size_y))
                    {
                        MessageBox.Show("Ошибка в поле <угловой размер по оси Y>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeHSpheric.Text, ref height))
                    {
                        MessageBox.Show("Ошибка в поле <толщина оболочки>");
                        return false;
                    }

                    //if (!((ShellSphereModellWithEdgesNew)(shell)).CreateModel(moduleGraphicLibrary.m_d3dDevice, radius_x, start_x, size_x, size_y, height, (int)nhy, (int)nhx, hwy, hwx, sizeRebroY * size_x, sizeRebroX * size_y, Function, 0, (float)Convert.ToDouble(textBox14.Text), _ProgFun))
                    {
                        MessageBox.Show("Ошибка при создании сферической оболочки");
                        return false;
                    }                    
                }
                if (tabControl1.SelectedIndex == 4)
                {
                    //shell = new ShellTorusModellWithEdgesNew();
                    if (!GetValue(textBoxRadiusTorus.Text, ref radius_x))
                    {
                        MessageBox.Show("Ошибка в поле <радиус>");
                        return false;
                    }
                    if (!GetValue(textBoxStartAngleTorus.Text, ref start_x))
                    {
                        MessageBox.Show("Ошибка в поле <угловая начальная координата по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeXTorus.Text, ref size_x))
                    {
                        MessageBox.Show("Ошибка в поле <угловой размер по оси X>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeYTorus.Text, ref size_y))
                    {
                        MessageBox.Show("Ошибка в поле <угловой размер по оси Y>");
                        return false;
                    }
                    if (!GetValue(textBoxSizeHTorus.Text, ref height))
                    {
                        MessageBox.Show("Ошибка в поле <толщина оболочки>");
                        return false;
                    }
                    if (!GetValue(textBoxOffsetTorus.Text, ref d))
                    {
                        MessageBox.Show("Ошибка в поле <смещение от оси вращения>");
                        return false;
                    }

                    //if (!((ShellTorusModellWithEdgesNew)(shell)).CreateModel(moduleGraphicLibrary.m_d3dDevice, d, radius_x, start_x, size_x, size_y, height, (int)nhy, (int)nhx, hwy, hwx, sizeRebroY * size_x, sizeRebroX * size_y, Function, 0, (float)Convert.ToDouble(textBox14.Text), _ProgFun))
                    {
                        MessageBox.Show("Ошибка при создании торообразной оболочки");
                        return false;
                    }                    
                }



                //moduleGraphicLibrary.AddModel(shell);

                //if (checkBox2.Checked)
                 //   shell.SetAutoAnimation(true);
                //shell.SetAnimationWeight((float)trackBar2.Value / trackBar2.Maximum);
            }
            catch
            {
                return false;
            }
            return true;
        }
        
        //отработка события колеса мыши
        void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (m_Graphic != null)
            {
                int dist;
                if (e.Delta < 0)
                    dist = m_Graphic.ZoomModel(1);
                else
                    dist = m_Graphic.ZoomModel(-1);

                if (dist <= trackBar1.Maximum)
                {
                    trackBar1.Value = dist;
                }
            }
        }

        //private int iterQ = 0;
        //поток отрисовки
        private void threadDrawingEntryPoint()
        {
            while (!doExit)
            {
                m_Graphic.Frame();
                //тормознём поток чтобы не отрисовывать слишком часто
                Thread.Sleep(100);
            }
        }

        //отработка события нажатия кнопки мыши
        private void splitContainer1_Panel2_MouseDown(object sender, MouseEventArgs e)
        {
            //запомнить положение курсора мыши
            mousePosition = e.Location;
            //сделать панель рисования активной
            splitContainer1.Panel2.Focus();
        }

        //обработка события перемещения курсора с нажатой кнопкой мышки
        private void splitContainer1_Panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) || (e.Button == MouseButtons.Left))
            {
                Point stopPos = new Point(e.X, e.Y);

                float yaw, pitch, roll;
                m_Graphic.RotateModel(splitContainer1.Panel2, mousePosition, stopPos, out yaw, out pitch, out roll);
                
                // получили углы Эйлера
                numUpDownTeta.Value = (decimal)(yaw * 180 / Math.PI);
                numUpDownPhi.Value = (decimal)(pitch * 180 / Math.PI);
                numUpDownPsi.Value = (decimal)(roll * 180 / Math.PI);

                //запомнить положение курсора мыши
                mousePosition = e.Location;
            }
        }

        //обработка события закрытия формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //устанавливаем флаг выхода из приложения
            doExit = true;
        }

        ////////////////////////////////////////////////////////////////////
        ////////////////////ХРЕНЬ///////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////
        //функция прогиба
        float Function(float x, float y)
        {
            //Form1.progibSeFunc = new AssemblyGenerator(textBox7.Text).Function;
            /*MessageBox.Show(Convert.ToString());*/
            /*AssemblyGenerator assgen = new AssemblyGenerator("Math.Sin(.3141592654 * x - 471.2388981)");*/
            
            /*double rrt = assgen.Function(x, y);*/
            /*MessageBox.Show(Convert.ToString(rrt));*/
            //double prog = rrt;//progibSeFunc(x, y);
            /*double prog = prog = .142181400775034 * Math.Sin(.3141592654 * x - 471.2388981) * Math.Sin(2.0 * y) + .322905743049186 * Math.Sin(.3141592654 * x - 471.2388981) * Math.Sin(6.0 * y) + 0.321202080966756e-1 * Math.Sin(.3141592654 * x - 471.2388981) * Math.Sin(10.0 * y) - 0.149248468385672e-1 * Math.Sin(.3141592654 * x - 471.2388981) * Math.Sin(14.0 * y) - 0.484832595466027e-4 * Math.Sin(.6283185308 * x - 942.4777962) * Math.Sin(2.0 * y) - 0.278936754819206e-3 * Math.Sin(.6283185308 * x - 942.4777962) * Math.Sin(6.0 * y) - 0.612031099322007e-4 * Math.Sin(.6283185308 * x - 942.4777962) * Math.Sin(10.0 * y) + 0.254879978716167e-4 * Math.Sin(.6283185308 * x - 942.4777962) * Math.Sin(14.0 * y) - 0.246710803971507e-1 * Math.Sin(.9424777962 * x - 1413.716694) * Math.Sin(2.0 * y) - 0.637616156961231e-2 * Math.Sin(.9424777962 * x - 1413.716694) * Math.Sin(6.0 * y) + 0.229142556177678e-1 * Math.Sin(.9424777962 * x - 1413.716694) * Math.Sin(10.0 * y) - 0.731197904248484e-2 * Math.Sin(.9424777962 * x - 1413.716694) * Math.Sin(14.0 * y) + 0.167345462792983e-5 * Math.Sin(1.256637062 * x - 1884.955592) * Math.Sin(2.0 * y) - 0.918789244473551e-5 * Math.Sin(1.256637062 * x - 1884.955592) * Math.Sin(6.0 * y) - 0.223235768414056e-4 * Math.Sin(1.256637062 * x - 1884.955592) * Math.Sin(10.0 * y) + 0.884748695332273e-5 * Math.Sin(1.256637062 * x - 1884.955592) * Math.Sin(14.0 * y);*/
            return 1;// (float)(scaleCoef * prog);
        }

        //отработка нажатия кнопки "применить"
        private void button2_Click(object sender, EventArgs e)
        {
            //CreateShells();
        }

        //отработка события смены типа оболочки
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CreateShells();
        }

        //отработка события включения автоматической анимации
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            //shell.SetAutoAnimation(((CheckBox)sender).Checked);
            
        }

        //отработка события ручной анимации
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //shell.SetAnimationWeight(((float)((TrackBar)sender).Value / ((TrackBar)sender).Maximum));
            if (animateProgFunMass != null)
            {
                _ProgFun = animateProgFunMass[trackBar2.Value];
            //    CreateShells();
            }
        }

        private void numUpDownTeta_ValueChanged(object sender, EventArgs e)
        {
            if (numUpDownTeta.Focused)
                m_Graphic.RotateModel(
                           (float)(numUpDownPsi.Value) * RADIANS,
                           (float)(numUpDownPhi.Value) * RADIANS,
                           (float)(numUpDownTeta.Value) * RADIANS
                           );
        }

        private void numUpDownPhi_ValueChanged(object sender, EventArgs e)
        {
            if (numUpDownPhi.Focused)
                m_Graphic.RotateModel(
                    (float)(numUpDownPsi.Value) * RADIANS,
                    (float)(numUpDownPhi.Value) * RADIANS,
                    (float)(numUpDownTeta.Value) * RADIANS
                    );
        }

        private void numUpDownPsi_ValueChanged(object sender, EventArgs e)
        {
            if (numUpDownPsi.Focused)
                m_Graphic.RotateModel(
                    (float)(numUpDownPsi.Value) * RADIANS,
                    (float)(numUpDownPhi.Value) * RADIANS,
                    (float)(numUpDownTeta.Value) * RADIANS
                    );
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            m_Graphic.SetCameraDistance(trackBar1.Value);
        }

        //private Image createImage(Control c)
        
            
            //{ TEXTURE2D --->> TO FILE or SAVEtoFILE .... !!!


        //    Bitmap bitmap = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(CaptureScreen(), SlimDX.Direct3D9.ImageFileFormat.Png));

        //    //Graphics graphicsPanel = c.CreateGraphics();
        //    //Bitmap bitmap = CreateBitmapFromGraphics(graphicsPanel, 0, 50, c.Width, c.Height-50);

        //    //graphicsPanel.CopyFromScreen()
        //    //Bitmap res = new Bitmap(c.Width, c.Height);
        //    //c.DrawToBitmap(res, new Rectangle(Point.Empty, c.Size));
        //    return bitmap;
        //}

        //----------------------------------------------------------
        // ПОКА НЕ РАБОТАЕТ
        //----------------------------------------------------------
        private void jPEGFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(m_Graphic.GetImage(SlimDX.Direct3D11.ImageFileFormat.Jpg));
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Jpeg Image|*.jpg";
            dialog.Title = "Сохранить рисунок";
            dialog.AddExtension = true;
            dialog.FileName = "shell_model";
            dialog.ShowDialog();
            if (dialog.FileName != string.Empty)
                bitmap.Save(dialog.FileName, ImageFormat.Jpeg);
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(m_Graphic.GetImage(SlimDX.Direct3D11.ImageFileFormat.Png));
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Png Image|*.png";
            dialog.Title = "Сохранить рисунок";
            dialog.FileName = "shell_model";
            dialog.ShowDialog();
            if (dialog.FileName != string.Empty)
                bitmap.Save(dialog.FileName, ImageFormat.Png);
        }

        private void bitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(m_Graphic.GetImage(SlimDX.Direct3D11.ImageFileFormat.Bmp));
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bitmap Image|*.bmp";
            dialog.Title = "Сохранить рисунок";
            dialog.FileName = "shell_model";
            dialog.ShowDialog();
            if (dialog.FileName != string.Empty)
                bitmap.Save(dialog.FileName, ImageFormat.Bmp);
        }

        private void gIFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ////if (animateProgFunMass == null)
            ////    return;
            //Bitmap bitmap;
            //GifBitmapEncoder gEnc = new GifBitmapEncoder();
            //List<Bitmap> images = new List<Bitmap>();
            //for (float i = 0; i <1.2f; i+=0.02f)
            //{
            //    shell.SetAnimationWeight(i);
            //    moduleGraphicLibrary.Draw();
            //    bitmap = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(moduleGraphicLibrary.GetSurface(), SlimDX.Direct3D9.ImageFileFormat.Png));
            //    images.Add(bitmap);
            //}

            //foreach (Bitmap bmpImage in images)
            //{
            //    var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            //        bmpImage.GetHbitmap(),
            //        IntPtr.Zero,
            //        System.Windows.Int32Rect.Empty,
            //        BitmapSizeOptions.FromEmptyOptions());
            //    gEnc.Frames.Add(BitmapFrame.Create(src));
            //}

            //SaveFileDialog dialog = new SaveFileDialog();
            //dialog.Filter = "Gif Image|*.gif";
            //dialog.Title = "Сохранить рисунок";
            //dialog.ShowDialog();
            //if (dialog.FileName != string.Empty)
            //{
            //    //bitmap.Save(dialog.FileName, ImageFormat.Gif);
            //    using (FileStream fs = new FileStream(dialog.FileName, FileMode.Create))
            //    {
            //        gEnc.Metadata = new BitmapMetadata("gif");
            //        gEnc.Save(fs);
            //    }
            //}
        }

        private Dictionary<int, string> animateProgFunMass;
        private string _ProgFun;

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text file|*.txt";
            dialog.Title = "Открыть файл";
            dialog.ShowDialog();
            if (dialog.FileName != string.Empty)
                using (FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    Dictionary<string, string> FileDataDictionary = new Dictionary<string, string>();
                    StreamReader sr = new StreamReader(fs, Encoding.UTF8);

                    string[] StringArrayFromLine;
                    while (!sr.EndOfStream)
                    {
                        StringArrayFromLine = sr.ReadLine().Split('\t');
                        FileDataDictionary.Add(StringArrayFromLine[0], StringArrayFromLine[1]);
                        if(StringArrayFromLine[0]=="Mass")
                        {
                            string[] str;
                            animateProgFunMass = new Dictionary<int, string>();
                            for (int i = 0; i < int.Parse(StringArrayFromLine[1]); i++)
                            {
                                str = sr.ReadLine().Split('\t');
                                animateProgFunMass.Add(i, str[1]);
                            }
                            trackBar2.Maximum = animateProgFunMass.Count - 1;
                        }
                    }
                    sr.Close();
                    fs.Close();

                    textBoxRxPlane.Text = FileDataDictionary["Rx"];
                    textBoxRyPlane.Text = FileDataDictionary["Ry"];
                    textBoxSizeXPlane.Text = FileDataDictionary["a"];
                    textBoxSizeYPlane.Text = FileDataDictionary["b"];
                    textBoxSizeHPlane.Text = FileDataDictionary["h"];

                    _ProgFun = FileDataDictionary["W"];
                    textBoxProgFun.Text = _ProgFun;

                    CreateShells();
                }
        }

        private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
        {
            if (m_Graphic != null)
            {
                m_Graphic.Resize(splitContainer1.Panel2);
                //CreateShells();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doExit = true;
            this.Close();
        }

        private void createPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Graphic.RotateModel(0f,0f,0f);
            numUpDownPhi.Value = 0;
            numUpDownPsi.Value = 0;
            numUpDownTeta.Value = 0;
            m_Graphic.SetCameraDistance(50);
            trackBar1.Value = 50;
        }

        private void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Graphic.BackgroundColor = Color.White;
            label1.BackColor= Color.White;
            label22.BackColor = Color.White;
            label25.BackColor = Color.White;
            trackBar1.BackColor = Color.White;
        }

        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Graphic.BackgroundColor = Color.LightSkyBlue;
            label1.BackColor = Color.LightSkyBlue;
            label22.BackColor = Color.LightSkyBlue;
            label25.BackColor = Color.LightSkyBlue;
            trackBar1.BackColor = Color.LightSkyBlue;
        }
    }
}
