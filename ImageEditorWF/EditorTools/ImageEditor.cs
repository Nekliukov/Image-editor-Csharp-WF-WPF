using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace EditorTools
{
    public static class ImageEditor
    {
        #region Constants
        public const int BRIGHTNESS_MIN = -100, BRIGHTNESS_MAX = 100,
                          CONTRAST_MIN = -100, CONTRAST_MAX = 100;
        public const string IMAGE_EXTENSIONS_PATTERN = "Image files (*.jpg, *.jpeg, *.jpe," +
            " *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        #endregion

        public static Image initialImage = null, loadedImage = null;
        // For getting right location after image scrathing
        public static float widthMultiplier, heightMultiplier;

        //Fields for drawing
        public static PointF lastPoint;
        public static bool isDrawing = false, isMouseDown = false;
        public static int brushThickness = 0;
        public static Color brushColor = Color.Black;

        public static Bitmap AdjustBrightness(Bitmap Image, int threshold)
        {
            Bitmap TempBitmap = Image;
            float FinalValue = (float)threshold / 255.0f;
            Bitmap NewBitmap = new Bitmap(TempBitmap.Width, TempBitmap.Height);
            Graphics NewGraphics = Graphics.FromImage(NewBitmap);
            float[][] FloatColorMatrix ={
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
                };

            ColorMatrix NewColorMatrix = new ColorMatrix(FloatColorMatrix);
            ImageAttributes Attributes = new ImageAttributes();
            Attributes.SetColorMatrix(NewColorMatrix);
            NewGraphics.DrawImage(TempBitmap, new Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height),
                0, 0, TempBitmap.Width, TempBitmap.Height, GraphicsUnit.Pixel, Attributes);
            Attributes.Dispose();
            NewGraphics.Dispose();
            return NewBitmap;
        }

        public static Bitmap Contrast(this Bitmap sourceBitmap, int threshold)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                        sourceBitmap.Width, sourceBitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
            double contrastLevel = Math.Pow((100.0 + threshold) / 100.0, 2);
            double blue = 0, green = 0, red = 0;

            for (int k = 0; k + 4 < pixelBuffer.Length; k += 4)
            {
                blue = ((((pixelBuffer[k] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                green = ((((pixelBuffer[k + 1] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                red = ((((pixelBuffer[k + 2] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;

                blue = (blue > 255) ? 255 : ((blue < 0) ? 0 : blue);
                green = (green > 255) ? 255 : ((green < 0) ? 0: green);
                red = (red > 255) ? 255 : ((red < 0) ? 0 : red);

                pixelBuffer[k] = (byte)blue;
                pixelBuffer[k + 1] = (byte)green;
                pixelBuffer[k + 2] = (byte)red;
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                        resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        public static Image Rotate90(Image img)
        {
            var bmp = new Bitmap(img);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return bmp;
        }

    }
}
