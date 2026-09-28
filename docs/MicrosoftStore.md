# Publishing Unit Converter to the Microsoft Store

The Microsoft Store accepts desktop applications in two forms. This guide covers both and shows
how each one applies to this project.

| | Route A: MSIX package | Route B: the existing MSI |
|---|---|---|
| What is uploaded | An `.msixupload` built by a *Windows Application Packaging Project* | A **URL** to `UnitConverterSetup.msi` hosted on HTTPS |
| Who installs and updates | The Store (clean install/uninstall, automatic updates, containerised file system and registry writes) | The MSI itself; the Store runs it silently |
| Code signing | Done by Microsoft during certification | **You** must sign the MSI with an Authenticode certificate from a CA in the Microsoft Trusted Root Program |
| Changes to this project | Add a packaging project; ship the app **self-contained** | Sign and host the MSI; no code changes |
| Covered by the unit walkthrough | Yes (section 6) | No |

Route A is the one the walkthrough describes and costs nothing beyond the developer account,
so it is the recommended route. Route B is included because it reuses the WiX installer
produced in Tasks 1.2 and 1.3 as is.

---

## 1. Common steps (both routes)

### 1.1 Register a developer account
1. Go to <https://storedeveloper.microsoft.com> and choose **Individual developer**.
2. Sign in with a Microsoft account and complete identity verification (government ID and a selfie).
   Verification can take from a few minutes to a few business days.
3. When the account is approved, open **Partner Center** at <https://partner.microsoft.com/dashboard>.

### 1.2 Reserve the app name
1. Go to **Apps and games → New product**.
2. Choose **MSIX or PWA app** (Route A) or **EXE or MSI app** (Route B).
3. Enter a name and select **Check availability**. Generic names such as "Unit Converter" are
   usually taken, so use a distinctive one, for example *"Unit Converter – SWE40006 <Your Name>"*.
4. Select **Reserve product name**. A reservation lasts three months unless a submission is made.

After reserving, open **Product management → Product identity** and note these values. Route A needs them:

| Field | Example |
|---|---|
| `Package/Identity/Name` | `12345YourName.UnitConverterSWE40006` |
| `Package/Identity/Publisher` | `CN=ABCDEF12-3456-7890-ABCD-EF1234567890` |
| `Package/Properties/PublisherDisplayName` | `Your Name` |

---

## 2. Route A: MSIX via a Windows Application Packaging Project

> **Status in this repository:** sections 2.2, 2.3 and 2.5 are already done. The packaging project
> is `packaging/UnitConverter.Package`, the app becomes self-contained when packaged, and
> `.\build.ps1 -Store` produces the `.msixupload`. What remains is the account-specific work:
> reserve a name (1.2), associate the package with it (2.4), rebuild, run WACK, and submit (2.6).

### 2.1 Prerequisites
Visual Studio 2022 or later with the **.NET desktop development** and **WinUI application development**
workloads (the latter installs the packaging project template and the Windows SDK).

### 2.2 Add the packaging project
1. Open `UnitConverter.sln` in Visual Studio.
2. **File → Add → New Project → Windows Application Packaging Project**. Name it
   `UnitConverter.Package`.
3. Accept the default target and minimum Windows versions (Windows 10, version 1809 or later
   is a sensible minimum).
4. In the new project, right-click **Dependencies → Add Project Reference** and tick
   **UnitConverter.App**. Because the app project references the three library projects,
   `UnitConverter.Core.dll`, `UnitConverter.Units.dll` and `UnitConverter.Persistence.dll` are
   packaged automatically.
5. Right-click `UnitConverter.App` under **Dependencies → Applications** and choose
   **Set as Entry Point**.

### 2.3 Make the app self-contained
The WiX installer checks that the .NET 10 Desktop Runtime is installed (`DotNetCompatibilityCheck`).
An MSIX package cannot run a prerequisite installer, so the runtime must be inside the package.
This project does that as follows:

```xml
<!-- Directory.Build.props: every C# project restores for both runtimes (avoids NETSDK1047) -->
<RuntimeIdentifiers>win-x64;win-arm64</RuntimeIdentifiers>

<!-- UnitConverter.App.csproj: self-contained only when built for a specific runtime (i.e. by the
     packaging project), so the WiX MSI build stays framework-dependent -->
<SelfContained Condition="'$(RuntimeIdentifier)' != ''">true</SelfContained>
```

### 2.4 Associate the package with the Store reservation
1. Right-click `UnitConverter.Package` and choose **Publish → Associate App with the Store**.
2. Sign in with the developer account and select the reserved name.

   Visual Studio writes the identity values from section 1.2 into `Package.appxmanifest`. Doing
   this by hand is error-prone, because any mismatch is rejected at upload.
3. Open `Package.appxmanifest` and complete:
   - **Visual Assets**: use *Asset Generator* with a 400×400 source image to create every tile and
     logo size, including `StoreLogo`, `Square44x44Logo` and `Square150x150Logo`.
   - **Packaging**: set the version to `1.0.0.0`. The fourth part must stay `0`, because the Store
     reserves it.

   The generated manifest declares the `runFullTrust` restricted capability. A packaged Win32
   desktop app needs it, and Partner Center asks for a one-line justification such as
   *"Windows Forms desktop application converted with the Desktop Bridge."*

### 2.5 Create the upload package
1. Right-click `UnitConverter.Package` and choose **Publish → Create App Packages…**
2. Choose **Microsoft Store as <reserved name>**, then select **Next**.
3. Select the architectures **x64** and **ARM64**, set the configuration to **Release**, and leave
   *Generate artifacts to upload to the Microsoft Store* ticked.
4. Select **Create**. The output is
   `AppPackages\UnitConverter.Package_1.0.0.0_x64_arm64_bundle.msixupload`.
5. When prompted, run the **Windows App Certification Kit (WACK)**. It runs the same basic checks
   as Store certification (manifest, supported APIs, launch and suspend, and so on). Fix every
   failure before uploading. A local pass avoids a rejected submission, which otherwise costs
   one to three days.

### 2.6 Submit in Partner Center
Open the product and select **Start your submission**. Complete each section:

| Section | What to provide for Unit Converter |
|---|---|
| **Pricing and availability** | Free; all markets; *Public audience* and *Make this product available and discoverable in the Store* so anyone can find and download it. |
| **Properties** | Category *Utilities & tools*. Provide a privacy-policy URL. It is mandatory for apps that access personal information and is often requested for full-trust desktop apps, so include one anyway. A short policy hosted with the source code is enough, for example: *"Stores conversion history only on this device; collects and transmits no data."* |
| **Age ratings** | Complete the IARC questionnaire (no violence, no user interaction, no data sharing). The expected result is *3+ / Everyone*. |
| **Packages** | Drag in the `.msixupload`. Partner Center checks the identity, version and capabilities immediately. |
| **Store listings** (English) | Description, at least one screenshot (1366×768 or larger), short feature list and search keywords. |
| **Submission options** | Choose to publish as soon as certification passes. Add notes for certification, for example *"No account or test data needed; select a category, enter a value and choose Convert."* |

Select **Submit to the Store**. Certification (security scan, technical compliance and content
review) normally takes up to three business days. Partner Center shows the progress, and any
failure report lists the policy number involved.

### 2.7 After publishing
- The public listing is at `https://apps.microsoft.com/detail/<StoreID>`. The 12-character Store
  ID is shown under **Product identity**. This is the public URL for the report.
- **Updates**: increase the package version (for example `1.0.1.0`), create new app packages and
  start a new submission. The Store delivers the update to existing users automatically.

---

## 3. Route B: submit the WiX MSI directly

The Store can list a traditional MSI or EXE installer that you host yourself.

1. **Sign the MSI.** Unsigned or self-signed installers are rejected. You need an Authenticode
   code-signing certificate from a CA in the Microsoft Trusted Root Program, or Azure Trusted Signing.
   ```powershell
   signtool sign /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 /a UnitConverterSetup.msi
   ```
   The same certificate should also sign `UnitConverter.exe` before the MSI is built.
2. **Host it at a versioned HTTPS URL.** The file at the URL must never change, so each version
   needs its own URL, for example a GitHub Release asset
   `https://github.com/DuongD0/unit-converter-wix/releases/download/v1.0.0/UnitConverterSetup.msi`.
3. **Reserve the name as an *EXE or MSI app*** (section 1.2), then under **Packages** enter:
   - Package URL: the URL from step 2
   - Architecture: x64
   - Installer parameters: none (MSI silent install `/qn` is implicit)
4. Complete Pricing, Properties, Age ratings and Store listings as in section 2.6, and submit.
5. The Store downloads the MSI and installs it silently on test machines, so the installer must
   not require user interaction. Also declare the .NET 10 Desktop Runtime dependency in the
   listing. The `DotNetCompatibilityCheck` launch condition blocks installation when the runtime
   is missing.

Updates are handled by the installer rather than the Store. The `MajorUpgrade` element in
`Package.wxs` already replaces older versions, so publishing an update means building a new MSI
with a higher `Version`, hosting it at a new URL, and submitting that URL.

---

## 4. Common rejection reasons and how this project avoids them

| Rejection reason | Prevention |
|---|---|
| Manifest identity does not match the reservation | Use *Associate App with the Store* rather than editing identity by hand (2.4). |
| App crashes on launch on a clean machine (missing .NET runtime) | Self-contained build for MSIX (2.3); runtime launch condition for MSI (Route B). |
| Missing privacy policy | Provide the policy URL in Properties (2.6). |
| Listing screenshots or description don't match the app | Take screenshots of the installed build; describe only features that exist. |
| MSI/EXE not signed, or URL content changed | Sign with a trusted certificate and use one immutable URL per version (Route B). |
