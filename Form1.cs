using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;//for working mouse and buttons
using Emgu.CV;
using Emgu.CV.Structure;

namespace mini_game_fishing_Albion_helper
{

    public partial class Form1 : Form
    {
        #region options_of_program
        Stopwatch stopwatch = new Stopwatch();//debug for count
        int state_of_fishing = -1;//состояние процесса рыбалки
        /*
         * 2 - мини-игра
         */
        Point pos_of_match = new Point(0, 0);
        bool mouse_pressed_now = false;
        bool prev_state_fishing = false;
        #endregion

        #region OPENCV_options

        //string path_for_files = "C:\\Users\\Andrey\\Desktop\\";
        string path = "D:\\";

        #endregion

        #region mouse_options
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        #endregion

        #region try5_find_matches_ITS_FUCKING_WORKING!
        public void Try5(string where, string what, float percent)
        {
            // Запускаем внутренний таймер объекта Stopwatch
            stopwatch.Start();
            Image<Bgr, byte> source = new Image<Bgr, byte>(path + where + ".bmp"); // Image A где ищем, картинка рыбалки
            Image<Bgr, byte> template = new Image<Bgr, byte>(path + what + ".bmp"); // Image B что ищем
            Image<Bgr, byte> imageToShow = source.Copy();
            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > percent)//0.9
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    Rectangle match = new Rectangle(maxLocations[0], template.Size);
                    imageToShow.Draw(match, new Bgr(Color.Red), 3);
                    pos_of_match = maxLocations[0];
                    label4.Text = "Позиция совпадения: X - " + pos_of_match.X + " | Y - " + pos_of_match.Y;
                }
                else
                {
                    if (what != "fish_is_mine")
                    {
                        label4.Text = "Совпадений не найдено";
                    }
                }
                label3.Text = "Процент совпадения: " + Math.Round(maxValues[0], 3).ToString();
            }
            imageToShow.Save(path + "final.bmp");
            stopwatch.Stop();// Останавливаем внутренний таймер объекта Stopwatch
            label5.Text = "Время опознания: " + (stopwatch.ElapsedMilliseconds).ToString() + " мс";//time of exec CV
            stopwatch.Reset();
        }
        #endregion

        public void MyMouseMove(int x, int y)
        {
            PointConverter pc = new PointConverter();
            System.Drawing.Point pt = new System.Drawing.Point(x, y);
            Cursor.Position = pt;
        }

        public Form1()
        {
            TopMost = true;
            InitializeComponent();
        }

        public void Mouse_press(int state)
        {
            if (state == 0)
            {
                //Call the imported function with the cursor's current position
                uint X = (uint)Cursor.Position.X;
                uint Y = (uint)Cursor.Position.Y;
                mouse_event(MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
                //Thread.Sleep(time); 
                //mouse_event(MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                mouse_pressed_now = true;
            }
            else
            if (state == 1)
            {
                //Call the imported function with the cursor's current position
                uint X = (uint)Cursor.Position.X;
                uint Y = (uint)Cursor.Position.Y;
                mouse_event(MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                mouse_pressed_now = false;
            }

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = "Позиция курсора: " + Cursor.Position.X + " | " + Cursor.Position.Y;
        }

        private void Get_screenshot(int x1, int y1, int x2, int y2)
        {
            Bitmap printscreen = new Bitmap(x2 - x1, y2 - y1/*Screen.PrimaryScreen.Bounds.Height*/);

            Graphics graphics = Graphics.FromImage(printscreen as Image);

            graphics.CopyFromScreen(x1, y1, 0, 0, printscreen.Size);

            printscreen.Save(path + "screenshot.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //Close();
        }

        private void Start_fishing()
        {
            //if (timer5.Enabled==false)
            //{
            //timer5.Enabled = true;
            //}
            state_of_fishing = 0;//proc of fish         start
            button2.Enabled = false;//start             false
            button3.Enabled = true;//stop               true
            timer3.Enabled = true;//timer for fishing   run
        }

        private void Stop_fishing()
        {
            state_of_fishing = -1;//proc of fish        stop
            button2.Enabled = true;//start              true
            button3.Enabled = false;//stop              false
            timer3.Enabled = false;//timer for fishing  stop
            label6.Text = "Ждем...";
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Start_fishing();
        }

        private void Timer3_Tick(object sender, EventArgs e)
        {
            state_of_fishing = 2;
            pos_of_match.X = 0;
            pos_of_match.Y = 0;

            if (state_of_fishing >= 0)
            {
                switch (state_of_fishing)
                {
                    case 2://+ мини-игра, ловим рыбу
                        {
                            label6.Text = "Понеслась! Рыбачим!";
                            Get_screenshot(850, 520, 1070, 570);//скрин мини-игры адекватные = начальные + 100/170 = -120/-80 | начальный значения 750, 350, 1200, 650
                            pos_of_match.X = 0;
                            Try5("screenshot", "mini-game", 0.65f);
                            if (pos_of_match.X > 20 && pos_of_match.X < 160)
                            {
                                Mouse_press(0);
                                //Close();
                                prev_state_fishing = true;
                            }
                            else
                            if (pos_of_match.X > 20 && pos_of_match.X > 160)
                            {
                                Mouse_press(1);
                                prev_state_fishing = true;
                            }
                            
                            if (pos_of_match.X==0 && prev_state_fishing == true)
                            {
                                Mouse_press(1);
                                prev_state_fishing = false;
                            }
                            Get_screenshot(767, 66, 1151, 88);//скрин выигрыша, поймали или нет
                            //Close();
                            pos_of_match.X = 0;
                            Try5("screenshot", "fish_is_mine", 0.75f);
                            if (pos_of_match.X > 0 && mouse_pressed_now == false)
                            {
                                mouse_pressed_now = true;
                            }
                            label6.Text = "успокоились";
                        }
                        break;
                }
                pos_of_match.X = 0;
            }
            else
            {
                Stop_fishing();
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Stop_fishing();
        }
    }
}
