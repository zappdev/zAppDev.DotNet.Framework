$dstBasePath = "../NugetLocalFeed"
$ideNugetLocalFeed = "../../zAppDev.new/NugetLocalFeed"
$cfNugetLocalFeed = "../../zAppDev.CodingFacility.CSharp.MVP/NugetLocalFeed"
$version = $args[0]

$libraries = @(
    "zAppDev.DotNet.Framework",
    "zAppDev.DotNet.Framework.Configurations"
)

Push-Location "../src"

Write-Host "Pack libraries"
foreach ($library in $libraries) {
    Push-Location "$library"
    Write-Host "Pack $library"
    Remove-Item "$dstBasePath/$library*"
    dotnet pack --no-restore --nologo  -c Release -o "$dstBasePath" -p:PackageVersion=$version
    Pop-Location
}

Write-Host "Sync libraries"
foreach ($library in $libraries) {
  Write-Host "Update $library"
  Remove-Item "$ideNugetLocalFeed/$library*"
  Copy-Item -Force -Confirm:$false "$dstBasePath\$library.$version.nupkg" "$ideNugetLocalFeed\"
  Remove-Item "$cfNugetLocalFeed/$library*"
  Copy-Item -Force -Confirm:$false "$dstBasePath\$library.$version.nupkg" "$cfNugetLocalFeed\"
}

Pop-Location