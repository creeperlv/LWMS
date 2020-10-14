dotnet build -c Debug
cd LWMS.Management.Commands
dotnet build -c Debug
Copy-Item ".\bin\Debug\netcoreapp3.1\LWMS.Management.Commands.dll" -Destination "..\LWMS\bin\Debug\netcoreapp3.1\LWMS.Management.Commands.dll" -Force
cd ..