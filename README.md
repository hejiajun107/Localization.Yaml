# Localization.Yaml

## Intro

dotnet core yaml file based localization refer to the repository

[https://github.com/WeihanLi/WeihanLi.Extensions.Localization.Json]

support:

* [x] asp.net core
* [x] blazor server
* [x] blazor wasm (embeded resource)
* [x] blazor auto (embeded resource)

## GetStarted

### Install
```
dotnet add package Localization.Yaml --version 8.0.0
```

register required services:

```csharp
services.AddYamlLocalization(options =>
    {
        options.ResourcesPath = "Resource";
        options.ResourcesPathType = ResourcesPathType.TypeBased; // by default, looking for resourceFile like Microsoft do
        // options.ResourcesPathType = ResourcesPathType.CultureBased; // looking for resource file in culture sub dir see details follows
        // options.BuildType = ResourceBuildType.Embeded;  //support embeded resource alse can be used in blazor wasm, ResourceBuildType.FileSystem by default
        // options.ResourceAssemblyType = typeof(Resource);   //will allocate the assembly of embeded file,must be defined when use ResourceBuildType.Embeded
    });
```

middleware config(the same with before):

```csharp
app.UseRequestLocalization();
```

### FileSystem

Resource.zh-CN.yaml

```yaml
Hello : 你好
```

### EmbededResource

Resource_zh-CN.yaml

```yaml
Hello : 你好
```
