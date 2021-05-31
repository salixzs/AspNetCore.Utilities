<#
.SYNOPSIS
    Generates a build report in HTML file, taking Build variales and Git commits.
.DESCRIPTION
    Script takes few parameters (from MSBuild task) and generates build_data.html file to be used in index page
    listing all collected data there.

    To include this in build, add these statements to main CSPROJ file of your solution:

    <Target Name="CreateBuildReport" AfterTargets="AfterBuild" >
      <PropertyGroup>
        <PowerShellCommand Condition=" '$(OS)' == 'Windows_NT' " >powershell</PowerShellCommand>
        <PowerShellCommand Condition=" '$(OS)' == 'Unix' " >pwsh</PowerShellCommand>
        <ScriptLocation>$(MSBuildProjectDirectory)\..\BuildScripts\CreateBuildReport.ps1</ScriptLocation>
        <RequestedBy  Condition="'$(RequestedBy)' == ''">Local user</RequestedBy>
      </PropertyGroup>
      <Exec Command="$(PowerShellCommand) -ExecutionPolicy Unrestricted -NoProfile -File $(ScriptLocation) -OS $(OS) -Platform $(Platform) -Configuration &quot;$(Configuration)&quot; -TargetFrameworkVersion &quot;$(TargetFrameworkVersion) ($(MSBuildRuntimeType))&quot; -RequestedBy &quot;$(RequestedBy)&quot;" />
    </Target>

.PARAMETER OS
    Name of operating system on which was build agent wunning.
.PARAMETER Platform
    Platform name, example "AnyCPU"
.PARAMTER Configuration
    A Configuration which was used for build (Examples: Debug, Production)
.PARAMETER TargetFrameworkVersion
    Build resulting assembly target framework (Example: Core 3.1, 5.0)
.PARAMTER RequestedBy
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
$report = "<h1>Build report</h1>`r`n"
$report += "<p>`r`n"
$report += "Build time: <strong>$((Get-Date).ToString("d-MMM-yyyy HH:mm", $culture))</strong>`r`n"

$current_branch = git branch --show-current
$report += "<br/>`r`n"
$report += "From branch: <strong>$current_branch</strong>`r`n"

if ($RequestedBy -ne $null -and $RequestedBy -ne "") {
    $report += "<br/>`r`n"
    $report += "Requested by: <strong>$RequestedBy</strong>`r`n"
}

<#
$report += "<br/>"
$branch_fullname = $SourceBranch.Replace("refs/heads/", "")
Write-Host "Source branch: $branch_fullname"
#>

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

$report += "<h2>Latest commits</h2>`r`n"
$report += "<table>`r`n<thead><tr><th>Date</th><th>Subject</th><th>Author</th><th>Commit</th></tr></thead>`r`n<tbody>`r`n"

$gitLog = (git log -15 --no-merges --format="%ai`t%h`t%an`t%s") | ConvertFrom-Csv -Delimiter "`t" -Header ("Date","CommitId","Author","Subject")
foreach ($commit in $gitLog) {
    $commit_date = [DateTime]$commit.Date
    $report += "<tr><td>$($commit_date.ToString("d-MMM HH:mm", $culture))</td><td>$($commit.Subject)</td><td>$($commit.Author)</td><td>$($commit.CommitId)</td></tr>`r`n"
}

$report += "</tbody></table>`r`n"

Write-Host $report

Out-File -Force -FilePath .\build_data.html -InputObject $report -Encoding utf8