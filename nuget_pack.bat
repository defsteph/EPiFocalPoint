msbuild ImageResizer.Plugins.EPiFocalPoint.csproj /t:Build /p:Configuration="Release46"
msbuild ImageResizer.Plugins.EPiFocalPoint.csproj /t:Build /p:Configuration="Release452"
NuGet.exe pack ImageResizer.Plugins.EPiFocalPoint.csproj -OutputDirectory "Build" -Build -Properties Configuration=Release461