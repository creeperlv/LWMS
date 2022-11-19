$Arch="win-x64"
if($args.Count -gt 0){
$Arch=$args[0]
}
rm ".\LWMS\bin\Release\" -Force -Confirm -Recurse
# Build projects
cd LWMS
dotnet build -c Release -r $Arch
cd ..
#  LWMS.Management.Commands
cd LWMS.Management.Commands
dotnet build -c Release -r $Arch
Copy-Item ".\bin\Release\$Arch\LWMS.Management.Commands.dll" -Destination "..\LWMS\bin\Release\$Arch\LWMS.Management.Commands.dll" -Force
cd ..
#  LWMS.EventDrivenRequests
cd LWMS.EventDrivenRequests
dotnet build -c Release -r $Arch
Copy-Item ".\bin\Release\$Arch\LWMS.EventDrivenRequests.dll" -Destination "..\LWMS\bin\Release\$Arch\LWMS.EventDrivenRequests.dll" -Force
cd ..
#  LWMS.SimpleDirectoryBrowser
cd LWMS.SimpleDirectoryBrowser
dotnet build -c Release -r $Arch
Copy-Item ".\bin\Release\$Arch\LWMS.SimpleDirectoryBrowser.dll" -Destination "..\LWMS\bin\Release\$Arch\LWMS.SimpleDirectoryBrowser.dll" -Force
cd ..
#  LWMS.Sample.MarkdownBlog
cd LWMS.Sample.MarkdownBlog
dotnet build -c Release -r $Arch
Copy-Item ".\bin\Release\$Arch\LWMS.Sample.MarkdownBlog.dll" -Destination "..\LWMS\bin\Release\$Arch\LWMS.Sample.MarkdownBlog.dll" -Force
cd ..
# Pack the binary files
cd ".\LWMS\bin\Release\"
Compress-Archive ".\$Arch\" ".\LWMS.$Arch.zip"
cd ..\..\..\