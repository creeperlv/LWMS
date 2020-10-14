dotnet build -c Release
cd LWMS.Management.Commands
dotnet build -c Release
Copy-Item ".\bin\Release\netcoreapp3.1\LWMS.Management.Commands.dll" -Destination "..\LWMS\bin\Release\netcoreapp3.1\LWMS.Management.Command.dll" -Force
cd ..