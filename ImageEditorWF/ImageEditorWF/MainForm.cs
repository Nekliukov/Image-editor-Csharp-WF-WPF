using EditorTools;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ImageEditorWF
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();

        #region Generated events
        private void button_loadImage_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = ImageEditor.IMAGE_EXTENSIONS_PATTERN;
                openFileDialog1.ShowDialog();
                ImageEditor.loadedImage = Image.FromFile(openFileDialog1.FileName);
                ImageEditor.initialImage = Image.FromFile(openFileDialog1.FileName);
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("It's not an image. Look for a .jped,.jpg,.png,.bmp files", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }

            pictureBox_image.Image = ImageEditor.initialImage;
            pictureBox_image.SizeMode = PictureBoxSizeMode.StretchImage;
            ImageEditor.widthMultiplier = pictureBox_image.Width / (float)pictureBox_image.Image.Width;
            ImageEditor.heightMultiplier = pictureBox_image.Height / (float)pictureBox_image.Image.Height;
            tabControl.Enabled = true;
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = ImageEditor.IMAGE_EXTENSIONS_PATTERN;
                saveFileDialog1.ShowDialog();
                pictureBox_image.Image.Save(saveFileDialog1.FileName);
            }
            // In case if we have cancelled saving process
            catch (ArgumentException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Image is not loaded yet", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void trackBar_brightness_Scroll(object sender, EventArgs e)
            => PerformChanges(ImageEditor.initialImage);

        private void trackBar_contrast_Scroll(object sender, EventArgs e)
            => PerformChanges(ImageEditor.initialImage);

        private void MainForm_Load(object sender, EventArgs e)
        {
            tabControl.Enabled = false;
            SetTrackPadValues(trackBar_brightness, ImageEditor.BRIGHTNESS_MIN, ImageEditor.BRIGHTNESS_MAX);
            SetTrackPadValues(trackBar_contrast, ImageEditor.CONTRAST_MIN, ImageEditor.CONTRAST_MAX);
        }

        private void button_rotate_Click(object sender, EventArgs e)
        {
            if (!IsImageExists())
                return;

            ImageEditor.initialImage = ImageEditor.Rotate90(ImageEditor.initialImage);
            pictureBox_image.Image = ImageEditor.initialImage;
            pictureBox_image.SizeMode = PictureBoxSizeMode.StretchImage;
            PerformChanges(ImageEditor.initialImage);
        }

        private void checkBox_drawing_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_drawing.Checked == true)
            {
                pictureBox_image.Cursor = Cursors.Cross;
                ImageEditor.isDrawing = true;
            }
            else
            {
                pictureBox_image.Cursor = Cursors.Default;
                ImageEditor.isDrawing = false;
            }
        }

        private void pictureBox_image_MouseMove(object sender, MouseEventArgs e)
        {
            if (ImageEditor.isMouseDown && ImageEditor.isDrawing)
            {
                using (Graphics g = Graphics.FromImage(pictureBox_image.Image))
                {
                    PointF currLocation = e.Location;
                    currLocation.X /= ImageEditor.widthMultiplier;
                    currLocation.Y /= ImageEditor.heightMultiplier;
                    g.DrawLine(new Pen(ImageEditor.brushColor, ImageEditor.brushThickness), ImageEditor.lastPoint, currLocation);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                }

                pictureBox_image.Invalidate();
                ImageEditor.lastPoint = new PointF(e.Location.X / ImageEditor.widthMultiplier, e.Location.Y / ImageEditor.heightMultiplier);
                ImageEditor.initialImage = pictureBox_image.Image;
            }
        }

        private void trackBar_thickness_Scroll(object sender, EventArgs e)
            => ImageEditor.brushThickness = trackBar_thickness.Value;

        private void button_setColor_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            ImageEditor.brushColor = colorDialog1.Color;
            label_color.BackColor = ImageEditor.brushColor;
        }

        private void pictureBox_image_MouseUp(object sender, MouseEventArgs e)
        {
            ImageEditor.isMouseDown = false;
            ImageEditor.lastPoint.X = 0; ImageEditor.lastPoint.Y = 0;
        }

        private void pictureBox_image_MouseDown(object sender, MouseEventArgs e)
        {
            ImageEditor.lastPoint = e.Location;
            ImageEditor.lastPoint.X /= ImageEditor.widthMultiplier;
            ImageEditor.lastPoint.Y /= ImageEditor.heightMultiplier;
            ImageEditor.isMouseDown = true;
        }

        private void button_reset_Click(object sender, EventArgs e)
        {
            pictureBox_image.Image = ImageEditor.loadedImage;
            trackBar_brightness.Value = 0;
            trackBar_contrast.Value = 0;
        }

        #endregion

        #region Custom private fields

        private void PerformChanges(Image currentImage)
        {
            if (!IsImageExists())
                return;
            
            currentImage = ImageEditor.AdjustBrightness(new Bitmap(currentImage),
                trackBar_brightness.Value);
            currentImage = ImageEditor.Contrast(new Bitmap(currentImage),
                trackBar_contrast.Value);
            pictureBox_image.Image = currentImage;
        }

        private void SetTrackPadValues(TrackBar trackbar, int min, int max)
        {
            trackbar.Maximum = max;
            trackbar.Minimum = min;
            trackbar.Value = (max + min) / 3;
        }

        private bool IsImageExists()
        {
            if (pictureBox_image.Image == null)
            {
                MessageBox.Show("Image is not loaded yet", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion
    }
}
