using OrchardCore.FileStorage;
using System;


namespace YuSheng.OrchardCore.FileStorage.Tencent
{
    public class CosDirectory : IFileStoreEntry
    {
        private readonly string _path;

        private readonly DateTime _lastModifiedUtc = default(DateTime);

        private readonly string _name;

        private readonly string _directoryPath;

        public string Path => _path;

        public string Name => _name;

        public string DirectoryPath => _directoryPath;

        public long Length => 0L;

        public DateTime LastModifiedUtc => _lastModifiedUtc;

        public bool IsDirectory => true;

        public CosDirectory(string path, DateTime lastModifiedUtc = default(DateTime))
        {
            _path = path;
            _lastModifiedUtc = lastModifiedUtc;
            _name = _path.Split('/')[_path.Split('/').Length - 2];
            _directoryPath = _path.Substring(0, _path.LastIndexOf("/") ) ;
        }
    }
}


