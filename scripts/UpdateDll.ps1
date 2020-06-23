$srcBasePath = "..\..\src\zAppDev.DotNet.Framework\bin\Release"
$dstBasePath = "..\..\..\zAppDev.CodingFacility.CSharp.MVP\CLMS.CodingFacility.CS.Resources\Libraries"

Set-Location ..\src\zAppDev.DotNet.Framework

$res = dotnet build -c Release

if ($null -ne $res)
{
    Write-Host "Update net472"
    Copy-Item -Force -Confirm:$false "$srcBasePath\net472\zAppDev.DotNet.Framework.dll" "$dstBasePath"

    Write-Host "Update netcoreapp2.2"
    Copy-Item -Force -Confirm:$false "$srcBasePath\netcoreapp2.2\zAppDev.DotNet.Framework.dll" "$dstBasePath\Core"

    Write-Host "Update netstandard2.0"
    Copy-Item -Force -Confirm:$false "$srcBasePath\netstandard2.0\zAppDev.DotNet.Framework.dll" "$dstBasePath\Standard"
}

Set-Location ..\..\scripts
