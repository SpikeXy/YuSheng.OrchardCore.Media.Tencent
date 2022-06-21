using System;

namespace YuSheng.OrchardCore.FileStorage.Tencent
{
    public abstract class CosStorageOptions
    {
        //设置bucket桶的名称
        public string BucketName
        {
            get;
            set;
        }

        //初始访问路径
        public string BasePath
        {
            get;
            set;
        }

        //设置腾讯云账户的账户标识 APPID
        public string Appid
        {
            get;
            set;
        }

        //是否设置默认HTTPS 请求
        public bool IsHttps
        {
            get;
            set;
        } = true;

        //设置一个默认的存储桶地域
        public string Region
        {
            get;
            set;
        }

        //设置是否显示日志
        public bool IsSetDebugLog
        {
            get;
            set;
        } = false;

        //云 API 密钥 SecretId
        public string SecretId
        {
            get;
            set;
        }

        //云 API 密钥 SecretKey
        public string SecretKey
        {
            get;
            set;
        }
    }
}
