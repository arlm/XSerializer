msbuild /p:Configuration=Release ..\XSerializer\XSerializer.csproj
nuget pack ..\XSerializer\XSerializer.csproj -Properties Configuration=Release