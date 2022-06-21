# YuSheng.OrchardCore.Media.Tencent

Tencent cloud cos orchardcore plugin

[![NuGet](https://img.shields.io/nuget/v/YuSheng.OrchardCore.Media.Tencent.svg)](https://www.nuget.org/packages/YuSheng.OrchardCore.Media.Tencent)

## Install Nuget
```
dotnet add package YuSheng.OrchardCore.Media.Tencent --version 0.0.3
```
## appsettings.json config demo

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


