TODO: write more documentation

# skwas.IO
Library with IO classes.

## Requirements

Visual Studio 2015 / C# 6

### skwas.IO namespace

Name | Description
:---- | :----
[UnmanagedStream](src/UnmanagedStream.cs) | Encapsulates an unmanaged [IStream](https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.comtypes.istream.aspx) from the System.Runtime.InteropServices.ComTypes namespace, to provide access from managed code. Supports reading/writing, and is tested with IStreams created via [CreateStreamOnHGlobal](https://msdn.microsoft.com/en-us/library/windows/desktop/aa378980.aspx) and [SHCreateMemStream](https://msdn.microsoft.com/en-us/library/windows/desktop/bb773831.aspx), but it should work with any IStream implementation (for example [StructuredStorage](https://msdn.microsoft.com/en-us/library/windows/desktop/aa380369.aspx)).
[ProgressStream](src/ProgressStream.cs) | Represents a stream that can track progress of the stream position versus the length of the stream. A seperate background task is used to monitor the position of the stream. This class is specifically designed to aid  in implementing a progress indicator in UI applications. Supports reading/writing (latter requires one small setup though, see the code example in the source file).
