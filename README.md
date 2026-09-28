# Unit Converter: WiX Desktop Deployment

A small Windows Forms unit converter (length, mass, temperature, volume, speed) built from
several class libraries and packaged as a Windows Installer (`.msi`) with **WiX Toolset v7**.

Written for SWE40006 Software Deployment and Evolution, Deployment Activity 1, Task 1.

## Solution layout

```
UnitConverter.sln
├── src/
│   ├── UnitConverter.Core/          → UnitConverter.Core.dll         abstractions + ConversionService
│   ├── UnitConverter.Units/         → UnitConverter.Units.dll        concrete units + built-in catalogue
│   ├── UnitConverter.Persistence/   → UnitConverter.Persistence.dll  JSON-file conversion history
│   └── UnitConverter.App/           → UnitConverter.exe              WinForms UI + composition root
├── tests/UnitConverter.Tests/       xUnit tests for Core, Units and Persistence
├── installer/UnitConverter.Installer/
│   ├── UnitConverter.Installer.wixproj   WiX SDK project (references the app project)
│   ├── Package.wxs                       product definition, .NET runtime check, UI
│   ├── Folders.wxs                       install and Start-menu directories
│   ├── AppComponents.wxs                 executable, DLLs and shortcut
│   └── License.rtf                       licence shown by the installer
├── packaging/UnitConverter.Package/  Windows Application Packaging Project (MSIX for the Microsoft Store)
│   ├── UnitConverter.Package.wapproj     x64 + ARM64 bundle, StoreUpload mode
│   ├── Package.appxmanifest              identity, logos, runFullTrust
│   └── Images/                           Store logos (generated)
├── tools/Generate-Assets.ps1        draws app.ico and all Store logos from one routine
├── docs/MicrosoftStore.md           how to publish the app to the Microsoft Store
├── docs/PRIVACY.md                  privacy policy (linked from the Store listing)
├── report/main.tex                  assignment report (LaTeX, compile on Overleaf)
└── build.ps1                        test + MSI (+ Store package with -Store) in one command
```

### Dependency graph

```
                  ┌──► UnitConverter.Units.dll ────────┐
UnitConverter.exe ├──► UnitConverter.Persistence.dll ──┼──► UnitConverter.Core.dll
                  └────────────────────────────────────┘
```

`Core` depends on nothing. `Units` and `Persistence` each implement one `Core` interface
(`IUnitCatalog`, `IConversionHistory`). Only `Program.cs` knows which concrete classes are used.

## Design notes

| Concern | How it is handled |
|---|---|
| Adding a unit | One `.Add(...)` line in `StandardUnitCatalog`. |
| Adding a category | One more `CategoryBuilder` block. The UI picks it up automatically. |
| Conversion maths | Every unit converts to/from a base unit (`base = value × factor + offset`), so *N* units need *N* definitions instead of *N²* pairs. Temperature uses the same `AffineUnit` class with an offset. |
| Changing storage | Implement `IConversionHistory` and change one line in `Program.cs`. |
| Testability | Services receive their dependencies (`IConversionHistory`, `TimeProvider`) through constructors. |
| Safe persistence | History is written to a temp file and then moved, so a crash never leaves a half-written file. A corrupt file is treated as empty. |

## Prerequisites

- .NET SDK 10 (includes the .NET 10 Desktop Runtime)
- WiX Toolset v7: `dotnet tool install --global wix`
- For the Store package, and optional otherwise: Visual Studio 2022 or later with the *.NET desktop development* workload and the
  [FireGiant HeatWave](https://marketplace.visualstudio.com/items?itemName=FireGiantHQ.FireGiantHeatWaveDev17) extension

## Build

```powershell
.\build.ps1          # tests + MSI
.\build.ps1 -Store   # tests + MSI + Microsoft Store package (needs Visual Studio)
```

This runs the unit tests and builds
`installer\UnitConverter.Installer\bin\Release\UnitConverterSetup.msi`.
Building the installer project also builds the application, because the `.wixproj` has a
`ProjectReference` to it.

With `-Store` it also builds
`packaging\UnitConverter.Package\AppPackages\UnitConverter.Package_1.0.0.0_x64_arm64_bundle.msixupload`,
the file uploaded to Partner Center. The MSIX contains a self-contained build of the app (the .NET
runtime is bundled); the MSI build stays framework-dependent. The `.wapproj` can only be built by
Visual Studio's MSBuild, not the `dotnet` CLI.

## Install and uninstall

```powershell
msiexec /i installer\UnitConverter.Installer\bin\Release\UnitConverterSetup.msi              # wizard
msiexec /i installer\UnitConverter.Installer\bin\Release\UnitConverterSetup.msi /qn /l*v install.log  # silent, admin prompt
msiexec /x installer\UnitConverter.Installer\bin\Release\UnitConverterSetup.msi              # uninstall
```

The app installs to `C:\Program Files\Unit Converter` and adds a Start-menu shortcut.
Conversion history is stored per user in `%LOCALAPPDATA%\UnitConverter\history.json`.
