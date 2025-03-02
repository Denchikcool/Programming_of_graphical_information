using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
namespace RGR;

class Program
{
    private const int COLOR_DIF = 100;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct FileHeader
    {
        public ushort id;
        public uint f_size;
        public ushort rez_1, rez_2;
        public uint bm_offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct V3Header
    {
        public uint h_size;
        public uint width;
        public uint height;
        public ushort planes, bit_per_pixel;
        public uint compression;
        public uint size_image;
        public uint h_res;
        public uint v_res;
        public uint clr_used;
        public uint clr_imp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ColorInfo
    {
        public byte blue;
        public byte green;
        public byte red;
        public byte temp;

        public ColorInfo(byte b, byte g, byte r)
        {
            blue = b;
            green = g;
            red = r;
            temp = 0;
        }
    }

    private static int FindNead(byte b, byte g, byte r, List<ColorInfo> arr)
    {
        int result = 0;
        int minVal = int.MaxValue;
        int siz = arr.Count;
        int temp;

        for (int i = 0; i < siz; ++i)
        {
            temp = (b - arr[i].blue) * (b - arr[i].blue) +
                   (g - arr[i].green) * (g - arr[i].green) +
                   (r - arr[i].red) * (r - arr[i].red);

            if (temp < minVal)
            {
                result = i;
                minVal = temp;
            }
        }

        return result;
    }

    public static void ConvertBmp(string inputFile, string outputFile)
    {
        using (FileStream fileIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(fileIn))
        {
            FileHeader header = ReadStruct<FileHeader>(reader);
            V3Header headerV3 = ReadStruct<V3Header>(reader);

            Console.WriteLine(" File header info:\n");
            Console.WriteLine($"id={header.id}, f_size={header.f_size}, r1={header.rez_1}, r2={header.rez_2}, ofset={header.bm_offset}\n \n");

            Console.WriteLine(" v3 header info:\n");
            Console.WriteLine($"size={headerV3.h_size}, width={headerV3.width}, height={headerV3.height}\n");
            Console.WriteLine($"planes={headerV3.planes}, bit_per_pix={headerV3.bit_per_pixel}, compression={headerV3.compression}\n");
            Console.WriteLine($"size_image={headerV3.size_image}, h_res={headerV3.h_res}, v_res={headerV3.v_res}\n");
            Console.WriteLine($"clr_used={headerV3.clr_used}, clr_imp={headerV3.clr_imp}\n \n");

            fileIn.Seek(header.bm_offset, SeekOrigin.Begin);
            int bytesPerRow = (int)Math.Floor((headerV3.bit_per_pixel * headerV3.width + 31) / 32.0) * 4;
            List<byte[]> rows = new List<byte[]>();

            for (int i = 0; i < headerV3.height; ++i)
            {
                byte[] row = reader.ReadBytes(bytesPerRow);
                rows.Add(row);
            }
            Console.WriteLine(" * Start color selection\n");
            List<ColorInfo> colors = new List<ColorInfo>();
            int x = rows[0].Length;
            int y = rows.Count;
            int colorSize = 0;
            bool findEl;

            for (int i = 0; i < y; ++i)
            {
                for (int j = 0; j < x; j += 3)
                {
                    findEl = false;

                    for (int ii = 0; ii < colorSize; ++ii)
                    {
                        int diff = (rows[i][j] - colors[ii].blue) * (rows[i][j] - colors[ii].blue) +
                                   (rows[i][j + 1] - colors[ii].green) * (rows[i][j + 1] - colors[ii].green) +
                                   (rows[i][j + 2] - colors[ii].red) * (rows[i][j + 2] - colors[ii].red);

                        if (diff <= COLOR_DIF)
                        {
                            findEl = true;
                            ColorInfo tempColor = colors[ii];
                            tempColor.temp += 1;
                            colors[ii] = tempColor;
                            break;
                        }
                    }

                    if (!findEl)
                    {
                        ColorInfo newColor = new ColorInfo(rows[i][j], rows[i][j + 1], rows[i][j + 2]);
                        newColor.temp = 1;
                        colors.Add(newColor);
                        ++colorSize;
                    }
                }
            }

            colors = colors.OrderByDescending(c => c.temp).ToList();
            if (colors.Count > 256)
            {
                colors.RemoveRange(256, colors.Count - 256);
            }
            while (colors.Count < 256)
            {
                colors.Add(new ColorInfo(0,0,0));
            }

            Console.WriteLine(" * End color selection\n");

            using (FileStream fileOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fileOut))
            {
                headerV3.bit_per_pixel = 8;
                headerV3.clr_imp = 256;
                headerV3.clr_used = 256;
                headerV3.size_image = (uint)(Math.Floor((headerV3.bit_per_pixel * headerV3.width + 31) / 32.0) * 4 * headerV3.height);
                header.f_size = (uint)(header.bm_offset + headerV3.size_image);
                header.bm_offset = (uint)(Marshal.SizeOf(typeof(FileHeader)) + Marshal.SizeOf(typeof(V3Header)) + 256 * Marshal.SizeOf(typeof(ColorInfo)));

                WriteStruct(writer, header);
                WriteStruct(writer, headerV3);

                foreach (var color in colors)
                {
                    WriteStruct(writer, color);
                }

                Console.WriteLine(" * Start write in file\n");
                bytesPerRow = (int)Math.Floor((headerV3.bit_per_pixel * headerV3.width + 31) / 32.0) * 4;

                for (int i = 0; i < y; ++i)
                {
                    int curByte = 0;
                    for (int j = 0; j < x; j += 3)
                    {
                        byte val = (byte)FindNead(rows[i][j], rows[i][j + 1], rows[i][j + 2], colors);
                        writer.Write(val);
                        ++curByte;
                    }

                    for (int j = curByte; j < bytesPerRow; ++j)
                    {
                        writer.Write((byte)0);
                    }
                }

                Console.WriteLine(" * End write in file\n");
            }
        }
    }

    private static T ReadStruct<T>(BinaryReader reader) where T : struct
    {
        byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T theStruct = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();
        return theStruct;
    }

    private static void WriteStruct<T>(BinaryWriter writer, T obj) where T : struct
    {
        int size = Marshal.SizeOf(obj);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        writer.Write(arr);
    }
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: BmpConverter <input.bmp> <output.bmp>");
            return;
        }

        string inputFile = args[0];
        string outputFile = args[1];

        try
        {
            ConvertBmp(inputFile, outputFile);
            Console.WriteLine("Conversion successful!");
            int uniqueColorCountinput = CountUniqueColors(inputFile);
            Console.WriteLine($"Unique colors in input file: {uniqueColorCountinput}");
            int uniqueColorCountoutput = CountUniqueColors(outputFile);
            Console.WriteLine($"Unique colors in output file: {uniqueColorCountoutput}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static int CountUniqueColors(string bmpFile)
    {
        HashSet<int> uniqueColors = new HashSet<int>();

    using (FileStream fileIn = new FileStream(bmpFile, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(fileIn))
        {
            FileHeader header = ReadStruct<FileHeader>(reader);
            V3Header headerV3 = ReadStruct<V3Header>(reader);

            fileIn.Seek(header.bm_offset, SeekOrigin.Begin);

            int bytesPerRow = (int)Math.Floor((headerV3.bit_per_pixel * headerV3.width + 31) / 32.0) * 4;

            for (int i = 0; i < headerV3.height; ++i)
            {
                byte[] row = reader.ReadBytes(bytesPerRow);

                if (headerV3.bit_per_pixel == 8)
                {
                    for (int j = 0; j < row.Length; ++j)
                    {
                        uniqueColors.Add(row[j]);
                    }
                }
                else if (headerV3.bit_per_pixel == 24)
                {
                    for (int j = 0; j < row.Length; j += 3)
                    {
                        int color = (row[j] << 16) | (row[j + 1] << 8) | row[j + 2];
                        uniqueColors.Add(color);
                    }
                }
            }
        }

        return uniqueColors.Count;
    }
}
