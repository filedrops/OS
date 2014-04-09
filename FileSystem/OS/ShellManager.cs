using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Org.Filedrops.FileSystem.OS
{
	public class ShellManager
	{

		public static Icon GetIcon(string path, ItemType type, IconSize size, ItemState state)
		{
			var flags = (uint)(Interop.SHGFI_ICON | Interop.SHGFI_USEFILEATTRIBUTES);
			var attribute = (uint)(object.Equals(type, ItemType.Folder) ? Interop.FILE_ATTRIBUTE_DIRECTORY : Interop.FILE_ATTRIBUTE_FILE);
			if (object.Equals(type, ItemType.Folder) && object.Equals(state, ItemState.Open))
			{
				flags += Interop.SHGFI_OPENICON;
			}

			//if (object.Equals(size, IconSize.Small))
			//{
			//	flags += Interop.SHGFI_SMALLICON;
			//}
			//else
			//{
			//	flags += Interop.SHGFI_LARGEICON;
			//}
			flags += Interop.SHIL_EXTRALARGE;

			var shfi = new SHFileInfo();
			var res = Interop.SHGetFileInfo(path, attribute, out shfi, (uint)Marshal.SizeOf(shfi), flags);
			if (object.Equals(res, IntPtr.Zero)) throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			try
			{
				Icon.FromHandle(shfi.hIcon);
				return (Icon)Icon.FromHandle(shfi.hIcon).Clone();
			}
			catch
			{
				throw;
			}
			finally
			{
				Interop.DestroyIcon(shfi.hIcon);
			}
		}
	}



	public static class FolderManager
	{
		public static BitmapImage getBitmapImage(string directory, ItemState folderType, int size)
		{
			BitmapSource source = GetImageSource(directory, folderType, size) as BitmapSource;

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			MemoryStream memoryStream = new MemoryStream();
			BitmapImage bImg = new BitmapImage();

			encoder.Frames.Add(BitmapFrame.Create(source));
			encoder.Save(memoryStream);

			bImg.BeginInit();
			bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
			bImg.EndInit();

			memoryStream.Close();

			return bImg;
		}


		public static ImageSource GetImageSource(string directory, ItemState folderType, int size)
		{
			try
			{
				return FolderManager.GetImageSource(directory, new System.Drawing.Size(size, size), folderType);
			}
			catch
			{
				throw;
			}
		}

		public static ImageSource GetImageSource(string directory, System.Drawing.Size size, ItemState folderType)
		{
			try
			{
				using (var icon = ShellManager.GetIcon(directory, ItemType.Folder, IconSize.Large, folderType))
				{
					return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)size.Width, (int)size.Height));
				}
			}
			catch
			{
				throw;
			}
		}
	}

	public static class FileManager
	{

		public static BitmapImage getBitmapImage(string file, int size)
		{
			BitmapSource source = GetImageSource(file, size) as BitmapSource;

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			MemoryStream memoryStream = new MemoryStream();
			BitmapImage bImg = new BitmapImage();

			encoder.Frames.Add(BitmapFrame.Create(source));
			encoder.Save(memoryStream);

			bImg.BeginInit();
			bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
			bImg.EndInit();

			memoryStream.Close();

			return bImg;
		}

		public static ImageSource GetImageSource(string filename, int size)
		{
			try
			{
				return FileManager.GetImageSource(filename, new System.Drawing.Size(size, size));
			}
			catch
			{
				throw;
			}
		}

		public static ImageSource GetImageSource(string filename, System.Drawing.Size size)
		{
			try
			{
				//using (var icon = ShellManager.GetIcon(Path.GetExtension(filename), ItemType.File, IconSize.Large, ItemState.Undefined))
				using (var icon = Icon.ExtractAssociatedIcon(filename))
				{
					return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)size.Width, (int)size.Height));
				}
			}
			catch
			{
				throw;
			}
		}
	}



	public enum ItemType : short
	{
		File,
		Folder
	}

	public enum IconSize : short
	{
		Small,
		Large
	}

	public enum ItemState : short
	{
		Undefined,
		Open,
		Close
	}
}
