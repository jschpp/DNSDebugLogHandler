param (
    [string] $ApiKey
)

$ModuleName = "DNSDebugLogHandler"
$ProjectDir = Join-Path -Path (Split-Path -Path $MyInvocation.MyCommand.Source -Parent) -ChildPath ".." -Resolve
$ProjectFile = Join-Path -Path $ProjectDir -ChildPath DNSDebugLogHandler.csproj -Resolve
$BuildPath = Join-Path -Path $ProjectDir -ChildPath $ModuleName

Task default -depends Test, CompileHelp, CopyMetaData

Task Clean {
    $ToDelete = @(
        $BuildPath,
        "$ProjectDir/TestResult.xml",
        "$ProjectDir/pester-codecoverage.xml",
        [System.IO.Path]::Combine($ProjectDir, "{0}-nunit.xml" -f $ModuleName)
        )
    foreach ($item in $ToDelete) {
        if (Test-Path $item) {
            Write-Output $item
            Remove-Item $item -Recurse -Force
        }
        Assert -conditionToCheck (-not (Test-Path $item)) -failureMessage "Couldn't delete $item"
    }
    Exec -cmd {dotnet clean $ProjectFile --configuration Release}

}

Task SetUp -depends Clean {
    New-Item -Path $BuildPath -ItemType Directory
    New-Item -Path $BuildPath -ItemType Directory -Name "en-US"
    New-Item -Path $BuildPath -ItemType Directory -Name "lib"
    New-Item -Path $BuildPath -ItemType Directory -Name "format"
}

Task CompileCSharp -depends SetUp {
    $lib = Join-Path -Path $BuildPath -ChildPath "lib"
    Exec -cmd {dotnet build $ProjectFile --configuration Release --no-dependencies}
    Exec -cmd {dotnet publish $ProjectFile -o $lib --no-build --configuration Release --no-dependencies}
    Remove-Item "$lib/*" -Exclude "*.dll"
}

Task CompileHelp -depends SetUp {
    New-ExternalHelp -Path "$ProjectDir/docs" -OutputPath "$BuildPath/en-US"
}

Task Test -depends CompileCSharp, CopyMetaData {
    $configuration = @{
        Run          = @{
            Path     = [string](Join-Path -Path $ProjectDir -ChildPath "Tests")
            PassThru = $true
        }
        CodeCoverage = @{
            Enabled    = $true
            OutputPath = [string](Join-Path -Path $ProjectDir -ChildPath "pester-codecoverage.xml")
        }
        TestResult   = @{
            Enabled    = $true
            OutputPath = [string](Join-Path -Path $ProjectDir -ChildPath (
                '{0}-nunit.xml' -f $ModuleName
            ))
        }
        Output       = @{
            Verbosity = 'Detailed'
        }
    }
    $testResults = Invoke-Pester -Configuration $configuration
    Assert -conditionToCheck ($testResults.FailedCount -eq 0) -failureMessage "Pester Test failes"
}

Task CopyMetaData -depends SetUp {
    Copy-Item -Path "$ProjectDir/src/$ModuleName.psd1" -Destination $BuildPath
    Copy-Item -Path "$ProjectDir/src/format/*.ps1xml" -Destination "$BuildPath/format"
}

Task Publish {
    Assert (Test-Path $BuilPath) -failureMessage "Module directory not found"
    try {
        $ModuleManifest = Test-ModuleManifest (Join-Path $BuildPath -ChildPath "$ModuleName.psd1") -ErrorAction Stop
    } catch {
        $ModuleManifest = $false
    }
    Assert ($ModuleManifest) -failureMessage "Error in the Modulemanifest"
    Publish-Module -Path $BuildPath -NuGetApiKey $ApiKey
}
