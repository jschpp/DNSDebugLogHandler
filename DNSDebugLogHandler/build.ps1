# Requires -Module platyPS
param (
    [string] $TargetPath,
    [string] $ProjectDir,
    [string] $OutDir
)

$NugetSpec = Join-Path -Path $ProjectDir -ChildPath "DNSDebugLogHandler.nuspec"
$NewModulePath = Join-Path -Path $ProjectDir -ChildPath "PowerShell"
$LibPath = Join-Path -Path $NewModulePath -ChildPath "lib"
$DocSrcPath = Join-Path -Path $ProjectDir -ChildPath "docs"
$DocPath = Join-Path -Path $NewModulePath -ChildPath "en-US"
$ModuleInfo = Test-ModuleManifest (Join-Path -Path $NewModulePath -ChildPath "DNSDebugLogHandler.psd1")
$OutputPath = Join-Path -Path $ProjectDir -ChildPath $OutDir

Copy-Item -Path $TargetPath -Destination $LibPath

New-ExternalHelp -Path $DocSrcPath -OutputPath $DocPath
