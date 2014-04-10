using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Org.Filedrops.Utilities
{
    static class FileImageGetter
    {
        /// <summary>
        /// Gets the BitmapImage of an icon specified by the path
        /// </summary>
        /// <param name="path">The path to the Icon</param>
        /// <param name="size">The size of the BitmapImage</param>
        /// <returns>The icon as a BitmapImage</returns>
        /// http://msdn.microsoft.com/en-us/library/vstudio/system.drawing.icon.extractassociatedicon
        /// http://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap
        /// http://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        static internal BitmapImage GetImage(string path, int size)
        {
            // Get the icon from the path
            Icon icon = GetIcon(path);

            if (icon != null)
            {
                // convert the icon to a Bitmap
                Bitmap bitmap = icon.ToBitmap();

                // convert the bitmap to a BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(size, size));

                // return the bitmapSource as a BitmapImage
                return BitmapSourceToBitmapImage(bitmapSource);
            }
            else
            {
                // icon = null, so return a new BitmapImage
                return new BitmapImage();
            }
        }

        /// <summary>
        /// Converts the given BitmapSource to a BitmapImage
        /// </summary>
        /// <param name="bitmapSource">The BitmapSource to convert</param>
        /// <returns>The BitmapImage converted from the BitmapSource</returns>
        /// http://stackoverflow.com/questions/5338253/bitmapsource-to-bitmapimage
        static private BitmapImage BitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        /// <summary>
        /// Gets the icon from a file or directory specified by the string fName
        /// </summary>
        /// <param name="fName">The path to the file</param>
        /// <returns>If the file/directory exists, the icon of the file/directory, else null</returns>
        /// http://www.codeguru.com/csharp/csharp/cs_misc/icons/article.php/c4261/Getting-Associated-Icons-Using-C.htm
        static private Icon GetIcon(string fName)
        {
            if (File.Exists(fName) || Directory.Exists(fName))
            {
                // intitiate the SHFILEINFO
                SHFILEINFO shinfo = new SHFILEINFO();

                // the handle to the system image list
                IntPtr hImgSmall = Win32.SHGetFileInfo(fName, 0, ref shinfo,
                                                   (uint)Marshal.SizeOf(shinfo),
                                                    Win32.SHGFI_ICON |
                                                    Win32.SHGFI_SMALLICON);

                // return the icon
                return Icon.FromHandle(shinfo.hIcon);
            }

            // if we get here, something's wrong: return null
            return null;
        }
    }

    /// <summary>
    /// Needed for the GetIcon method
    /// </summary>
    /// http://www.codeguru.com/csharp/csharp/cs_misc/icons/article.php/c4261/Getting-Associated-Icons-Using-C.htm
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    /// <summary>
    /// Needed for the GetIcon method
    /// </summary>
    /// http://www.codeguru.com/csharp/csharp/cs_misc/icons/article.php/c4261/Getting-Associated-Icons-Using-C.htm
    class Win32
    {
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath,
                                    uint dwFileAttributes,
                                    ref SHFILEINFO psfi,
                                    uint cbSizeFileInfo,
                                    uint uFlags);
    }
}