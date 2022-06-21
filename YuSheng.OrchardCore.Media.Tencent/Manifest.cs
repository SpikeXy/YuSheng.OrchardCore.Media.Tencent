using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "YuSheng Tencent Media",
    Author = "spike",
    Website = "https://github.com/SpikeXy/YuSheng.OrchardCore.Media.Tencent",
    Version = "0.0.1",
    Category = "YuSheng Media",
    Description = "Enables support for storing media files in Tencent Cos Storage.",
    Dependencies = new[]
    {
        "OrchardCore.Media.Cache"
    }

)]
