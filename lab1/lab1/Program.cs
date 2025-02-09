using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            using(Bitmap image = new Bitmap("image2.bmp"))
            {
                Console.WriteLine($"Format: {image.RawFormat}");
                Console.WriteLine($"Size: {image.Width}x{image.Height}");
                Console.WriteLine($"PixelFormat: {image.PixelFormat}");
                var size = image.Size;
                for(int i = 0; i < size.Width; i++)
                {
                    for(int j = 0; j < size.Height; j++)
                    {
                        Color pixel = image.GetPixel(i, j);
                        int grayScale = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                        Color grayPixel = Color.FromArgb(grayScale, grayScale, grayScale);
                        image.SetPixel(i, j, grayPixel);
                    }
                }

                image.Save("grayImage.bmp", ImageFormat.Bmp);
            }
        }
    }
}
