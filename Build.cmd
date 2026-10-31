@mkdir .\bin
dotnet msbuild ./src/SpaceTraffic.sln -t:Rebuild -p:Configuration=Release -restore
xcopy .\src\bin\Release\net10.0\*.* .\bin /S
