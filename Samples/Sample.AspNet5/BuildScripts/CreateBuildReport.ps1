<#
.SYNOPSIS
    Generates a build report in HTML file, taking Build variables and Git commits.
.DESCRIPTION
    Script takes few parameters (from MSBuild task) and generates build_data.html file to be used in index page
    listing all collected data there.

    To include this in build, add these statements to main CSPROJ file of your solution:

    <Target Name="CreateBuildReport" BeforeTargets="GenerateAdditionalSources" >
      <PropertyGroup>
        <PowerShellCommand Condition=" '$(OS)' == 'Windows_NT' " >powershell</PowerShellCommand>
        <PowerShellCommand Condition=" '$(OS)' == 'Unix' " >pwsh</PowerShellCommand>
        <ScriptLocation>$(MSBuildProjectDirectory)\..\BuildScripts\CreateBuildReport.ps1</ScriptLocation>
        <RequestedBy  Condition="'$(RequestedBy)' == ''">Local user</RequestedBy>
      </PropertyGroup>
      <Exec Command="$(PowerShellCommand) -ExecutionPolicy Unrestricted -NoProfile -File $(ScriptLocation) -OS $(OS) -Platform $(Platform) -Configuration &quot;$(Configuration)&quot; -TargetFrameworkVersion &quot;$(TargetFrameworkVersion) ($(MSBuildRuntimeType))&quot; -RequestedBy &quot;$(RequestedBy)&quot;" />
    </Target>

.PARAMETER OS
    Name of operating system on which was build agent running.
.PARAMETER Platform
    Platform name, example "AnyCPU"
.PARAMETER Configuration
    A Configuration which was used for build (Examples: Debug, Production)
.PARAMETER TargetFrameworkVersion
    Build resulting assembly target framework (Example: Core 3.1, 5.0)
.PARAMETER RequestedBy
    Name of person or agent who requested build (relevant for DevOps server agents).
.EXAMPLE 
    CreateBuildReport -OS Windows -Platform AnyCPU -Configuration Debug -TargetFrameworkVersion Core5 -RequestedBy Someone
#>

Param (
    [string]$OS,
    [string]$Platform,
    [string]$Configuration,
    [string]$TargetFrameworkVersion,
    [string]$RequestedBy
)

$culture = [CultureInfo]'en-US'
$header = Get-Content $PSScriptRoot\build_report_header.html -Raw
$report = $header + "<h1>Build report</h1>`r`n"
$report += "<p>`r`n"
$report += "Build time: <strong>$((Get-Date).ToString("MMMM d yyyy HH:mm", $culture))</strong>`r`n"

$current_branch = git branch --show-current
if ([string]::IsNullOrWhiteSpace($current_branch)) {
    $current_branch = $env:BUILD_SOURCEBRANCH # Getting if from Azure DevOps Service environment variable
    if (-not [string]::IsNullOrWhiteSpace($current_branch)) {
        $current_branch = $current_branch.Replace("refs/heads/", "")
    }
}
$report += "<br/>`r`n"
$report += "From branch: <strong>$current_branch</strong>`r`n"

if ($RequestedBy -ne $null -and $RequestedBy -ne "") {
    $report += "<br/>`r`n"
    $report += "Requested by: <strong>$RequestedBy</strong>`r`n"
}

if ($Configuration -ne $null -and $Configuration -ne "") {
    $report += "<br/>`r`n"
    $report += "Configuration: <strong>$Configuration</strong>`r`n"
}

if ($Platform -ne $null -and $Platform -ne "") {
    $report += "<br/>`r`n"
    $report += "Platform: <strong>$Platform</strong>`r`n"
}

if ($TargetFrameworkVersion -ne $null -and $TargetFrameworkVersion -ne "") {
    $report += "<br/>`r`n"
    $report += "Framework: <strong>$TargetFrameworkVersion</strong>`r`n"
}

$report += "<br/>`r`n"
$report += "Built on: <strong>$OS</strong>`r`n"

$report += "</p>`r`n"

$report += "<h2>Included commits (latest)</h2>`r`n"
$report += "<table>`r`n<thead><tr><th>Date</th><th>Subject</th><th>Author</th><th>Commit</th></tr></thead>`r`n<tbody>`r`n"

$gitLog = (git log -15 --no-merges --format="%ai`t%h`t%an`t%s`t%H") | ConvertFrom-Csv -Delimiter "`t" -Header ("Date","CommitShort","Author","Subject","CommitId")
foreach ($commit in $gitLog) {
    $commit_date = [DateTime]$commit.Date
    $report += "<tr><td>$($commit_date.ToString("d-MMM HH:mm", $culture))</td><td>$($commit.Subject)</td><td>$($commit.Author)</td><td><a href=""https://your.repo.base.url/$($commit.CommitId)"" target=_blank>$($commit.CommitShort)</a></td></tr>`r`n"
}

$report += "</tbody></table>`r`n"

Write-Host "`r`n`r`n========================== BUILD REPORT: START ===========================`r`n"
Write-Host $report
Write-Host "========================== BUILD REPORT: END ===========================`r`n`r`n"

Out-File -Force -FilePath .\build_data.html -InputObject $report -Encoding utf8
