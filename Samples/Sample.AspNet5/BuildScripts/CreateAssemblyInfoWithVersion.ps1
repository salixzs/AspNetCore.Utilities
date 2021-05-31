<#
.SYNOPSIS
    Generates AssemblyInfo.cs file from template with calculated version number.
.DESCRIPTION
    Script takes template "AssemblyInfo.txt" in script directory and generates
    from it AssemblyInfo.cs file, placed in Project directory, which is executing this script.
    Version is in format "X.X.Days.Minutes", where
      - X.X = from MasjorMinorVersion parameter and can be anything, like "1.0", "2.5.12"
      - Days = calucated days between given StartDate parameter and today.
      - Minutes = minutes since [todays] midnight.
    Resulting version is something like this "1.0.127.893" with ever incrementing last two values.
    You can calculate exact build time back from this, if you know StartDate.

    To add it as part of project build, add this to csproj file:

<Target Name="GenerateVersionedAssemblyInfo" AfterTargets="GenerateAdditionalSources" >
  <PropertyGroup>
  <ProjectStartDate>2021-05-09T09:00:00</ProjectStartDate>
  <BaseVersion>2.1</BaseVersion>

  <PowerShellCommand Condition=" '$(OS)' == 'Windows_NT' " >powershell</PowerShellCommand>
  <PowerShellCommand Condition=" '$(OS)' == 'Unix' " >pwsh</PowerShellCommand>
  <ScriptLocation>$(MSBuildProjectDirectory)\..\BuildScripts\CreateAssemblyInfoWithVersion.ps1</ScriptLocation>
  </PropertyGroup>
  <Message Text="        Running on OS: $(OS)" />
  <Exec Command="$(PowerShellCommand) -ExecutionPolicy Unrestricted -NoProfile -File $(ScriptLocation) -StartDate $(ProjectStartDate) -MajorMinorVersion $(BaseVersion)" />
</Target>

.PARAMETER StartDate
    DateTime of either project start or last major version change.
.PARAMETER MajorMinorVersion
    String in form of "X.X" or "X.X.X" to specifu major, minor version numbers (static ones). Do not add a dot in the end!

.EXAMPLE 
    .\CreateAssemblyInfoWithVersion.ps1 -StartDate "2021-05-31 09:00:00" -MajorMinorVersion "2.4"
#>

Param (
    [DateTime]$StartDate,
    [string]$MajorMinorVersion
)

$days_passed = [int]((Get-Date) - $StartDate).TotalDays;
$minutes_since_midnight = [int](Get-Date).TimeOfDay.TotalMinutes;

Write-Host "      Given start date: $($StartDate.ToString("d-MMM-yyyy HH:mm"))"
Write-Host "           Days passed: $days_passed"
Write-Host "Minutes since midnight: $minutes_since_midnight"
Write-Host "       Setting Version: $MajorMinorVersion.$days_passed.$minutes_since_midnight" -ForegroundColor Green

$assembly_template = Get-Content $PSScriptRoot\AssemblyInfo.txt -Raw
$assembly_info = $assembly_template.Replace("{version}", "$MajorMinorVersion.$days_passed.$minutes_since_midnight")

Out-File -Force -FilePath .\AssemblyInfo.cs -InputObject $assembly_info -Encoding utf8