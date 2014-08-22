msbuild /p:Configuration=Release ..\XSerializer.WebApi\XSerializer.WebApi.csproj
nuget pack ..\XSerializer.WebApi\XSerializer.WebApi.csproj -Properties Configuration=Release