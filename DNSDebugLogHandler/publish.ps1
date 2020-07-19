param (
    [Parameter(Mandatory=$true)]
    [string] $ApiKey
)
$ModuleName = "DNSDebugLogHandler"

$NewModuleDir = New-Item -Path (Join-Path -Path $PSScriptRoot -ChildPath $ModuleName) -ItemType Directory
Copy-Item -Recurse -Path (Join-Path -Path $PSScriptRoot -ChildPath "Powershell\*") -Destination $NewModuleDir -Exclude ".gitkeep"

Publish-Module -Path $NewModuleDir -NuGetApiKey $ApiKey
