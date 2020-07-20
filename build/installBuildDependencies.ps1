$here = Split-Path -Path $MyInvocation.MyCommand.Source -Parent
$dependencies = Import-PowerShellDataFile (Join-Path -Path $here -ChildPath "dependencies.psd1")

foreach ($Module in $dependencies.BuildDependencies) {
    Find-Module @Module | Install-Module -Force
}
