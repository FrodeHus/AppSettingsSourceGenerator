# AppSettingsSourceGenerator
Source generator that turns appsettings.json into ready to be used classes for dependency injection.

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

The `SimpleSettings` class is a special one as it contains all simple valued settings such as `"AllowedHosts": "*"`.

You can then add these to dependency injection like this:
```csharp
builder.Services.Configure<DemoApp.AppSettings.MySettings>(builder.Configuration.GetSection("MySettings"));
```
