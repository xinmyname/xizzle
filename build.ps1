$buildDir = ".\build"
if (test-path $buildDir) { ri -r -fo $buildDir }
.\tools\psake\psake.ps1 default.ps1 Compile 4.0
.\Tools\nuget\NuGet.exe pack .\xizzle.nuspec -BasePath $buildDir -OutputDirectory $buildDir
