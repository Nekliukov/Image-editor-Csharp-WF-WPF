using EditorTools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageEditorWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GroupBoxOptions.IsEnabled = false;
        }

        ImageSource initImage, currImage;

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofdPicture = new Microsoft.Win32.OpenFileDialog();
            ofdPicture.Filter =
                "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";
            ofdPicture.FilterIndex = 1;

            if (ofdPicture.ShowDialog() == true)
                MainImage.Source =
                    new BitmapImage(new Uri(ofdPicture.FileName));
            currImage = BitmapToImageSource(ConvertToBitmap(MainImage.Source as BitmapSource));
            initImage = BitmapToImageSource(ConvertToBitmap(MainImage.Source as BitmapSource));
            GroupBoxOptions.IsEnabled = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            => PerformChanges();

        private void PerformChanges()
        {
            ImageSource currentImage = BitmapToImageSource(
                ImageEditor.AdjustBrightness(ConvertToBitmap(currImage as BitmapSource),
                    (int)BrightnessSlider.Value)
                );

            currentImage = BitmapToImageSource(
                ImageEditor.Contrast(ConvertToBitmap(currentImage as BitmapSource),
                    (int)ContrastSlider.Value)
                );

            MainImage.Source = currentImage;
        }

        private static Bitmap ConvertToBitmap(BitmapSource bitmapSource)
        {
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var memoryBlockPointer = Marshal.AllocHGlobal(height * stride);
            bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), memoryBlockPointer, height * stride, stride);
            var bitmap = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, memoryBlockPointer);
            return bitmap;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        private ImageSource BitmapToImageSource(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
            => PerformChanges();

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            MainImage.Source = initImage;
            currImage = MainImage.Source;
            BrightnessSlider.Value = 0;
            ContrastSlider.Value = 0;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFD = new Microsoft.Win32.SaveFileDialog();
            saveFD.Filter = ImageEditor.IMAGE_EXTENSIONS_PATTERN;
            saveFD.ShowDialog();
            try
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)MainImage.Source));
                using (FileStream stream = new FileStream(saveFD.FileName, FileMode.Create))
                    encoder.Save(stream);
            }
            // In case if we have cancelled saving process
            catch (ArgumentException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                System.Windows.Forms.MessageBox.Show("Image is not loaded yet", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void GrayScaleButton_Click(object sender, RoutedEventArgs e)
        {
            FormatConvertedBitmap grayBitmap = new FormatConvertedBitmap();
            grayBitmap.BeginInit();
            grayBitmap.Source = MainImage.Source as BitmapSource;
            grayBitmap.DestinationFormat = PixelFormats.Gray8;
            grayBitmap.EndInit();
            MainImage.Source = grayBitmap;
        }

        private void BlurButton_Click(object sender, RoutedEventArgs e)
        {
            BlurBitmapEffect blurEffect = new BlurBitmapEffect();
            blurEffect.Radius = 2;

            // Set the KernelType property of the blur. A KernalType of "Box"
            // creates less blur than the Gaussian kernal type.
            blurEffect.KernelType = KernelType.Box;

            // Apply the bitmap effect to the Button.
#pragma warning disable CS0618 // Type or member is obsolete
            MainImage.BitmapEffect = blurEffect;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            currImage = BitmapToImageSource((Bitmap)ImageEditor.Rotate90
                (ConvertToBitmap(currImage as BitmapSource)));

            MainImage.Source = currImage;
            PerformChanges();
        }
    }
}
