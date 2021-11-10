# AppSettingsSourceGenerator
Source generator that turns appsettings.json into ready to be used classes for dependency injection.

## Installation

Use this [NuGet package](https://www.nuget.org/packages/Reodor.AppSettingsSourceGenerator/).
Install using, for example, `dotnet add package Reodor.AppSettingsSourceGenerator --version 1.0.0`

_Note_  
Due to how analyzers need to be packaged when referencing external NuGet packages, your analyzer list in Visual Studio may appear cluttered with other libraries than just analyzers.
For now, this is the only way to make this work.

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

