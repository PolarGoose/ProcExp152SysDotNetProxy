Function Info($msg) {
    Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
    Write-Host `n`n
    Write-Error $msg
    exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
    if(-Not $?) {
        Error "${msg}. Error code: $LastExitCode"
    }
}

Function GetVersion() {
    $gitCommand = Get-Command -Name git

    try { $tag = & $gitCommand describe --exact-match --tags HEAD 2>$null } catch { }
    if(-Not $?) {
        Info "The commit is not tagged. Use 'v0.0' as a version instead"
        $tag = "v0.0"
    }

    return "$($tag.Substring(1))"
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot"
$buildDir = "$root/build"
$version = GetVersion

Info "Create Nuget package. Version=$version"
dotnet pack `
  --configuration Release `
  /property:Version=$version `
  "$root/ProcExp152SysDotNetProxy.sln"
CheckReturnCodeOfPreviousCommand "Nuget package creation failed"

Info "Copy the Nuget package into the publish directory"
Remove-Item "$buildDir/Publish" -Force -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path "$buildDir/Publish" > $null
Copy-Item "$buildDir/Release/ProcExp152SysDotNetProxy/*.nupkg" "$buildDir/Publish"
