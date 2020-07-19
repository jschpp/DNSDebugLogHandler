# Requires -Module platyPS
param (
    [string] $TargetPath,
    [string] $ProjectDir
)

$NewModulePath = Join-Path -Path $ProjectDir -ChildPath "PowerShell"
$LibPath = Join-Path -Path $NewModulePath -ChildPath "lib"
$DocSrcPath = Join-Path -Path $ProjectDir -ChildPath "docs"
$DocPath = Join-Path -Path $NewModulePath -ChildPath "en-US"

Copy-Item -Path $TargetPath -Destination $Destination

New-ExternalHelp -Path $DocSrcPath -OutputPath $DocPath
