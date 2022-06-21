using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.Model.Bucket;
using COSXML.CosException;
using COSXML.Model.Tag;
using COSXML.Transfer;



namespace YuSheng.OrchardCore.FileStorage.Tencent
{


    public class CosFileStore : IFileStore
    {
        private const string _directoryMarkerFileName = "OrchardCore.Media.txt";

        private readonly CosStorageOptions _options;


        private readonly CosXml _cosXml;

        private readonly TransferManager _transferManager;

        private readonly IContentTypeProvider _contentTypeProvider;

        public CosFileStore(CosStorageOptions options, IContentTypeProvider contentTypeProvider)
        {
            _options = options;
            _contentTypeProvider = contentTypeProvider;
            CosXmlConfig config = new CosXmlConfig.Builder()
                .IsHttps(options.IsHttps)  //设置默认 HTTPS 请求
                .SetRegion(options.Region)  //设置一个默认的存储桶地域
                .SetDebugLog(options.IsSetDebugLog)  //显示日志
                .Build();
            long durationSecond = 600;  //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(options.SecretId, options.SecretKey, durationSecond);
            _cosXml = new CosXmlServer(config, cosCredentialProvider);
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();
            // 初始化 TransferManager
            _transferManager = new TransferManager(_cosXml, transferConfig);
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            return Task.Run<IFileStoreEntry>((Func<IFileStoreEntry>)delegate
            {
                try
                {
                    path = path.Replace("//", "/");
                    HeadObjectRequest request = new HeadObjectRequest(_options.BucketName, path);
                    HeadObjectResult result = _cosXml.HeadObject(request);
                    if (result == null)
                    {
                        return null;
                    }
                    return new CosFile(path, result.size, Convert.ToDateTime(result.lastModified));
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    return null;
                }
            });
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            return Task.Run<IFileStoreEntry>((Func<IFileStoreEntry>)delegate
            {
                try
                {
                    if (string.IsNullOrEmpty(path))
                    {

                        //the root folder path
                        path = _options.BasePath + _directoryMarkerFileName;
                    }
                    else if (path.Contains(_directoryMarkerFileName))
                    {
                        //this path is created folder path
                        //do nothing
                    }
                    else if (path.Split("/").FirstOrDefault() == _options.BasePath.Split("/").FirstOrDefault())
                    {
                        //cursor click the folder
                        path += "/" + _directoryMarkerFileName;
                    }
                    else
                    {
                        //created folder finished
                        path = _options.BasePath + path + "/" + _directoryMarkerFileName;
                    }

                    HeadObjectRequest request = new HeadObjectRequest(_options.BucketName, path);
                    HeadObjectResult result = _cosXml.HeadObject(request);
                    if (result == null)
                    {
                        return null;
                    }
                    return new CosDirectory(path, Convert.ToDateTime(result.lastModified));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("404") && (path.Contains("_Users") || path.Contains("mediafields")))
                    {
                        //创建_directoryMarkerFileName文件
                        string objectContent = "this is text file tag";
                        byte[] binaryData = Encoding.ASCII.GetBytes(objectContent);
                        PutObjectRequest putObjectRequest = new PutObjectRequest(_options.BucketName, path, binaryData);
                        var result = _cosXml.PutObject(putObjectRequest);
                        if (result.IsSuccessful())
                        {
                            return new CosDirectory(path, DateTime.Now);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
              
                }
            });
        }

      
        public IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = "", bool includeSubDirectories = false)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = _options.BasePath;
                }
                List<IFileStoreEntry> list = new List<IFileStoreEntry>();
                List<CosFile> list2 = new List<CosFile>();
                List<CosDirectory> list3 = new List<CosDirectory>();
                ListBucket tempListBucket = new ListBucket();
                string marker = string.Empty;
                do
                {
                    GetBucketRequest request = new GetBucketRequest(_options.BucketName);
                    //获取 a/ 下的对象以及子目录
                    request.SetPrefix(path);
                    request.SetDelimiter("/");
                    if (tempListBucket != null)
                    {
                        //上一次拉取数据的下标
                        request.SetMarker(tempListBucket.nextMarker);
                    }

                    //执行请求
                    var result = _cosXml.GetBucket(request);
                    //bucket的相关信息
                    ListBucket info = result.listBucket;
                    tempListBucket = info;
                    // 对象列表
                    List<ListBucket.Contents> objects = info.contentsList;
                    // 子目录列表
                    List<ListBucket.CommonPrefixes> subDirs = info.commonPrefixesList;

                    foreach (var commonPrefix in subDirs)
                    {
                        if (commonPrefix.prefix.LastIndexOf("/") == commonPrefix.prefix.Length - 1)
                        {
                            list3.Add(new CosDirectory(commonPrefix.prefix));
                        }
                    }
                    foreach (var content in objects)
                    {
                        if (content.key.Contains(_directoryMarkerFileName) || content.key.Equals(_options.BasePath)) continue;
                        list2.Add(new CosFile(content.key, content.size, Convert.ToDateTime(content.lastModified)));
                    }
                }
                while (tempListBucket.isTruncated);
                list.AddRange(list3);
                list.AddRange(list2);
                return list.ToAsyncEnumerable();
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get directory content with path '{path}'.", ex);
            }
        }



        public async Task<bool> TryCreateDirectoryAsync(string path)
        {
            return await Task.Run<bool>((Func<bool>)delegate
            {
                try
                {
                    string filePath = ((!path.Contains(_options.BasePath)) ? (_options.BasePath  + path + "/" + _directoryMarkerFileName) : (path + "/" + _directoryMarkerFileName));
                    string objectContent = "this is text file tag";
                    byte[] binaryData = Encoding.ASCII.GetBytes(objectContent);
                    PutObjectRequest putObjectRequest = new PutObjectRequest(_options.BucketName, filePath, binaryData);
                    var result = _cosXml.PutObject(putObjectRequest);
                    return result.IsSuccessful();
                }
                catch (Exception)
                {
                    return false;
                }

            });
        }

        public async Task<bool> TryDeleteFileAsync(string path)
        {
            return await Task.Run<bool>((Func<bool>)delegate
            {
                try
                {
                    DeleteObjectRequest request = new DeleteObjectRequest(_options.BucketName, path);
                    DeleteObjectResult result = _cosXml.DeleteObject(request);
                    return result.IsSuccessful();
                }
                catch (Exception)
                {
                    return false;
                }

            });
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            return await Task.Run<bool>(async () =>
            {
                try
                {
                    //检查一下是否还文件或者目录存在，存在则不能删除
                    var entities = await  GetDirectoryContentAsync(path).ToListAsync();
                    if (entities.Count() > 0) return false;

                    var filePath = path + _directoryMarkerFileName;
                    //删除所有文件，文件夹自动删除
                    DeleteObjectRequest request = new DeleteObjectRequest(_options.BucketName, filePath);
                    DeleteObjectResult result = _cosXml.DeleteObject(request);
                    return result.IsSuccessful();
                }
                catch (Exception)
                {
                    return false;
                }

            });
        }

        public async Task MoveFileAsync(string oldPath, string newPath)
        {
            await Task.Run<bool>(async () =>
            {
                bool re = false;
                try
                {
                    //构造源对象属性
                    CopySourceStruct copySource = new CopySourceStruct(_options.Appid, _options.BucketName, _options.Region, oldPath);
                    COSXMLCopyTask copyTask = new COSXMLCopyTask(_options.BucketName, newPath, copySource);
                    // 拷贝对象
                    COSXMLCopyTask.CopyTaskResult result = await _transferManager.CopyAsync(copyTask);
                    Console.WriteLine(result.GetResultInfo());
                    // 删除对象
                    DeleteObjectRequest request = new DeleteObjectRequest(_options.BucketName, oldPath);
                    DeleteObjectResult deleteResult = _cosXml.DeleteObject(request);

                    re = deleteResult.IsSuccessful();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
                return re;
            });
        }

        public async Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            CosFile ossFile = fileStoreEntry as CosFile;
            if (ossFile == null || ossFile.Path == null)
            {
                throw new FileStoreException("Cannot get file stream because the file does not exist.");
            }
            return await GetFileStreamAsync(ossFile.Path);
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            return await Task.Run<Stream>((Func<Stream>)delegate
            {
                GetObjectBytesRequest request = new GetObjectBytesRequest(_options.BucketName, path);
                GetObjectBytesResult result = _cosXml.GetObject(request);
                Stream stream = new MemoryStream(result.content);
                return stream;
            });
        }

        public async Task CopyFileAsync(string oldPath, string newPath)
        {
            await Task.Run<bool>(async () =>
            {
                bool re = false;
                try
                {
                    //构造源对象属性
                    CopySourceStruct copySource = new CopySourceStruct(_options.Appid, _options.BucketName, _options.Region, oldPath);
                    COSXMLCopyTask copyTask = new COSXMLCopyTask(_options.BucketName, newPath, copySource);
                    // 拷贝对象
                    COSXMLCopyTask.CopyTaskResult result = await _transferManager.CopyAsync(copyTask);

                    re = result.IsSuccessful();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
                return re;
            });
        }

        public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite)
        {
            if (path.Split("/").FirstOrDefault() != _options.BasePath.Split("/").FirstOrDefault()) return Task.FromResult(string.Empty);
            if (path.Contains(_directoryMarkerFileName))
            {
                path = path.Remove(path.IndexOf(_directoryMarkerFileName)-1, _directoryMarkerFileName.Length+1);
            }
            //stream to bytes
            byte[] bytes = new byte[inputStream.Length];
            inputStream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始 
            inputStream.Seek(0, SeekOrigin.Begin);
            PutObjectRequest putObjectRequest = new PutObjectRequest(_options.BucketName, path, bytes);
            return Task.Run((Func<string>)delegate
            {
                var objectResult = _cosXml.PutObject(putObjectRequest);
                return objectResult.IsSuccessful() ? path : string.Empty;
            });
        }
    }
}

