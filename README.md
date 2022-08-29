# Localization.Yaml

## Intro
dotnet core yaml file based localization refer to the repository [https://github.com/WeihanLi/WeihanLi.Extensions.Localization.Json]
## GetStarted

register required services:

``` csharp
services.AddYamlLocalization(options =>
    {
        options.ResourcesPath = "Resource";
        options.ResourcesPathType = ResourcesPathType.TypeBased; // by default, looking for resourceFile like Microsoft do
        // options.ResourcesPathType = ResourcesPathType.CultureBased; // looking for resource file in culture sub dir see details follows
    });
```

middleware config(the same with before):

``` csharp
app.UseRequestLocalization();
```
