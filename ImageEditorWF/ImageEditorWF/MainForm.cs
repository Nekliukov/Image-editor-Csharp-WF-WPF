using EditorTools;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ImageEditorWF
{
    public partial class MainForm : Form
    {
        #region Constants
        private const int BRIGHTNESS_MIN = -100, BRIGHTNESS_MAX = 100,
                          CONTRAST_MIN = -100, CONTRAST_MAX = 100;
        private const string IMAGE_EXTENSIONS_PATTERN = "Image files (*.jpg, *.jpeg, *.jpe," +
            " *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        #endregion

        #region Private fields

        private Image initialImage = null, loadedImage = null;
        // For getting right location after image scrathing
        private float widthMultiplier, heightMultiplier;

        //Fields for drawing
        private PointF lastPoint;
        private bool isDrawing = false, isMouseDown = false;
        private int brushThickness = 0;
        private Color brushColor = Color.Black;

        #endregion

        public MainForm() => InitializeComponent();

        #region Generated events
        private void button_loadImage_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = IMAGE_EXTENSIONS_PATTERN;
                openFileDialog1.ShowDialog();
                loadedImage = Image.FromFile(openFileDialog1.FileName);
                initialImage = Image.FromFile(openFileDialog1.FileName);
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

            pictureBox_image.Image = initialImage;
            pictureBox_image.SizeMode = PictureBoxSizeMode.StretchImage;
            widthMultiplier = pictureBox_image.Width / (float)pictureBox_image.Image.Width;
            heightMultiplier = pictureBox_image.Height / (float)pictureBox_image.Image.Height;
            tabControl.Enabled = true;
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = IMAGE_EXTENSIONS_PATTERN;
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
            => PerformChanges(initialImage);

        private void trackBar_contrast_Scroll(object sender, EventArgs e)
            => PerformChanges(initialImage);

        private void MainForm_Load(object sender, EventArgs e)
        {
            tabControl.Enabled = false;
            SetTrackPadValues(trackBar_brightness, BRIGHTNESS_MIN, BRIGHTNESS_MAX);
            SetTrackPadValues(trackBar_contrast, CONTRAST_MIN, CONTRAST_MAX);
        }

        private void button_rotate_Click(object sender, EventArgs e)
        {
            if (!IsImageExists())
                return;

            initialImage = ImageEditor.Rotate90(initialImage);
            pictureBox_image.Image = initialImage;
            pictureBox_image.SizeMode = PictureBoxSizeMode.StretchImage;
            PerformChanges(initialImage);
        }

        private void checkBox_drawing_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_drawing.Checked == true)
            {
                pictureBox_image.Cursor = Cursors.Cross;
                isDrawing = true;
            }
            else
            {
                pictureBox_image.Cursor = Cursors.Default;
                isDrawing = false;
            }
        }

        private void pictureBox_image_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && isDrawing)
            {
                using (Graphics g = Graphics.FromImage(pictureBox_image.Image))
                {
                    PointF currLocation = e.Location;
                    currLocation.X /= widthMultiplier;
                    currLocation.Y /= heightMultiplier;
                    g.DrawLine(new Pen(brushColor, brushThickness), lastPoint, currLocation);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                }

                pictureBox_image.Invalidate();
                lastPoint = new PointF(e.Location.X / widthMultiplier, e.Location.Y / heightMultiplier);
                initialImage = pictureBox_image.Image;
            }
        }

        private void trackBar_thickness_Scroll(object sender, EventArgs e)
            => brushThickness = trackBar_thickness.Value;

        private void button_setColor_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            brushColor = colorDialog1.Color;
            label_color.BackColor = brushColor;
        }

        private void pictureBox_image_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            lastPoint.X = 0; lastPoint.Y = 0;
            PerformChanges(initialImage);
        }

        private void pictureBox_image_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = e.Location;
            lastPoint.X /= widthMultiplier;
            lastPoint.Y /= heightMultiplier;
            isMouseDown = true;
        }

        private void button_reset_Click(object sender, EventArgs e)
        {
            pictureBox_image.Image = loadedImage;
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
