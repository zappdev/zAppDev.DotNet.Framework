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
    Remove-Item "../../NugetLocalFeed/$library*"
    dotnet pack --no-restore --nologo  -c Release -o "../../NugetLocalFeed" -p:PackageVersion=$version -p:Version=$version -p:AssemblyVersion=$version -p:FileVersion=$version
    Pop-Location
}

# Write-Host "Sync libraries"
# foreach ($library in $libraries) {
#   Write-Host "Update $library"
#   Remove-Item "../../zAppDev.new/NugetLocalFeed/$library*"
#   Copy-Item -Force -Confirm:$false "../NugetLocalFeed/$library.$version.nupkg" "../../zAppDev.new/NugetLocalFeed/"
#   Remove-Item "../../zAppDev.CodingFacility.CSharp.MVP/NugetLocalFeed/$library*"
#   Copy-Item -Force -Confirm:$false "../NugetLocalFeed/$library.$version.nupkg" "../../zAppDev.CodingFacility.CSharp.MVP/NugetLocalFeed/"
#   Remove-Item "../../zAppDev.CodingFacility.CSharp.MVP/CLMS.CodingFacility.CS.Resources/Libraries/$library*"
#   Copy-Item -Force -Confirm:$false "../NugetLocalFeed/$library.$version.nupkg" "../../zAppDev.CodingFacility.CSharp.MVP/CLMS.CodingFacility.CS.Resources/Libraries/"
# }

Pop-Location