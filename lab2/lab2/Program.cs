using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap image = new Bitmap("image.bmp");
            Console.WriteLine($"Format: {image.RawFormat}");
            Console.WriteLine($"Size: {image.Width}x{image.Height}");
            Console.WriteLine($"Pixel Format: {image.PixelFormat}");

            int framePixels = 15;
            if (image.Width <= framePixels || image.Height <= framePixels)
            {
                framePixels = 0;
            }

            Random random = new Random();

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < framePixels; j++)
                {
                    image.SetPixel(i, j, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                    image.SetPixel(i, image.Height - j - 1, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                }
            }

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < framePixels; j++)
                {
                    image.SetPixel(j, i, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                    image.SetPixel(image.Width - j - 1, i, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                }
            }

            image.Save("FramedImage.bmp", ImageFormat.Bmp);
        }
    }
}
