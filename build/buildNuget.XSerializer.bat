nuget restore -SolutionDirectory ..\  ..\XSerializer\XSerializer.csproj

msbuild /p:Configuration=Build /t:Clean ..\XSerializer\XSerializer.csproj

msbuild /p:Configuration=Build /t:Rebuild ..\XSerializer\XSerializer.csproj

msbuild /t:pack /p:PackageOutputPath=..\build  /p:Configuration=Build ..\XSerializer\XSerializer.csproj