param(
   [string] $in
)

if($in -eq "ok") {
    return '{"Test": 0}'
}

if($in -eq "more") {
    return '{"Test": 0}', '{"Test": 1}', '{"Test": 2}]'
}
