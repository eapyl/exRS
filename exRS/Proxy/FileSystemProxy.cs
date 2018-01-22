using Common.Logging;
using System.IO;
using System.Xml.Linq;

namespace exRS.Proxy
{
    internal class FileSystemProxy : IFileSystemProxy
    {
        private readonly ILog _log;

        public FileSystemProxy(ILog log) => _log = log;

        public string ReadAllText(string path) => File.ReadAllText(path);

        public void WriteAllText(string path, string content)
        {
            _log.Trace($"{nameof(WriteAllText)} to {path}");
            File.WriteAllText(path, content);
        }

        public void CreateFolder(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
        }

        public void SaveXElement(XElement element, string path) => element.Save(path);

        public XElement LoadXElement(string path) => XElement.Load(path);

        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

        public bool FileExists(string path) => File.Exists(path);
    }
}
