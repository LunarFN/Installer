using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using IWshRuntimeLibrary;

namespace LunarInstaller
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DownloadAndInstallButton_Click(object sender, RoutedEventArgs e)
        {
            string zipUrl = "https://cdn.lunarfn.com/lunar_files.zip";

            string destinationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LunarFN");

            string zipFilePath = Path.Combine(destinationFolder, "lunar_files.zip");

            string executablePathInZip = "Lunar.exe";

            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            try
            {
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(zipUrl, zipFilePath);
                }

                ZipFile.ExtractToDirectory(zipFilePath, destinationFolder);

                string shortcutPath = Path.Combine(desktopFolder, "Lunar.lnk");
                CreateShortcut(executablePathInZip, destinationFolder, shortcutPath);

                MessageBox.Show("Lunar has been Downloaded Succesfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CreateShortcut(string targetPath, string workingDirectory, string shortcutPath)
        {
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = Path.Combine(workingDirectory, targetPath);
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.Description = "LunarFN Shortcut";
            shortcut.Save();
        }
    }
}
