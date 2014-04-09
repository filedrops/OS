using System;
using System.Xml;


namespace Org.Filedrops.FileSystem.OS
{
    [Serializable()]
    public class FiledropsOSFileSystem : FiledropsFileSystem
    {
        /// <summary>
        /// The root of this file system.
        /// </summary>
        public override FiledropsDirectory WorkingDirectory { get; set; }

        /// <summary>
        /// File operations will use impersonation if set to true. Note that
        /// the Username, Password and Domain properties must be set.
        /// </summary>
        public bool UseCredentials { get; set; }

        /// <summary>
        /// Username of the user to impersonate.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password of the user to impersonate.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Domain of the user to impersonate.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Construct a new file system with the given folder as root.
        /// </summary>
        public FiledropsOSFileSystem(string root)
        {
            Setup(root);
        }

        /// <summary>
        /// Construct a new OS file system. The xml contains the needed information
        /// </summary>
        /// <param name="element">The xml element with the information</param>
        public FiledropsOSFileSystem(XmlElement elt)
        {
            string root = elt.SelectSingleNode("Directory").ToString();
            Setup(root);

            Username = elt.SelectSingleNode("Credentials/Username").ToString();
            Password = elt.SelectSingleNode("Credentials/Password").ToString();
            Domain = elt.SelectSingleNode("Credentials/Domain").ToString();
            UseCredentials = !string.IsNullOrWhiteSpace(Username);
        }
        
        /// <summary>
        /// Setup method for the constructors
        /// </summary>
        /// <param name="root">the path to the root folder</param>
        private void Setup(string root)
        {
            WorkingDirectory = new FiledropsOSDirectory(this, root);
            UseCredentials = false;
        }

        public override FiledropsFile ConstructFile(string path)
        {
            return new FiledropsOSFile(this, path);
        }

        public override FiledropsDirectory ConstructDirectory(string path)
        {
            return new FiledropsOSDirectory(this, path);
        }

        public override XmlDocument GetMetaDataDefinition()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return a string representation of this folder.
        /// </summary>
        public override string ToString()
        {
            return "[FOLDER: " + WorkingDirectory.FullName + "]";
        }
    }
}
