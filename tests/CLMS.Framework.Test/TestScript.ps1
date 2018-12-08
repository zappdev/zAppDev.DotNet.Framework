param(
   [string] $in
)

if($in -eq "raiseError") {
    NotACommand
}

Write-Host "Hello, World!"

return $false, $in