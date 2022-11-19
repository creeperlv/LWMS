dotnet build -c Debug
cd LWMS.Management.Commands
dotnet build -c Debug
Copy-Item ".\bin\Debug\LWMS.Management.Commands.dll" -Destination "..\LWMS\bin\Debug\LWMS.Management.Commands.dll" -Force
cd ..