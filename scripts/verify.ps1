Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
Set-Location $RootDir

New-Item -ItemType Directory -Force -Path "artifacts/logs" | Out-Null
New-Item -ItemType Directory -Force -Path "artifacts/publish" | Out-Null
$env:MSBuildEnableWorkloadResolver = "false"

if (Test-Path "./Camagru.sln") {
    $SlnPath = "./Camagru.sln"
}
elseif (Test-Path "./Camagru.slnx") {
    $SlnPath = "./Camagru.slnx"
}
else {
    $solutionCandidates = Get-ChildItem -Path . -Recurse -Depth 2 -Include *.sln, *.slnx | Sort-Object FullName
    if ($solutionCandidates.Count -eq 1) {
        $SlnPath = $solutionCandidates[0].FullName
    }
    else {
        throw "Unable to determine a single solution file."
    }
}

dotnet --info | Tee-Object -FilePath "artifacts/logs/dotnet-info.txt"
if ($LASTEXITCODE -ne 0) { throw "dotnet --info failed." }

dotnet build $SlnPath -c Debug -m:1 /nr:false -p:UseSharedCompilation=false -v normal | Tee-Object -FilePath "artifacts/logs/build-debug.txt"
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }

dotnet build $SlnPath -c Debug -m:1 /nr:false -p:UseSharedCompilation=false -bl:"artifacts/logs/build-debug.binlog" -v minimal
if ($LASTEXITCODE -ne 0) { throw "dotnet build binlog generation failed." }

dotnet test $SlnPath -c Debug --no-build -m:1 /nr:false -p:UseSharedCompilation=false -v normal | Tee-Object -FilePath "artifacts/logs/test-debug.txt"
if ($LASTEXITCODE -ne 0) { throw "dotnet test failed." }

dotnet publish "src/Camagru.Web/Camagru.Web.csproj" -c Release -m:1 /nr:false -p:UseSharedCompilation=false -o "artifacts/publish" -v normal | Tee-Object -FilePath "artifacts/logs/publish-release.txt"
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }
