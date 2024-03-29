# LWMS
LWMS is shot for **L**ight**W**eight **M**anaged **S**erver

## How lightweight LWMS is?
Including `conhost.exe`, it occupies only 17 MB memory on Windows 10 when there's no burden (with LWMS.SimpleDirectoryBrowser module loaded).

## Compatibility

It runs on .Net 6, it should be a cross-platform server. :P

**NOTE:** LWMS uses HttpListener as its backend, if you configured LWMS and your OS/.NET correctly, LWMS should be able to handle HTTPS.

## Difference Between LWMS and LWSwnS
LWMS uses pipeline by design while LWSwnS uses event model only. 
In the meanwhile, LWSwnS only support static text file transmission by default if not installing BFT module while LWMS support any file transmission.
Also, LWMS uses HttpListener as its back-end a higher-level class while LWSwmS uses TcpListner which is complex to use.

## Extend your server

### Extend Manage Commands

#### What is manage command?

Manage commands are extendable commands that will be load during the initialization of the server. They can be called from console or in your own ways.

#### Make a manage command

To make your own manage command, you need to create a dotnet library project, refer LWMS.Management library for very basic functions.

In auto created class by `dotnet sdk`, type:

```csharp
public class Sample : LWMS.Management.IManageCommand
    {
        public string CommandName => "Sample";
        public List<string> Alias=>new List<string>();
        public int Version=1;

        public void Invoke(string AuthContext, params LWMS.Management.CommandPack[] args)
        {
            LWMS.Management.Output.WriteLine("Hello, LWMS!",AuthContext);
        }
    }
```

After build it, copy generated `dll` file to your LWMS installation location. Then, open `ManageModules.ini`, add the absoulte path of that `dll` file to it, then you can type `Sample` to test the manage command you just made.

### Extend Pipeline

LWMS allows user to load their own pipeline unit. Currently LWMS handles request and send out stream through pipeline. (Based on `CLUNL.Pipeline`)

#### Pipeline Types

##### R pipeline

**R** stands for requeset, R pipline means this pipeline processes http request.

##### W pipeline

**W** stands for write, W pipeline means this pipeline processes output stream.

#### CmdOut pipeline

**CmdOut** stands for command output, CmdOut pipeline means this pipeline processes output of a manage command.

#### Build your own pipeline unit

 To build up your own unit, please look into `LWMS.Core\DefaultStaicFileUnit.cs` `LWMS.Core\ErrorResponseUnit.cs` for R pipeline, `LWMS.Core\HttpRoutedLayer\DefaultStreamProcessUnit.cs` for W pipeline, `LWMS.Management\ConsoleCmdOutUnit.cs` for CmdOut pipeline.

**Note:** You must refer LWMS.Core.dll to your project, different types of pipeline units can exists in one dll file.

#### Register your pipeline unit

##### Method 1 : Edit configuration file.

You can directly edit configuration file to register your pipeline manually. `RPipelineUnit.tsd` records R pipeline units, `WPipelineUnit.tsd` recrods W pipeline units.

##### Method 2 : Register through manage commands

`ppl reg -w/r/c <dll-file> <type-fullname>`

### Extend HttpEventHandler

Since `v0.5.0.0`, LWMS now support event-like http handler (EventDrivenSupport) through `LWMS.EventDrivenRequests.dll` which is **not** enabled by default.

To build up your own handler, please look into `LWMS.SimpleDirectoryBrowser` and `LWMS.Sample.MarkdownBlogs`, these modules showed usage of the event driven support of LWMS.

## Improve Performance

**Note:** Following steps requires `LWMS.Management.Commands.dll` registered in `ManageModules.ini`.

### Disable Remote Shell

`RemoteShell` is a SSH-Like shell and LWMS uses at least one thread to deal with it, it may effect the performance of your server. This feature is set to off by default. The Key `ISREMOTESHELLENABLED` controls it.

### Adjust Console Output

You can improve performance by disabling console beautification or console output. To do this you can use `LWMS.exe /preboot runtimeconfig /disablebeautifyconsole` or `LWMS.exe /preboot runtimeconfig /disableconsole`.

### Disable writing to log file

You can improve performance by disable writing logs to file. To do this, you can use `runtimeconfig /disablelogtofile` in runtime or `LWMS.exe /preboot runtimeconfig /disablelogtofile` when launching the server.

### Decrease memory usage by downsize buffer.
You can lower memory usage by downsizing buffer. You can edit `BUF_LENGTH` item in Server.ini. Or, you can modify it in runtime by typing `runtimeconfig -buf_length=<an integer that you want>`. Default value is 1MB.

**NOTE: smaller buffer may cause low transmission speed.**