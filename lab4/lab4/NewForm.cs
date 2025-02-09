namespace lab4
{
    public partial class NewForm : Form
    {
        private PictureBox _pictureBox16;
        private PictureBox _pictureBox256;
        private PictureBox _pictureBoxTrueColor;

        public NewForm()
        {
            this.Text = "Initial image";
            //this.Size = new Size(800, 400);
            this.WindowState = FormWindowState.Maximized;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            Image image16 = Image.FromFile("image16.bmp");
            image16 = ResizeImage(image16, image16.Width / 2, image16.Height / 2);
            _pictureBox16 = new PictureBox
            {
                Image = image16,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(10, 10)
            };

            this.Controls.Add(_pictureBox16);

            Image image256 = Image.FromFile("image256.bmp");
            image256 = ResizeImage(image256, image256.Width / 2, image256.Height / 2);
            _pictureBox256 = new PictureBox
            {
                Image = image256,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(_pictureBox16.Right + 10, 10)
            };

            this.Controls.Add(_pictureBox256);

            Image imageTrueColor = Image.FromFile("image.bmp");
            imageTrueColor = ResizeImage(imageTrueColor, imageTrueColor.Width / 2, imageTrueColor.Height / 2);
            _pictureBoxTrueColor = new PictureBox
            {
                Image = imageTrueColor,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(_pictureBox256.Right + 10, 10)
            };

            this.Controls.Add(_pictureBoxTrueColor);
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap newImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return newImage;
        }
    }
}
