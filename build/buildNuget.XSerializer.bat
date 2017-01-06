msbuild /p:Configuration=Build ..\XSerializer\XSerializer.csproj

ildasm ..\XSerializer\bin\Build\XSerializer.dll /out:..\XSerializer\bin\Build\XSerializer.il
ren ..\XSerializer\bin\Build\XSerializer.dll XSerializer.dll.orig
ilasm ..\XSerializer\bin\Build\XSerializer.il /res:..\XSerializer\bin\Build\XSerializer.res /dll /key=..\XSerializer\XSerializer.snk

nuget pack ..\XSerializer\XSerializer.csproj -Properties Configuration=Build