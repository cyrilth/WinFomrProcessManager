# WinFomrProcessManager
Testing out dotnet core winform w/ background service &amp; system tray icon

# Create a Release
```
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -o ./publish
```