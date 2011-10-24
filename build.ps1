$buildDir = ".\build"
if (test-path $buildDir) { ri -r -fo $buildDir }
.\packages\psake.4.0.1.0\tools\psake.ps1 default.ps1 Compile 4.0
.\Tools\nuget\NuGet.exe pack .\xizzle.nuspec -BasePath $buildDir -OutputDirectory $buildDir
