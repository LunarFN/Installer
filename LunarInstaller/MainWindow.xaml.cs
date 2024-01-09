using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Principal;
using System.Windows;
using IWshRuntimeLibrary;
using Microsoft.Win32;

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
            if (!IsRunningAsAdministrator())
            {
                RelaunchAsAdministrator();
            }
            string zipUrl = "https://api.lunarfn.org/assets/files/lunar_files.zip";

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

                // Create shortcut
                string shortcutPath = Path.Combine(desktopFolder, "Lunar.lnk");
                CreateShortcut(executablePathInZip, destinationFolder, shortcutPath);

                // Add Registry entries
                AddRegistryEntries();

                MessageBox.Show("Lunar has been Downloaded and Installed Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void AddRegistryEntries()
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("lunarfn"))
                {
                    if (key != null)
                    {
                        key.SetValue("", "URL:lunarfn");

                        using (RegistryKey subKeyShell = key.CreateSubKey("shell"))
                        {
                            using (RegistryKey subKeyOpen = subKeyShell.CreateSubKey("open"))
                            {
                                using (RegistryKey subKeyCommand = subKeyOpen.CreateSubKey("command"))
                                {
                                    subKeyCommand.SetValue("", @"""C:\Program Files\LunarFN\Lunar.exe"" ""%1""");
                                }
                            }
                        }
                    }
                }

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("lunar"))
                {
                    if (key != null)
                    {
                        key.SetValue("URL Protocol", "");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error, Code: 2", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static bool IsRunningAsAdministrator()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);

            // Check if the current user has administrative privileges
            return currentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void RelaunchAsAdministrator()
        {
            // Get the path to the current executable
            string exePath = Process.GetCurrentProcess().MainModule.FileName;

            // Create a new process start info with the same executable path
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                Verb = "runas" // This is needed to trigger UAC
            };

            try
            {
                // Start the new process (current executable with "runas" verb)
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to relaunch as administrator: " + ex.Message);
            }

            // Note: Avoid calling Environment.Exit(0);
            // Allow the application to exit naturally.
        }

    }
}
