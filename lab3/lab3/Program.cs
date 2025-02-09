using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Bitmap image = new Bitmap("image.bmp"))
            {
                int width = image.Width;
                int height = image.Height;
                Bitmap rotatedImage = new Bitmap(height, width, PixelFormat.Format32bppArgb);

                for(int i = 0; i < width; i++)
                {
                    for(int j = 0; j < height; j++)
                    {
                        Color pixel = image.GetPixel(i, j);
                        rotatedImage.SetPixel(height - 1 - j, i, pixel);
                    }
                }

                rotatedImage.Save("rotatedImage.bmp", ImageFormat.Bmp);
            }
        }
    }
}
