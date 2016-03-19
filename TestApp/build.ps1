# tools
$msbuild = "C:\Program Files (x86)\MSBuild\14.0\bin\MSBuild.exe"
set-alias msbuild $msbuild
 
# solution settings
$sln_name = "TestApp.sln"
$vs_config = "Release" 
$vs_platfom = "ARM"
 
# call the rebuild method

Write-Host "Building solution $sln_name" -foregroundcolor Green
msbuild $sln_name /t:Build /p:Configuration=$vs_config /p:Platform=$vs_platfom /nologo