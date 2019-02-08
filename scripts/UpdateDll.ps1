$srcBasePath = "D:\Work\CLMS\CLMS.Framework\src\CLMS.Framework\bin\Release"
$dstBasePath = "D:\Work\zAppDev.CodingFacility.CSharp.MVP\CLMS.CodingFacility.CS.Resources\Libraries"

cd ..\src\CLMS.Framework

$res = dotnet build -c Release

if ($res -ne $null)
{
    Write-Host "Update net462"
    Copy-Item -Force -Confirm:$false "$srcBasePath\net462\CLMS.Framework.dll" "$dstBasePath"

    Write-Host "Update netstandard2.0"
    Copy-Item -Force -Confirm:$false "$srcBasePath\netstandard2.0\CLMS.Framework.dll" "$dstBasePath\Standard"
}

cd ..\..\scripts
