msbuild /p:Configuration=Release ..\XSerializer\XSerializer.csproj

ildasm ..\XSerializer\bin\Release\XSerializer.dll /out:..\XSerializer\bin\Release\XSerializer.il
ren ..\XSerializer\bin\Release\XSerializer.dll ..\XSerializer\bin\Release\XSerializer.dll.orig
ilasm ..\XSerializer\bin\Release\XSerializer.il /res:..\XSerializer\bin\Release\XSerializer.res /dll /key=..\XSerializer\XSerializer.snk

nuget pack ..\XSerializer\XSerializer.csproj -Properties Configuration=Release