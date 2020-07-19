Set-StrictMode -Version Latest

# Test taken from https://github.com/virot/DNSLogModule/blob/master/Pester/100%20-%20Get-DNSDebugLog.tests.ps1
# Test were converted to newer Pester Syntax and modified to suit the paths in this project

BeforeAll {
    Get-Module DNSDebugLogHandler | Remove-Module
    Import-Module "$PSScriptRoot\..\PowerShell\DNSDebugLogHandler.psd1" -ErrorAction Stop
    $ExampleData = Join-Path -Path $PSScriptRoot -ChildPath "Example-Data"
}

Describe "Basic tests" {
    It "Read a simple log" {
      ([array](Import-DNSDebugLog -Path "$ExampleData\dns-Locale_en-US-Windows2012.txt")).count | Should -Be 436
    }
    It "Read an empty log" {
      ([array](Import-DNSDebugLog -Path "$ExampleData\dns-format-Windows2012.txt")).count |Should -Be 0
    }
}
Describe "Localization" {
    It "Verify date on en-US log" {
      Import-DNSDebugLog -Culture 'en-US' -Path "$ExampleData\dns-Locale_en-US-Windows2012.txt" |Select -First 1 |%{Get-Date $_.Datetime -Format 'yyyy-MM-dd HH:mm:ss'} |Should -Be "2017-06-20 05:36:59"
    }
    It "Verify date on de-AT log" {
      Import-DNSDebugLog -Culture 'de-AT' -Path "$ExampleData\dns-Locale-de-AT-Windows2008r2.txt"|Select -First 1 |%{Get-Date $_.Datetime -Format 'yyyy-MM-dd HH:mm:ss'} |Should -Be "2015-09-30 12:04:28"
    }
    It "Verify date on sv-SE log" {
      Import-DNSDebugLog -Culture 'sv-SE' -Path "$ExampleData\dns-Locale-sv-SE-Windows2012r2.txt"|Select -First 1 |%{Get-Date $_.Datetime -Format 'yyyy-MM-dd HH:mm:ss'} |Should -Be "2017-05-21 19:46:11"
    }
    It "Verify date on de-DE log" {
      Import-DNSDebugLog -Culture 'de-DE' -Path "$ExampleData\dns-Locale-de-DE-WindowsUnknown.txt" |Select -First 1 |%{Get-Date $_.Datetime -Format 'yyyy-MM-dd HH:mm:ss'} |Should -Be "2020-03-06 17:15:40"
    }
}
