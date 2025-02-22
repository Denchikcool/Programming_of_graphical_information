using System;
using System.Drawing;
using System.IO;

namespace lab6
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filename = "_сarib_TC";
            string extencion = "bmp";
            string outputDir = Path.Combine(path, "output");

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string imagePath = Path.Combine(path, "input", $"{filename}.{extencion}");
            Console.WriteLine($"Image path: {imagePath}");

            using (Bitmap image = new Bitmap(imagePath))
            {
                string logoFilename = "logo";
                string logoExtencion = "png";

                using (Bitmap logo = new Bitmap(Path.Combine(path, "input", $"{logoFilename}.{logoExtencion}")))
                {
                    if (logo.Height >= image.Height || logo.Width >= image.Width)
                    {
                        throw new InvalidOperationException("Logo must be smaller than image!");
                    }

                    for (double k = 0.1; k <= 0.9; k += 0.1)
                    {
                        using (Bitmap modifiedImage = new Bitmap(image))
                        {
                            for (int row = 0; row < logo.Height; row++)
                            {
                                for (int col = 0; col < logo.Width; col++)
                                {
                                    Color logoPixel = logo.GetPixel(col, row);
                                    if (logoPixel != Color.FromArgb(255, 174, 201))
                                    {
                                        int centerH = (image.Height - logo.Height) / 2;
                                        int centerW = (image.Width - logo.Width) / 2;

                                        Color imagePixel = image.GetPixel(col + centerW, row + centerH);
                                        int r = (int)(imagePixel.R * k + logoPixel.R * (1 - k));
                                        int g = (int)(imagePixel.G * k + logoPixel.G * (1 - k));
                                        int b = (int)(imagePixel.B * k + logoPixel.B * (1 - k));

                                        modifiedImage.SetPixel(col + centerW, row + centerH, Color.FromArgb(r, g, b));
                                    }
                                }
                            }

                            string outputPath = Path.Combine(outputDir, $"{filename}_watermark_k_{k:0.0}.{extencion}");
                            modifiedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Bmp);

                            Console.WriteLine($"Image saved in {outputPath}");
                        }
                    }
                }
            }
        }
    }
}