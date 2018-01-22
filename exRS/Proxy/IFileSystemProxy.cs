using System.Collections.Generic;
using System.Xml.Linq;

namespace exRS.Proxy
{
    internal interface IFileSystemProxy
    {
        string ReadAllText(string path);

        void WriteAllText(string path, string content);

        void CreateFolder(string path);

        void SaveXElement(XElement element, string path);

        XElement LoadXElement(string path);

        byte[] ReadAllBytes(string path);

        bool FileExists(string path);
    }
}
