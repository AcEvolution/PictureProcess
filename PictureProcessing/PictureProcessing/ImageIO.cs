using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PictureProcessing
{
    // 声明：ImageIO 部分功能实现思路参考了互联网相关资料，具体实现为笔者自行完成，相关部分已用 * 号标注，未使用 * 标注为完全自主完成。
    class ImageIO
    {
        // 变量存储区
        private Bitmap _bitmap = null;
        
        private string _path = null;
        
        private IntPtr Iptr = IntPtr.Zero;
        
        private BitmapData bitmapData = null;

        // 数据接口
        public byte[] Pixels { get; set; }
        
        public int Depth { get; private set; }
        
        public int Width { get; private set; }
        
        public int Height { get; private set; }
        
        public string Path { get => _path; set => _path = value; }
        
        public Bitmap Bitmap { get => _bitmap; set => _bitmap = value; }

        // 构造函数
        public ImageIO(string path)
        {
            Bitmap = new Bitmap(Image.FromFile(path));
            Path = path;
            Width = Bitmap.Width;
            Height = Bitmap.Height;
        }

        public ImageIO(ImageIO src)
        {
            Bitmap = new Bitmap(src.Width, src.Height);
            Path = src.Path;
            Width = Bitmap.Width;
            Height = Bitmap.Height;
        }

        public ImageIO(string path, int width, int height)
        {
            Bitmap = new Bitmap(width, height);
            Path = path;
            Width = Bitmap.Width;
            Height = Bitmap.Height;
        }

        public ImageIO(string path, string savePath)
        {
            Bitmap = new Bitmap(Image.FromFile(path));
            Path = savePath;
            Width = Bitmap.Width;
            Height = Bitmap.Height;
        }

        // * 锁定Bitmap
        public void LockBits()
        {
            // 总像素个数
            int PixelCount = Width * Height;

            // 锁定框
            Rectangle rect = new Rectangle(0, 0, Width, Height);

            // RGB深度 8, 24 或 32
            Depth = System.Drawing.Bitmap.GetPixelFormatSize(Bitmap.PixelFormat);

            // 锁定Bitmap并返回data
            bitmapData = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Bitmap.PixelFormat);

            // 创建数组存取数值
            int step = Depth / 8;
            Pixels = new byte[PixelCount * step];
            Iptr = bitmapData.Scan0;

            // 复制数据
            Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
        }

        // * 解锁Bitmap
        public void UnlockBits()
        {
            // 复制数据
            Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

            // 解锁Bitmap
            Bitmap.UnlockBits(bitmapData);
        }

        // * 获取像素位置颜色
        public Color GetPixel(int x, int y)
        {
            Color color = Color.Empty;

            // 颜色占的位数
            int step = Depth / 8;

            // 获取颜色索引
            int i = ((y * Width) + x) * step;

            // 根据颜色深度生成颜色
            if (Depth == 32)
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3];
                color = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24)
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                color = Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            {
                byte c = Pixels[i];
                color = Color.FromArgb(c, c, c);
            }
            return color;
        }

        // * 设置像素位置颜色
        public void SetPixel(int x, int y, Color color)
        {
            // 颜色占的位数
            int step = Depth / 8;

            // 获取颜色索引
            int i = ((y * Width) + x) * step;

            // 根据颜色深度生成颜色
            if (Depth == 32)
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24)
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            {
                Pixels[i] = color.B;
            }
        }

        // 保存图片
        public bool SaveImage(string type)
        {
            // 根据格式保存图片
            switch(type)
            {
                case "bmp":
                    {
                        this.Bitmap.Save(this.Path, ImageFormat.Bmp);
                        break;
                    }
                case "jpg":
                    {
                        this.Bitmap.Save(this.Path, ImageFormat.Jpeg);
                        break;
                    }
                case "png":
                    {
                        this.Bitmap.Save(this.Path, ImageFormat.Png);
                        break;
                    }
            }
            return true;
        }
    }
}
