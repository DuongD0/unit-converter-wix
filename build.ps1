<#
.SYNOPSIS
    Runs the tests, then builds the WiX installer and, optionally, the Microsoft Store package.
.EXAMPLE
    .\build.ps1                  # tests + MSI
    .\build.ps1 -Store           # tests + MSI + MSIX upload package (needs Visual Studio)
    .\build.ps1 -Configuration Debug
#>
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [switch] $Store
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Invoke-Step([string] $Name, [scriptblock] $Command) {
    Write-Host "`n==> $Name" -ForegroundColor Cyan
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "$Name failed (exit code $LASTEXITCODE)." }
}

# The .wapproj can only be built by Visual Studio's MSBuild, so the dotnet CLI builds projects, not the .sln.
Invoke-Step 'Run unit tests'  { dotnet test tests\UnitConverter.Tests -c $Configuration }
Invoke-Step 'Build installer' { dotnet build installer\UnitConverter.Installer -c $Configuration }

$msi = Get-Item "installer\UnitConverter.Installer\bin\$Configuration\UnitConverterSetup.msi"
Write-Host "`nInstaller ready: $($msi.FullName) ($([math]::Round($msi.Length / 1KB)) KB)" -ForegroundColor Green

if ($Store) {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) { throw 'Visual Studio MSBuild not found; it is required to build the MSIX package.' }

    # Incremental builds reuse a cached AppxManifest.xml, so manifest edits (e.g. the Store identity)
    # would silently not reach the package. Always package from a clean state.
    $pkgDir = 'packaging\UnitConverter.Package'
    'bin', 'obj', 'AppPackages', 'BundleArtifacts' | ForEach-Object {
        Remove-Item (Join-Path $pkgDir $_) -Recurse -Force -ErrorAction SilentlyContinue
    }

    Invoke-Step 'Build Microsoft Store package' {
        & $msbuild packaging\UnitConverter.Package\UnitConverter.Package.wapproj `
            /restore /p:Configuration=$Configuration /p:Platform=x64 /v:minimal /nologo
    }

    $upload = Get-ChildItem packaging\UnitConverter.Package\AppPackages -Filter *.msixupload | Select-Object -First 1
    Write-Host "`nStore upload ready: $($upload.FullName) ($([math]::Round($upload.Length / 1MB)) MB)" -ForegroundColor Green
}
