using Org.Filedrops.FileSystem.Exceptions;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using SystemIconRetriever;

namespace Org.Filedrops.FileSystem.OS
{
    [Serializable()]
    public class FiledropsOSFile : FiledropsFile
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

        public FiledropsOSFile(FiledropsFileSystem fileSystem, string fullName,
                               FiledropsDirectory parent)
            : base(fileSystem, fullName, parent) { }

        public FiledropsOSFile(FiledropsFileSystem fileSystem, string fullName)
            : this(fileSystem, fullName, null) { }

        public override string FullName
        {
            get
            {
                return base.FullName;
            }
            set
            {
                base.FullName = value;
                Parent = new FiledropsOSDirectory(FileSystem, Path.GetDirectoryName(value));
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// If credentials should be used, read the files using an Impersonator.
        /// Otherwise, simply read the given files.
        /// </remarks>
        public override void Read()
        {
            FiledropsOSFileSystem fs = (FiledropsOSFileSystem)FileSystem;
            if (fs.UseCredentials)
                using (new Impersonator(fs.Username, fs.Domain, fs.Password))
                    _Read();
            else
                _Read();
        }

        /// <summary>
        /// </summary>
        void _Read()
        {
            BytesContent = File.ReadAllBytes(FullName);
            AddMetaData("Size", BytesContent.Length.ToString());
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// If credentials should be used, write the files using an Impersonator.
        /// Otherwise, simply write the given files.
        /// </remarks>
        public override void Create(bool createDirectoryPath = true)
        {
            FiledropsOSFileSystem fs = (FiledropsOSFileSystem)FileSystem;
            if (fs.UseCredentials)
                using (new Impersonator(fs.Username, fs.Domain, fs.Password))
                    _Create(createDirectoryPath);
            else
                _Create(createDirectoryPath);
        }

        /// <summary>
        /// </summary>
        void _Create(bool createDirectoryPath = true)
        {
            if (!Directory.Exists(Parent.FullName))
            {
                if (createDirectoryPath)
                {
                    Directory.CreateDirectory(Parent.FullName);
                }
                else throw new DirectoryCreationFailedException(Parent.FullName);
            }

            if (BytesContent == null)
                using (File.Create(FullName)) { }
            else
                File.WriteAllBytes(FullName, BytesContent);
        }

        /// <summary>
        /// </summary>
        public override void Delete()
        {
            File.Delete(FullName);
        }

        public override void Rename(string name)
        {
            if (name == Path.GetFileName(FullName)) return;
			string newName = Path.Combine(Path.GetDirectoryName(FullName), name);
			File.Move(FullName, newName);
			FullName = newName;
        }

        public override bool Exists()
        {
            return File.Exists(FullName);
        }

        public override BitmapImage Icon16x16 { get { return getImg(16); } }
        public override BitmapImage Icon24x24 { get { return getImg(24); } }
        public override BitmapImage Icon32x32 { get { return getImg(32); } }
        public override BitmapImage Icon48x48 { get { return getImg(48); } }
        public override BitmapImage Icon64x64 { get { return getImg(64); } }
        public override BitmapImage Icon128x128 { get { return getImg(128); } }
        public override BitmapImage Icon256x256 { get { return getImg(256); } }

        private BitmapImage getImg(int size)
        {
            BitmapImage img = null;
            if (this.FileSystem.GetFileIcon != null)
            {
                img = this.FileSystem.GetFileIcon(this, size);
            }
            if (img == null)
            {
                img = FileImageGetter.GetImage(FullName, size);
            }
            return img;
        }

    }
}
