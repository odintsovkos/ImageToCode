using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace ImageToCode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private BitmapImage image = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image files (*.jpg;*.gif;*.bmp;*.png)|*.jpg;*.gif;*.bmp;*.png|JPG (*.jpg)|*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif|BMP (*.bmp)|*.bmp"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                image = new BitmapImage(new Uri(openFileDialog.FileName));
                imageBox.Source = image;
                btnSaveCodeToFile.IsEnabled = true;

                LblsWidthAndHeightInit();
            }
        }

        private void btnSaveCodeToFile_Click(object sender, RoutedEventArgs e)
        {
            string hexCode = ConvertImage();
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Header file (*.h)|*.h"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                using FileStream file = File.Create(saveFileDialog.FileName);
                AddText(file, hexCode);

            }
        }

        private void LblsWidthAndHeightInit()
        {
            label.Visibility = Visibility.Visible;
            string width = "Width:  " + image.PixelWidth.ToString();
            string height = "Height: " + image.PixelHeight.ToString();
            label.Content = width + "\n" + height;
        }

        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        private string ConvertImage()
        {
            int height = image.PixelHeight;
            int width = image.PixelWidth;
            int nStride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixelByteArray = new byte[image.PixelHeight * nStride];
            image.CopyPixels(pixelByteArray, nStride, 0);

            int color565 = 0;
            string result = $"const uint16_t test_img_{width}x{height}[][{height}] = " + "{\n";
            int r, g, b = 0;
            int count = 0;
            for (int i = 0; i < pixelByteArray.Length; i += 4)
            {
                count++;
                if (count == 1)
                {
                    result += "\t{";
                }
                
                b = pixelByteArray[i];
                g = pixelByteArray[i + 1];
                r = pixelByteArray[i + 2];
                color565 = ((r & 0xF8) << 8) | ((g & 0xFC) << 3) | ((b & 0xF8) >> 3);
                color565 = ((color565 & 0xFF00) >> 8) | ((color565 & 0xFF) << 8);
                result += $"0x{color565:X4},";

                if (count == width)
                {
                    result += "},\n";
                    count = 0;
                }
            }  
            result  += "};\n";
            return result;
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {

        }
    }
}
