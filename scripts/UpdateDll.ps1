$srcBasePath = "D:\Work\CLMS\zAppDev.DotNet.Framework\src\zAppDev.DotNet.Framework\bin\Release"
$dstBasePath = "D:\Work\zAppDev.CodingFacility.CSharp.MVP\CLMS.CodingFacility.CS.Resources\Libraries"

Set-Location ..\src\zAppDev.DotNet.Framework

$res = dotnet build -c Release

if ($null -ne $res)
{
    Write-Host "Update net462"
    Copy-Item -Force -Confirm:$false "$srcBasePath\net462\zAppDev.DotNet.Framework.dll" "$dstBasePath"

    Write-Host "Update netcoreapp2.2"
    Copy-Item -Force -Confirm:$false "$srcBasePath\netcoreapp2.2\zAppDev.DotNet.Framework.dll" "$dstBasePath\Standard"
}

Set-Location ..\..\scripts
