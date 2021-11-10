# AppSettingsSourceGenerator
Source generator that turns appsettings.json into ready to be used classes for dependency injection.

> There is a NuGet-package for this and ideally all you would have to do is to add that to your project and it would automatically detect your `appsettings.json` and generate the files. However, there is a bug with third party dependencies that I haven't figured out yet so it doesn't work right now.

## How it works

Given an `appsettings.json` such as 
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MySettings": {
    "Test": "asdf",
    "Value": 1,
    "Exists": false
  }
}
```

the source generator will create three classes:

- `<YourTopNamespace>.AppSettings.Logging`
- `<YourTopNamespace>.AppSettings.MySettings`
- `<YourTopNamespace>.AppSettings.SimpleSettings`

The resulting code looks like this (using `MySettings` from the above `appsettings.json` as an example)
```csharp
#nullable enable
using System;
namespace DemoApp.AppSettings
{

    public partial class MySettings
    {
        public string Test { get; set; } = default!;
        public int Value { get; set; } = default!;
        public bool Exists { get; set; } = default!;
    }
}
```

The `SimpleSettings` class is a special one as it contains all simple valued settings such as `"AllowedHosts": "*"`.

You can then add these to dependency injection like this:
```csharp
builder.Services.Configure<DemoApp.AppSettings.MySettings>(builder.Configuration.GetSection("MySettings"));
```
