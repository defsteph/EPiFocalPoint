$vsVersion = "14.0"
$regKey = "HKLM:\software\Microsoft\MSBuild\ToolsVersions\$vsVersion"
$regProperty = "MSBuildToolsPath"

$msbuildExe = join-path -path (Get-ItemProperty $regKey).$regProperty -childpath "msbuild.exe"

&$msbuildExe ImageResizer.Plugins.EPiFocalPoint.csproj /t:Build /p:Configuration="Release46"
&$msbuildExe ImageResizer.Plugins.EPiFocalPoint.csproj /t:Build /p:Configuration="Release452"
.\NuGet.exe pack ImageResizer.Plugins.EPiFocalPoint.csproj -OutputDirectory "Build" -Build -Properties Configuration=Release461