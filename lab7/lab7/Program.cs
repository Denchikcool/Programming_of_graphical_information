using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
namespace lab7;

class Program
{
    static void Main(string[] args)
    {
        string path_ = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        string imageName = "_сarib_TC";
        string imageExt = "bmp";

        string secretName = "secret";
        string secretExt = "txt";

        string outputDir = Path.Combine(path_, "output");

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        Bitmap img = new Bitmap(Path.Combine(path_, "input", $"{imageName}.{imageExt}"));

        byte[] textBytes = File.ReadAllBytes(Path.Combine(path_, "input", $"{secretName}.{secretExt}"));

        int headerSize = 54;

        if (textBytes.Length * 8 > img.Width * img.Height * 3)
        {
            Console.WriteLine($"Текстовый файл превышает максимально допустимый размер для данного изображения ({(img.Width * img.Height * 3) / 8} байт)");
            return;
        }

        string textBits = string.Join("", textBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        string textBitsSize = Convert.ToString(textBits.Length, 2).PadLeft(32, '0');
        Console.WriteLine($"Размер закодированного текста: {Convert.ToInt32(textBitsSize, 2)} бит");

        byte[] imgBytes;
        using (MemoryStream ms = new MemoryStream())
        {
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            imgBytes = ms.ToArray();
        }

        for (int i = 0; i < textBitsSize.Length; i++)
        {
            imgBytes[i + headerSize] = (byte)((imgBytes[i + headerSize] & ~1) | (textBitsSize[i] - '0'));
        }

        for (int i = 0; i < textBits.Length; i++)
        {
            imgBytes[i + textBitsSize.Length + headerSize] = (byte)((imgBytes[i + textBitsSize.Length + headerSize] & ~1) | (textBits[i] - '0'));
        }

        using (MemoryStream ms = new MemoryStream(imgBytes))
        {
            Bitmap newImg = new Bitmap(ms);
            newImg.Save(Path.Combine(outputDir, $"{imageName}_steganography.{imageExt}"), System.Drawing.Imaging.ImageFormat.Bmp);
            Console.WriteLine($"Закодированное изображение сохранено в {Path.Combine(outputDir, $"{imageName}_steganography.{imageExt}")}");
        }

        Bitmap stegoImg = new Bitmap(Path.Combine(outputDir, $"{imageName}_steganography.{imageExt}"));
        byte[] stegoImgBytes;
        using (MemoryStream ms = new MemoryStream())
        {
            stegoImg.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            stegoImgBytes = ms.ToArray();
        }

        string extractedTextSizeBits = "";
        for (int i = 0; i < 32; i++)
        {
            extractedTextSizeBits += (stegoImgBytes[i + headerSize] & 1).ToString();
        }
        int extractedTextSize = Convert.ToInt32(extractedTextSizeBits, 2);
        Console.WriteLine($"Размер расшифрованного текста: {extractedTextSize} бит");

        string extractedTextBits = "";
        for (int i = 0; i < extractedTextSize; i++)
        {
            extractedTextBits += (stegoImgBytes[i + 32 + headerSize] & 1).ToString();
        }

        byte[] extractedTextBytes = new byte[extractedTextBits.Length / 8];
        for (int i = 0; i < extractedTextBytes.Length; i++)
        {
            extractedTextBytes[i] = Convert.ToByte(extractedTextBits.Substring(i * 8, 8), 2);
        }
        string extractedText = Encoding.UTF8.GetString(extractedTextBytes);
        Console.WriteLine($"Расшифрованный текст:\n{extractedText}");
    }
}

