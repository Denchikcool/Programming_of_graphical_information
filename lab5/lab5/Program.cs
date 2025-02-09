using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace lab5
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap image = new Bitmap("image.bmp");
            Console.WriteLine($"Original: {image.Width}x{image.Height}");
            Console.WriteLine("Enter scale: ");

            float scalingFactor = float.Parse(Console.ReadLine());

            int scaledWidth = (int)Math.Round(image.Width * scalingFactor);
            int scaledHeight = (int)Math.Round(image.Height * scalingFactor);
            Console.WriteLine($"Scaled: {scaledWidth}x{scaledHeight}");

            Bitmap scaledImage = new Bitmap(scaledWidth, scaledHeight);

            for(int i = 0; i < scaledHeight; i++)
            {
                for(int j = 0; j < scaledWidth; j++)
                {
                    int originalX = (int)(j / scalingFactor);
                    int originalY = (int)(i / scalingFactor);
                    Color pixel = image.GetPixel(originalX, originalY);
                    scaledImage.SetPixel(j, i, pixel);
                }
            }

            scaledImage.Save("imageScaled.bmp", ImageFormat.Bmp);
        }
    }
}
