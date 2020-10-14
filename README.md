# LWMS
LWMS is shot for **L**ight**W**eight **M**anaged **S**erver

## How lightweight LWMS is?
Including `conhost.exe`, it occupies only 12 MB memory on Windows 10 when there's no burden.

## Compatibility

It runs on .Net Core, it should be a cross-platform server. :P

**NOTE:** LWMS uses HttpListener as its backend, if you configured LWMS and your OS correctly, LWMS should be able to handle HTTPS.

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
public class Sample : IManageCommand
    {
        public string CommandName => "Sample";

        public void Invoke(params CommandPack[] args)
        {
            Console.WriteLine("Hello, LWMS!");
        }
    }
```

After build it, copy generated `dll` file to your LWMS installation location. Then, open `ManageModules.ini`, add the absoulte path of that `dll` file to it, then you can type `Sample` to test the manage command you just made.

## Improve Performance

### Ajust Console Output
You can improve performance by disabling console beautification or console output. To do this you can use `LWMS.exe /preboot runtimeconfig /disablebeautifyconsole` or `LWMS.exe /preboot runtimeconfig /disableconsole`.

### Disable writing to log file

You can improve performance by disable writing logs to file. To do this, you can use `runtimeconfig /disablelogtofile` in runtime or `LWMS.exe /preboot runtimeconfig /disablelogtofile` when launching the server.

### Decrease memory usage by downsize buffer.
You can lower memory usage by downsizing buffer. You can edit `BUF_LENGTH` item in Server.ini. Or, you can modify it in runtime by typing `runtimeconfig -buf_length=<an integer that you want>`. Default value is 1MB.

**NOTE: smaller buffer may cause low transmission speed.**