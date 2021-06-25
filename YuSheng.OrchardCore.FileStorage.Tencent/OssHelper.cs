using COSXML;
using COSXML.Model.Object;
using System;
using System.Collections.Generic;

namespace YuSheng.OrchardCore.FileStorage.Tencent
{
    public static class OssHelper
    {
        public static void DeleteMultiObjects(CosXml cosXml, string bucketName, List<string> keys)
        {
            DeleteMultiObjectRequest request = new DeleteMultiObjectRequest(bucketName);
            //设置返回结果形式
            request.SetDeleteQuiet(false);
            //对象key
            request.SetObjectKeys(keys);
            //执行请求
            DeleteMultiObjectResult result = cosXml.DeleteMultiObjects(request);
            //返回结果
            var resultInfo = result.GetResultInfo();
        }
    }
}
