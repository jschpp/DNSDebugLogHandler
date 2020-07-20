param (
    [string] $Apikey,
    [switch] $Publish
)
$buildFile = Resolve-Path .\build\psakefile.ps1
if ($Publish) {
    if (-not $Apikey) {
        Write-Error -Message "Publishing needs the PSGallery API key"
    } else {
        Invoke-psake -buildFile $buildFile -taskList Publish
    }
} else {
    Invoke-psake -buildFile $buildFile -taskList Test
}
