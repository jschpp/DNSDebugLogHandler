Import-Module platyPS
param (
    [string] $TargetPath,
    [string] $ProjectDir
)
$Destination = [System.IO.Path]::Combine($ProjectDir, "PowerShell", "lib")

Copy-Item -Path $TargetPath -Destination $Destination
