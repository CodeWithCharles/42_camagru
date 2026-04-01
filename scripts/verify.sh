#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

mkdir -p artifacts/logs artifacts/publish

export MSBuildEnableWorkloadResolver=false

if [[ -f ./Camagru.sln ]]; then
  SLN_PATH="./Camagru.sln"
elif [[ -f ./Camagru.slnx ]]; then
  SLN_PATH="./Camagru.slnx"
else
  mapfile -t solution_candidates < <(find . -maxdepth 2 \( -name "*.sln" -o -name "*.slnx" \) | sort)
  if [[ "${#solution_candidates[@]}" -eq 1 ]]; then
    SLN_PATH="${solution_candidates[0]}"
  else
    echo "Unable to determine a single solution file." >&2
    exit 1
  fi
fi

dotnet --info | tee artifacts/logs/dotnet-info.txt
dotnet build "$SLN_PATH" -c Debug -m:1 /nr:false -v normal | tee artifacts/logs/build-debug.txt
dotnet build "$SLN_PATH" -c Debug -m:1 /nr:false -bl:"artifacts/logs/build-debug.binlog" -v minimal
dotnet test "$SLN_PATH" -c Debug --no-build -m:1 /nr:false -v normal | tee artifacts/logs/test-debug.txt
dotnet publish "src/Camagru.Web/Camagru.Web.csproj" -c Release -m:1 /nr:false -o artifacts/publish -v normal | tee artifacts/logs/publish-release.txt
