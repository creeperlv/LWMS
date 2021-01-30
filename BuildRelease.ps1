dotnet build -c Release
cd LWMS.Management.Commands
dotnet build -c Release
Copy-Item ".\bin\Release\net5.0\LWMS.Management.Commands.dll" -Destination "..\LWMS\bin\Release\net5.0\LWMS.Management.Commands.dll" -Force
cd ..