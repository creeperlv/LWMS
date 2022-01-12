dotnet build -c Debug
cd LWMS.Management.Commands
dotnet build -c Debug
Copy-Item ".\bin\Debug\net6.0\LWMS.Management.Commands.dll" -Destination "..\LWMS\bin\Debug\net6.0\LWMS.Management.Commands.dll" -Force
cd ..