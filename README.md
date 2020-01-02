Dilbert
=======

[![Build status](https://ci.appveyor.com/api/projects/status/3v66mx6u9v4dp8dk/branch/master?svg=true)](https://ci.appveyor.com/project/oriches/dilbert/branch/master)

A .Net class library to access the Daily Dilbert cartoon.

Supported versions:

    .Net Framework v4.8 and higher,
    .Net Core v3.0 and higher,


This class library makes use of the 'async & await' keywords available in the standard install of .Net 4.5 and by the inclusion of the <a href="https://www.nuget.org/packages/Microsoft.Bcl.Async">Microsoft.Bcl.Async</a> NuGet package for .Net 4.0, this means .Net 4.0 applications wanting to use this will have to have the Microsoft Hotfix <a href="http://support.microsoft.com/kb/2468871">KB2468871</a> installed.

This library is available as a nuget [package] (https://www.nuget.org/packages/Daily.Dilbert.Service/).

You can get the daily cartoon as either a file written to the local file system temporary folder or as  a .Net stream, the interface for the service is as follows:
```C#
public interface IDailyDilbertService
{
    Task<string> DailyAsFileAsync();

    Task<Stream> DailyAsStreamAsync();
}
```
There is a test project with a couple of tests, but a simple example of how to use this is shown below:

```C#
public async void Dilbert()
{
    var service = new DailyDilbertService();

    var filePath = await service.DailyAsFileAsync();   

    // Load the image as required....
}
```
