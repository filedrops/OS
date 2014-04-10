using Org.Filedrops.FileSystem.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;
using Org.Filedrops.Utilities;

namespace Org.Filedrops.FileSystem.OS
{
    [Serializable()]
    class FiledropsOSDirectory : FiledropsDirectory
    {
        FiledropsDirectory parent = null;
        public override FiledropsDirectory Parent
        {
            get
            {
                if (parent == null)
                {
                    FileInfo fileInfo = new FileInfo(FullName);
                    parent = new FiledropsOSDirectory(FileSystem, fileInfo.DirectoryName);
                }
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        public FiledropsOSDirectory(FiledropsFileSystem fileSystem,
                                    string fullName, FiledropsDirectory parent)
            : base(fileSystem, fullName, parent) { }

        public FiledropsOSDirectory(FiledropsFileSystem fileSystem, string fullName)
            : this(fileSystem, fullName, null) { }

        /// <summary>
        /// </summary>
		public override List<FiledropsFile> GetFiles(SearchOption sOption = SearchOption.TopDirectoryOnly)
        {
            return RecursivelyGetFiles(FullName, sOption);
        }

        /// <summary>
        /// Auxiliary recursive method.
        /// </summary>
		List<FiledropsFile> RecursivelyGetFiles(string folder, SearchOption sOption = SearchOption.TopDirectoryOnly)
        {
            List<FiledropsFile> files = new List<FiledropsFile>();

            try
            {
                foreach (string path in Directory.GetFiles(folder))
                {
                    FiledropsFile file = FileSystem.ConstructFile(path);
                    FileInfo fileInfo = new FileInfo(FullName);
                    file.LastModified = File.GetLastWriteTime(path);
                    file.AddMetaData("fileName", fileInfo.DirectoryName);
                    file.AddMetaData("lastAccessed", File.GetLastAccessTime(path).ToString());
                    file.AddMetaData("modifiedDate", File.GetLastWriteTime(path).ToString());
                    files.Add(file);
                }
            }
            catch (IOException e)
            {
                throw new DirectoryNotFoundException("Please verify that " + folder + " is a valid directory.", e);
            }

			if (sOption == SearchOption.AllDirectories)
				foreach (string dir in Directory.GetDirectories(folder))
					files.AddRange(RecursivelyGetFiles(dir, sOption));

            return files;
        }

		public override List<FiledropsFileSystemEntry> GetEntries(SearchOption sOption = SearchOption.TopDirectoryOnly)
        {
			List<FiledropsFileSystemEntry> entries = new List<FiledropsFileSystemEntry>();
			entries.AddRange(RecursivelyGetFiles(FullName, sOption));
			entries.AddRange(GetDirectories(FullName, sOption));
			return entries;
        }

		public override List<FiledropsDirectory> GetDirectories(SearchOption sOption = SearchOption.TopDirectoryOnly)
        {
			return GetDirectories(FullName, sOption);
        }

		private List<FiledropsDirectory> GetDirectories(string folder, SearchOption sOption = SearchOption.TopDirectoryOnly)
		{
			List<FiledropsDirectory> dirs = new List<FiledropsDirectory>();
            FiledropsDirectory parent = new FiledropsOSDirectory(FileSystem, folder);

			foreach (string path in Directory.GetDirectories(folder))
			{
                dirs.Add(new FiledropsOSDirectory(FileSystem, path, parent));
			}

			if (sOption == SearchOption.AllDirectories)
				foreach (string dir in Directory.GetDirectories(folder))
					dirs.AddRange(GetDirectories(dir, sOption));

			return dirs;
		}

        public override void Create()
        {
            if (Directory.Exists(FullName))
                throw new DirectoryAlreadyExistsException(FullName);
            Directory.CreateDirectory(FullName).Create();
        }

        public override void Delete(bool recursive = false)
        {
            //Directory.Delete(FullName, recursive);
            const int nrAttempts = 10;
            for (var attempt = 1; attempt <= nrAttempts; attempt++)
            {
                try
                {
                    Directory.Delete(FullName, recursive);
                }
                catch (DirectoryNotFoundException)
                {
                    return;
                }
                catch (IOException)
                { // System.IO.IOException: The directory is not empty
                    Thread.Sleep(50);
                    continue;
                }
                return;
            }
        }

		public override List<FiledropsFile> GetFiles(string filter, SearchOption sOption = SearchOption.TopDirectoryOnly)
        {
			filter = filter.Replace('\\', '/');
			List<FiledropsFile> files = RecursivelyGetFiles(FullName, sOption);
			List<FiledropsFile> brol =  new List<FiledropsFile>(from f in files where Regex.Match(f.FullName, filter).Success select f);
			return brol;
        }

        public override void Rename(string name)
        {
            if (name == Path.GetFileName(FullName)) return;
			string newName = Path.Combine(Path.GetDirectoryName(FullName), name);
			Directory.Move(FullName, newName);
			FullName = newName;
        }

        public override bool Exists()
        {
            return Directory.Exists(FullName);
        }

        public override BitmapImage Icon16x16 { get { return getImg(16); } }
        public override BitmapImage Icon24x24 { get { return getImg(24); } }
        public override BitmapImage Icon32x32 { get { return getImg(32); } }
        public override BitmapImage Icon48x48 { get { return getImg(48); } }
        public override BitmapImage Icon64x64 { get { return getImg(64); } }
        public override BitmapImage Icon128x128 { get { return getImg(128); } }
        public override BitmapImage Icon256x256 { get { return getImg(256); } }

        public override BitmapImage OpenIcon16x16 { get { return getImg(16, false); } }
        public override BitmapImage OpenIcon24x24 { get { return getImg(24, false); } }
        public override BitmapImage OpenIcon32x32 { get { return getImg(32, false); } }
        public override BitmapImage OpenIcon48x48 { get { return getImg(48, false); } }
        public override BitmapImage OpenIcon64x64 { get { return getImg(64, false); } }
        public override BitmapImage OpenIcon128x128 { get { return getImg(128, false); } }
        public override BitmapImage OpenIcon256x256 { get { return getImg(256, false); } }

        private BitmapImage getImg(int size, bool isClosed = true)
        {
            BitmapImage img = null;
            if (isClosed)
            {
                if (this.FileSystem.GetClosedDirIcon != null)
                {
                    img = this.FileSystem.GetClosedDirIcon(this, size);
                }
            }
            else
            {
                if (this.FileSystem.GetOpenDirIcon != null)
                {
                    img = this.FileSystem.GetOpenDirIcon(this, size);
                }
            }
            if (img == null)
            {
                try
                {
                    img = FileImageGetter.GetImage(FullName, size);
                }
                catch { }
            }
            return img;
        }
    }
}
