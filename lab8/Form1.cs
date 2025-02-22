using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace lab8
{
    public partial class Form1 : Form
    {
        private PCXIMAGE pcxImage;

        public Form1()
        {
            InitializeComponent();
            LoadPCX("input\\CAT256.PCX");
            this.Width = pcxImage.width;
            this.Height = pcxImage.height;
        }

        private void LoadPCX(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                pcxImage = new PCXIMAGE(fs);
                Console.WriteLine(pcxImage.info());
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (pcxImage != null)
            {
                using (Graphics g = e.Graphics)
                {
                    for (int i = 0; i < pcxImage.width; i++)
                    {
                        for (int j = 0; j < pcxImage.height; j++)
                        {
                            RGB color = pcxImage.img_arr[j * pcxImage.header.bytes_per_line + i];
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(color.r, color.g, color.b)))
                            {
                                g.FillRectangle(brush, i, j, 1, 1);
                            }
                        }
                    }
                }
            }
        }
    }

    public class RGB
    {
        public byte r;
        public byte g;
        public byte b;

        public RGB(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public override string ToString()
        {
            return $"R{r}  G{g}  B{b}";
        }
    }

    public class PCXIMAGE
    {
        public PCXFILEHEADER header;
        public int width;
        public int height;
        public List<RGB> colormap;
        public List<RGB> img_arr;

        public PCXIMAGE(Stream pcxFile)
        {
            header = new PCXFILEHEADER(pcxFile);
            width = header.x_max + 1;
            height = header.y_max + 1;

            long byteBeaconNum = pcxFile.Length - 769;
            pcxFile.Seek(-769, SeekOrigin.End);
            byte byteBeaconValue = (byte)pcxFile.ReadByte();
            bool byteBeacon = (byteBeaconValue == 0x0C || byteBeaconValue == 0x0A);
            pcxFile.Seek(-768, SeekOrigin.End);
            byteBeaconValue = (byte)pcxFile.ReadByte();
            byteBeacon = (byteBeaconValue == 0x0C || byteBeaconValue == 0x0A);

            if (byteBeacon)
            {
                colormap = new List<RGB>();
                pcxFile.Seek(-768, SeekOrigin.End);
                for (int i = 0; i < 256; i++)
                {
                    colormap.Add(ReadRGB(pcxFile));
                }
            }
            else
            {
                colormap = header.colormap;
            }

            img_arr = new List<RGB>();
            pcxFile.Seek(128, SeekOrigin.Begin);

            byte[] byteArr = new byte[1];

            while (pcxFile.Read(byteArr, 0, 1) > 0)
            {
                byte _byte = byteArr[0];
                if (byteBeacon && pcxFile.Position > byteBeaconNum)
                {
                    break;
                }

                if ((_byte >> 6) == 0b11)
                {
                    int counter = _byte & 0x3F;
                    if (pcxFile.Read(byteArr, 0, 1) > 0)
                    {
                        _byte = byteArr[0];
                        for (int i = 0; i < counter; i++)
                        {
                            img_arr.Add(colormap[_byte]);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    img_arr.Add(colormap[_byte]);
                }
            }

            Console.WriteLine($"img_arr.Count: {img_arr.Count}, width * height: {width * height}, width {width}, height {height}");
        }

        public string info()
        {
            return header.info();
        }

        private byte ReadBYTE(Stream binaryFile)
        {
            return (byte)binaryFile.ReadByte();
        }

        private ushort ReadWORD(Stream binaryFile)
        {
            byte[] buffer = new byte[2];
            binaryFile.Read(buffer, 0, 2);
            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        private RGB ReadRGB(Stream binaryFile)
        {
            return new RGB(ReadBYTE(binaryFile), ReadBYTE(binaryFile), ReadBYTE(binaryFile));
        }
    }

    public class PCXFILEHEADER
    {
        public byte manufacturer;
        public byte version;
        public byte encoding;
        public byte bits_per_pixel;
        public ushort x_min;
        public ushort y_min;
        public ushort x_max;
        public ushort y_max;
        public ushort hdpi;
        public ushort vdpi;
        public List<RGB> colormap;
        public byte reserved;
        public byte num_planes;
        public ushort bytes_per_line;
        public ushort palette_info;
        public ushort h_screen_size;
        public ushort v_screen_size;
        public byte[] filler = new byte[54];

        public PCXFILEHEADER(Stream pcxFile)
        {
            manufacturer = ReadBYTE(pcxFile);
            version = ReadBYTE(pcxFile);
            encoding = ReadBYTE(pcxFile);
            bits_per_pixel = ReadBYTE(pcxFile);
            x_min = ReadWORD(pcxFile);
            y_min = ReadWORD(pcxFile);
            x_max = ReadWORD(pcxFile);
            y_max = ReadWORD(pcxFile);
            hdpi = ReadWORD(pcxFile);
            vdpi = ReadWORD(pcxFile);

            colormap = new List<RGB>();
            for (int i = 0; i < 16; i++)
            {
                colormap.Add(ReadRGB(pcxFile));
            }

            reserved = ReadBYTE(pcxFile);
            num_planes = ReadBYTE(pcxFile);
            bytes_per_line = ReadWORD(pcxFile);
            palette_info = ReadWORD(pcxFile);
            h_screen_size = ReadWORD(pcxFile);
            v_screen_size = ReadWORD(pcxFile);
            pcxFile.Read(filler, 0, 54);
        }

        public string info()
        {
            string colormapStr = string.Join(", ", colormap.Select(c => c.ToString()));

            return "PCXFILEHEADER\n" +
                   $"  Manufacturer: {manufacturer}\n" +
                   $"  Version: {version}\n" +
                   $"  Encoding: {encoding}\n" +
                   $"  BitsPerPixel: {bits_per_pixel}\n" +
                   $"  Xmin: {x_min}\n" +
                   $"  Ymin: {y_min}\n" +
                   $"  Xmax: {x_max}\n" +
                   $"  Ymax: {y_max}\n" +
                   $"  HDpi: {hdpi}\n" +
                   $"  VDpi: {vdpi}\n" +
                   $"  Colormap: [{colormapStr}]\n" +
                   $"  Reserved: {reserved}\n" +
                   $"  NPlanes: {num_planes}\n" +
                   $"  BytesPerLine: {bytes_per_line}\n" +
                   $"  PaletteInfo: {palette_info}\n" +
                   $"  HscreenSize: {h_screen_size}\n" +
                   $"  VscreenSize: {v_screen_size}\n" +
                   $"  Filler: {string.Join(", ", filler)}";
        }

        private byte ReadBYTE(Stream binaryFile)
        {
            return (byte)binaryFile.ReadByte();
        }

        private ushort ReadWORD(Stream binaryFile)
        {
            byte[] buffer = new byte[2];
            binaryFile.Read(buffer, 0, 2);
            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        private RGB ReadRGB(Stream binaryFile)
        {
            return new RGB(ReadBYTE(binaryFile), ReadBYTE(binaryFile), ReadBYTE(binaryFile));
        }

    }
}