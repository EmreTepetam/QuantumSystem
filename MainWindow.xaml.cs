using System;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Controls; // Image için
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuantumSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Donanım bilgilerini çek ve UI'ya ata
            CpuText.Text = GetCpuName() ?? "Bilinmiyor";
            ScreenText.Text = GetScreenResolution() ?? "Bilinmiyor";
            GpuText.Text = GetGpuName() ?? "Bilinmiyor";
            RamText.Text = GetTotalRam() ?? "Bilinmiyor";
            DiskText.Text = GetDiskSize() ?? "Bilinmiyor";

            // PNG ikonlarını yükle
            LoadPngIcons();

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/QuantumSystem;component/GamingWallpaper.jpg");
            bitmap.EndInit();
            InnerImage.Source = bitmap;
        }

        private void LoadPngIcons()
        {
            string[] pngFiles = { "component/cpu.png", "component/display.png", "component/gpu.png", "component/ram.png", "component/storage.png","component/Windows.png" };
            foreach (var file in pngFiles)
            {
                try
                {
                    Image targetImage = GetImageControl(file);
                    if (targetImage != null)
                    {
                        targetImage.Source = LoadPng(file);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PNG yükleme hatası ({file}): {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }
        }

        private Image GetImageControl(string fileName)
        {
            switch (fileName)
            {
                case "component/cpu.png": return CpuIcon;
                case "component/display.png": return ScreenIcon;
                case "component/gpu.png": return GpuIcon;
                case "component/ram.png": return RamIcon;
                case "component/storage.png": return DiskIcon;
                case "component/Windows.png": return WindowsIcon;
                default: return null;
            }
        }

        private ImageSource LoadPng(string fileName)
        {
            try
            {
                Uri uri = new Uri($"pack://application:,,,/QuantumSystem;component/{fileName}");
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Dosyayı tamamen yükle
                bitmap.UriSource = uri;
                bitmap.EndInit();
                if (bitmap.IsDownloading)
                {
                    MessageBox.Show($"{fileName} yükleniyor, lütfen bekleyin...");
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PNG yükleme hatası ({fileName}): {ex.Message}\nStackTrace: {ex.StackTrace}");
                return null;
            }
        }

        private string GetCpuName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString();
                    }
                }
            }
            catch { }
            return null;
        }

        private string GetScreenResolution()
        {
            try
            {
                return $"{SystemParameters.PrimaryScreenWidth} x {SystemParameters.PrimaryScreenHeight}";
            }
            catch { }
            return null;
        }

        private string GetGpuName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    string gpu = null;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString();
                        if (name != null && (name.Contains("NVIDIA") || name.Contains("AMD") || name.Contains("RTX") || name.Contains("GeForce")))
                        {
                            return name;
                        }
                        gpu = name; // Entegre GPU olarak yedek
                    }
                    return gpu;
                }
            }
            catch { }
            return null;
        }

        private string GetTotalRam()
        {
            try
            {
                long totalRam = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalRam += Convert.ToInt64(obj["Capacity"] ?? 0);
                    }
                }
                return $"{totalRam / 1024 / 1024 / 1024} GB RAM";
            }
            catch { }
            return null;
        }

        private string GetDiskSize()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='C:'"))
                {
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        long size = Convert.ToInt64(disk["Size"] ?? 0);
                        return $"{size / 1024 / 1024 / 1024} GB"; // Byte'dan GB'ye dönüşüm
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Disk boyutu hatası: {ex.Message}");
            }
            return "Bilinmiyor";
        }
    }
}
