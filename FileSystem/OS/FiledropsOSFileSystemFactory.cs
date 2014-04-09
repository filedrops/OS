using System.Xml;

namespace Org.Filedrops.FileSystem.OS
{
    /// <summary>
    /// Factory for FolderDirectory objects.
    /// </summary>
    public class FiledropsOSFileSystemFactory
    {
        /// <summary>
        /// Create a FolderDirectory object that is represented by a
        /// &lt;data type="folder"&gt;-element.
        /// </summary>
        public FiledropsFileSystem Create(XmlElement elt)
        {
            string root = elt.SelectSingleNode("Directory").ToString(); 
            FiledropsOSFileSystem os = new FiledropsOSFileSystem(root);
            os.Username = elt.SelectSingleNode("Credentials/Username").ToString(); 
            os.Password = elt.SelectSingleNode("Credentials/Password").ToString(); 
            os.Domain = elt.SelectSingleNode("Credentials/Domain").ToString();
            os.UseCredentials = !string.IsNullOrWhiteSpace(os.Username);
            return os;
        }

        public override string ToString()
        {
            return "Folder";
        }
    }
}
