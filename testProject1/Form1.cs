﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testProject1
{
    public struct BorderPoint
    {
        public Point point;
        public int flag;   // откуда пришли

        public BorderPoint(int x, int y, int f)
        {
            point = new Point(x, y);
            flag = f;
        }
    }

    public partial class Form1 : Form
    {
        Graphics g;
        int size;
        Bitmap bitmap;
        Bitmap pattern;
        BitmapData bitmD;
        byte[] imageBytes;
        int RowSizeBytes;
        int total_size;

        Point mouse;

        public Form1()
        {
            InitializeComponent();
            this.MouseMove += new MouseEventHandler(this.MyMouseMove);
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bitmap);
            g.FillRectangle(new SolidBrush(pictureBox1.BackColor), 0, 0, pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;

            size = 1;
            label1.BackColor = colorDialog1.Color;
        }

        //---------------------------------------------------------------------------------

        public void GetBitmapData(PictureBox CurrentPicture)
        {
            Rectangle bounds = new Rectangle(
                0, 0, CurrentPicture.Image.Width, CurrentPicture.Image.Height);
            bitmD = ((Bitmap)pictureBox1.Image).LockBits(
                bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            RowSizeBytes = bitmD.Stride;
            total_size = RowSizeBytes * bitmD.Height;
            imageBytes = new byte[total_size];
            System.Runtime.InteropServices.Marshal.Copy(bitmD.Scan0, imageBytes, 0, total_size);
        }

        public void ReturnBitmapData(PictureBox CurrentPicture)
        {
            System.Runtime.InteropServices.Marshal.Copy(imageBytes, 0, bitmD.Scan0, total_size);
            ((Bitmap)pictureBox1.Image).UnlockBits(bitmD);
            bitmD = null;
        }

        public void GetBitmapData(Bitmap bitmap)
        {
            Rectangle bounds = new Rectangle(
                0, 0, bitmap.Width, bitmap.Height);
            bitmD = bitmap.LockBits(
                bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            RowSizeBytes = bitmD.Stride;
            total_size = RowSizeBytes * bitmD.Height;
            imageBytes = new byte[total_size];
            System.Runtime.InteropServices.Marshal.Copy(bitmD.Scan0, imageBytes, 0, total_size);
        }

        public void ReturnBitmapData(Bitmap bitmap)
        {
            System.Runtime.InteropServices.Marshal.Copy(imageBytes, 0, bitmD.Scan0, total_size);
            bitmap.UnlockBits(bitmD);
            bitmD = null;
        }

        //-------------------------Функции, привязанные к элементам формы--------------------------------------------------------

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouse = e.Location;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            size = trackBar1.Value;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                (sender as Label).BackColor = colorDialog1.Color;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (radioButton2.Checked && radioButton2.Enabled)
            {
                FillHard(new Point(e.X, e.Y), label2.BackColor, bitmap.GetPixel(e.X, e.Y));
            }
            else if (radioButton1.Checked && radioButton1.Enabled)
            {
                if (pictureBox2.Image == null)
                {
                    MessageBox.Show("Не установлен шаблон.");
                    return;
                }
                GetBitmapData(pattern);
                ReturnBitmapData(pattern);
                FillPattern(new Point(e.X, e.Y), int.MaxValue, pattern.Width / 2, bitmap.GetPixel(e.X, e.Y));
            }
            else if (найтиToolStripMenuItem.Checked)
            {
                GetBorderPoints(bitmap, Color.Red);
                найтиToolStripMenuItem.Checked = false;
            }
            else if (найтиИЗалитьToolStripMenuItem.Checked)
            {
                FillWithBorder(GetBorderPoints(bitmap, Color.Empty), Color.Red);
                найтиИЗалитьToolStripMenuItem.Checked = false;
            }
            pictureBox1.Refresh();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton3.Checked)
            {
                pictureBox1.MouseMove -= MyMouseMove;
            }
            else
            {
                pictureBox1.MouseMove += MyMouseMove;
            }
        }



        //----------------------------Элементы меню-----------------------------------------------------

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Нет изображения");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = "jpg";
            sfd.Filter =
            "Изображение (*.pmb;*.jpg;*.jpeg;*.tif;*.tiff;*.gif;*.png;*.exif)|*.pmb;*.jpg;*.jpeg;*.tif;*.tiff;*.gif;*.png;*.exif";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                bitmap.Save(sfd.FileName);
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (pattern != null)
                    pattern.Dispose();
                pattern = new Bitmap(ofd.FileName);
                pictureBox2.Image = pattern;
            }
        }

        private void найтиToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (найтиToolStripMenuItem.Checked == true)
            {
                найтиИЗалитьToolStripMenuItem.Checked = false;
                panel1.Enabled = false;
                pictureBox1.Cursor = Cursors.Hand;
            }
            else if (найтиИЗалитьToolStripMenuItem.Checked == true)
            {
                найтиToolStripMenuItem.Checked = false;
                panel1.Enabled = false;
                pictureBox1.Cursor = Cursors.Hand;
            }
            else
            {
                panel1.Enabled = true;
                pictureBox1.Cursor = Cursors.Default;
            }
        }

        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            g.Clear(pictureBox1.BackColor);
            if (pattern != null)
                pattern.Dispose();
            pictureBox2.Image = null;
            pictureBox1.Refresh();
        }

        //------------------------------Функции заливки---------------------------------------------------


        private void FillHard(Point e, Color fillingColor, Color backColor)
        {
            // processing
            int x1 = e.X, x2 = e.X;
            int y1 = e.Y, y2 = e.Y;
            Color pixelColor = new Color();
            pixelColor = bitmap.GetPixel(x1, y1);//цвет текущего пикселя

            if (pixelColor.ToArgb() != backColor.ToArgb() || pixelColor.ToArgb() == fillingColor.ToArgb())
                return;
            // если цвет пиксела равен цвету фона
            while (pixelColor.ToArgb() == backColor.ToArgb() && x1 > 0)
            {
                // цвет левого пикселя
                --x1;
                pixelColor = bitmap.GetPixel(x1, y1);
            }
            pixelColor = backColor;
            while (pixelColor.ToArgb() == backColor.ToArgb() && x2 < pictureBox1.Image.Width - 1)
            {
                // цвет правого пикселя
                ++x2;
                pixelColor = bitmap.GetPixel(x2, y2);
            }

            // рисуем линию
            g.DrawLine(new Pen(fillingColor, 1), new Point(++x1, y1), new Point(--x2, y2));
            for (int i = x1; i < x2; ++i)
            {
                if (y1 < pictureBox1.Image.Height - 1)
                    FillHard(new Point(i, y1 + 1), fillingColor, backColor);
                if (y1 > 0)
                    FillHard(new Point(i, y2 - 1), fillingColor, backColor);
            }
        }

        

        private void FillPattern(Point e, int x, int y, Color backColor)
        {
            // processing
            int x1 = e.X, x2 = e.X;
            int y1 = e.Y, y2 = e.Y;
            Color pixelColor = new Color();
            pixelColor = bitmap.GetPixel(x1, y1);

            if (pixelColor.ToArgb() != backColor.ToArgb())
                return;
            // если цвет пиксела равен цвету фона
            while (pixelColor.ToArgb() == backColor.ToArgb() && x1 > 0)
            {
                // цвет левого пикселя
                --x1;
                pixelColor = bitmap.GetPixel(x1, y1);
            }
            pixelColor = backColor;
            while (pixelColor.ToArgb() == backColor.ToArgb() && x2 < pictureBox1.Image.Width - 1)
            {
                // цвет правого пикселя
                ++x2;
                pixelColor = bitmap.GetPixel(x2, y2);
            }

            // рисуем линию
            int newX;
            newX = (pattern.Width / 2) - (mouse.X - x1) % pattern.Width;
            if (newX < 0)
            {
                newX = (pattern.Width + newX);
            }

            if (y == -1)
                y = (pattern.Height) - 1;
            ++x1;
            --x2;
            for (int i = x1 - 1; i <= x2; ++i)
            {
                if (i == pictureBox1.Width)
                {
                    i--;
                }
                bitmap.SetPixel(i, y1, pattern.GetPixel(newX++ % pattern.Width, y % pattern.Height));
            }

            for (int i = x1; i < x2; ++i)
            {
                if (y1 < pictureBox1.Image.Height - 1)
                    FillPattern(new Point(i, y1 + 1), mouse.X, y + 1, backColor);
                if (y1 > 0)
                    FillPattern(new Point(i, y2 - 1), mouse.X, y - 1, backColor);
            }
        }

        //------------------------------Нахождение границ и заливка доп---------------------------------------------------

        private void FillWithBorder(List<BorderPoint> l, Color fillingColor)
        {

            if (l != null)
            {
                bool flag = true;
                int i = 0;
                int count = 0;
                while (l.Count() > 0)
                {
                    flag = true;
                    int k = l[i].point.Y;
                    List<BorderPoint> A = l.FindAll(x => x.point.Y == k);
                    l.RemoveAll(x => x.point.Y == k);
                    A.Sort(CompareBorderPoints);
                    if (A.Count() > 1)
                    {
                        for (int j = 0; j < A.Count() - 1; j++)
                        {
                            if ((A[j + 1].point.X - A[j].point.X) > 1)
                            {
                                if (count >= 100)
                                {
                                    flag = !flag;
                                    count = 0;
                                }
                                if (flag)
                                    g.DrawLine(new Pen(fillingColor, 1), A[j].point.X + 1, k, A[j + 1].point.X - 1, k);
                                flag = !flag;
                            }
                            else
                                count++;
                            
                        }
                    }
                    
                }

            }
    }
         

        //---------------------------------------------------------------------------------

        private void Draw(MouseEventArgs e)
        {
            g.DrawLine(new Pen(label1.BackColor, size), mouse, e.Location);
            mouse = e.Location;
            pictureBox1.Refresh();
        }

        private void Clear(MouseEventArgs e)
        {
            g.DrawLine(new Pen(pictureBox1.BackColor, size), mouse, e.Location);
            mouse = e.Location;
            pictureBox1.Refresh();
        }

        private void MyMouseMove(object sender, MouseEventArgs e)
        {
            if (!radioButton3.Enabled || !radioButton3.Checked)
                return;
            if (e.Button == MouseButtons.Left)
            {
                Draw(e);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Clear(e);
            }
        }

        void ResetExceptionState(Control control)
        {
            typeof(Control).InvokeMember("SetState", System.Reflection.BindingFlags.NonPublic |
              System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance, null,
              control, new object[] { 0x400000, false });
        }
        

        private BorderPoint FindStartPoint(Bitmap sourceImage)
        {
            int x1 = mouse.X;
            int y1 = mouse.Y;
            int borderX = -1, borderY = -1;
            Color backgroundColor = sourceImage.GetPixel(mouse.X, mouse.Y);
            Color pixelColor = backgroundColor;
            while (pixelColor.ToArgb() == backgroundColor.ToArgb() && y1 > 0)
            {
                while (pixelColor.ToArgb() == backgroundColor.ToArgb() && x1 < sourceImage.Width - 1)
                {
                    // цвет левого пикселя
                    ++x1;
                    pixelColor = sourceImage.GetPixel(x1, y1);
                }
                if (borderX <= x1)
                {
                    borderY = y1;
                    borderX = x1;
                }
                y1--;
                x1 = mouse.X;
                pixelColor = sourceImage.GetPixel(x1, y1);
            }
            return new BorderPoint(borderX, borderY, 6);
        }

        private List<BorderPoint> GetBorderPoints(Bitmap sourceImage, Color c)
        {
            List<BorderPoint> border = new List<BorderPoint>();
            BorderPoint cur = FindStartPoint(sourceImage);
            border.Add(cur);
            BorderPoint start = cur;
            BorderPoint next = new BorderPoint(-1, -1, -1);
            Color borderColor = sourceImage.GetPixel(cur.point.X, cur.point.Y);
            int lookTo = -1;
            do
            {
                lookTo = cur.flag - 2;
                if (lookTo == -1)
                    lookTo = 7;
                else if (lookTo == -2)
                    lookTo = 6;
                int t = lookTo;
                do
                {
                    next.point = cur.point;
                    switch (lookTo)
                    {
                        case 0: next.point.X++; next.flag = 0; break;
                        case 1: next.point.X++; next.point.Y--; next.flag = 1; break;
                        case 2: next.point.Y--; next.flag = 2; break;
                        case 3: next.point.X--; next.point.Y--; next.flag = 3; break;
                        case 4: next.point.X--; next.flag = 4; break;
                        case 5: next.point.X--; next.point.Y++; next.flag = 5; break;
                        case 6: next.point.Y++; next.flag = 6; break;
                        case 7: next.point.X++; next.point.Y++; next.flag = 7; break;
                    }
                    if (next.point == start.point)
                        break;
                    if (next.point.X < 0 || next.point.X > pictureBox1.Width || next.point.Y > pictureBox1.Height || next.point.Y < 0)
                    {
                        MessageBox.Show("Границы не найдены");
                        return null;
                    }
                    if (sourceImage.GetPixel(next.point.X, next.point.Y) == borderColor)
                    {
                        border.Add(next);
                        if (cur.flag == 2 && next.flag == 6 || cur.flag == 6 && next.flag == 2)
                            border.Add(next);
                        else if (cur.flag == 1 && next.flag == 7 || cur.flag == 5 && next.flag == 3)
                            border.Add(next);
                        else if (cur.flag == 4 && next.flag == 0 || cur.flag == 4 && next.flag == 4)
                            border.Add(next);
                        cur = next;
                        next = new BorderPoint(-1, -1, -1);
                        break;
                    }
                    lookTo = (lookTo + 1) % 8;
                } while (lookTo != t);
            } while (next.point != start.point);

            if (c != Color.Empty)
            {
                foreach (var x in border)
                    sourceImage.SetPixel(x.point.X, x.point.Y, c);
                return null;
            }
            else
            {
                border.Sort(CompareBorderPoints);
               

                return border;
            }
        }

        

        private static int CompareBorderPoints(BorderPoint a, BorderPoint b)
        {
            if (a.point.Y == b.point.Y)
                return a.point.X.CompareTo(b.point.X);
            else if (a.point.Y < b.point.Y)
                return -1;
            else return 1;
        }

        private static int ComparePoints(Point a, Point b)
        {
            if (a.Y == b.Y)
                return a.X.CompareTo(b.X);
            else if (a.Y < b.Y)
                return -1;
            else return 1;
        }


    }
}
