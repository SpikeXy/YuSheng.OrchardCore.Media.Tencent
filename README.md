# YuSheng.OrchardCore.Media.Tencent
tencent cloud cos orchardcore plugin


appsettings.json config demo :

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "OrchardCore": {
    "OrchardCore.Media.Tencent": {
      "BucketName": "xxx-xxx",
      "BasePath": "xxxxxx", //the folder in the bucket,must contains '/', like 'myphoto/'
      "Appid": "xxxx", // tencent cloud appid
      "IsHttps": false,
      "Region": "xxxx", // tencent cloud bucket region
      "IsSetDebugLog": false,
      "SecretId": "xxxx", // tecent cloud secreId
      "SecretKey": "xxxx" // tencent cloud secreKey
    }
  }
}
```
