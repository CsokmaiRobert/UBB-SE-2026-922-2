# BoardRent + PropertyAndManagement Merge Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the merged third solution at `E:\UBB-SE-2026-922-2\BoardRentAndProperty\` per the design in `docs/superpowers/specs/2026-04-28-boardrent-and-property-merge-design.md`. The merged app is PaM-as-host with BoardRent's UI folded in as a feature area.

**Architecture:** Three projects in one solution: a merged WinUI 3 app (`BoardRentAndProperty`), `ServerCommunication` (verbatim copy), and `NotificationServer` (verbatim copy). Inside the merged app: flat layered folders (`Models/`, `Repositories/`, `Services/`, `ViewModels/`, `Views/`, `Mappers/`, `DataTransferObjects/`, `Data/`, `Utilities/`, `Constants/`), interfaces inline, BoardRent's user-domain renamed to `Account*`, two independent databases.

**Tech Stack:** .NET 8, WinUI 3 (Windows App SDK 1.8), `Microsoft.Extensions.DependencyInjection`, `CommunityToolkit.Mvvm`, `Microsoft.Data.SqlClient`, raw ADO.NET (no EF), `H.NotifyIcon.WinUI` for tray, `Microsoft.Toolkit.Uwp.Notifications` for toasts.

**TDD adaptation:** This is structural/scaffolding work — copy files, rename types, integrate two `App.xaml.cs` lifecycles. Unit tests are out of scope (per the design spec). The verification gate at every task is `dotnet build` + a manual smoke check; the final task is a full smoke test with two windows. Where new logic is introduced (the `App.NavigateTo` static helpers, the back-button click handler, the `FindNotificationServerBinDir` walker), the build is the test — if it compiles and the runtime smoke succeeds, the change is verified.

**Working directory for all bash commands:** `/e/UBB-SE-2026-922-2/`. Use POSIX-style paths in bash. The merged solution lives under `BoardRentAndProperty/`.

**Source paths to copy from:**
- BoardRent app: `BoardRent_A1+A2/BoardRent/`
- BoardRent core lib: `BoardRent_A1+A2/BoardRent.Core/`
- PaM app: `PropertyAndManagement/Property_and_Management/`
- PaM siblings: `PropertyAndManagement/NotificationServer/`, `PropertyAndManagement/ServerCommunication/`
- PaM root config: `PropertyAndManagement/App.config`, `PropertyAndManagement/SE.ruleset`

---

## File Structure (final state of merged project)

```
BoardRentAndProperty/
├── BoardRentAndProperty.sln
├── App.config                      ← from PaM
├── SE.ruleset                      ← from PaM
├── NotificationServer/             ← verbatim from PaM
├── ServerCommunication/            ← verbatim from PaM
└── BoardRentAndProperty/
    ├── BoardRentAndProperty.csproj
    ├── App.xaml(.cs)               ← MERGED (PaM host + BoardRent DI)
    ├── MainWindow.xaml(.cs)        ← from PaM
    ├── Package.appxmanifest        ← from PaM (identity renamed)
    ├── app.manifest                ← from PaM
    ├── stylecop.json               ← from BoardRent
    ├── Constants.cs                ← from PaM
    ├── ConstantsBridge.cs          ← from PaM
    ├── NotificationManager.cs      ← from PaM
    ├── InternalsVisibleTo.cs       ← from PaM
    ├── Assets/                     ← merged (both apps' assets)
    ├── Properties/launchSettings.json
    ├── Scripts/                    ← from PaM, verbatim
    ├── Constants/                  ← from PaM (subfolder)
    ├── Data/                       ← BoardRent's data layer (AppDbContext renamed schema)
    ├── Models/                     ← Account, AccountRole, Role, FailedLoginAttempt, User, Game, Rental, Request, Notification
    ├── DataTransferObjects/        ← FLAT, both apps' DTOs
    ├── Mappers/                    ← PaM mappers + new AccountMapper, AccountProfileMapper
    ├── Repositories/               ← FLAT, interfaces inline, BoardRent renames + PaM
    ├── Services/                   ← FLAT, interfaces inline + Listeners/ subfolder
    ├── ViewModels/                 ← FLAT
    ├── Views/                      ← FLAT (LoginPage with new back button; MenuBarPage with new BoardRent branch)
    └── Utilities/                  ← FLAT (PaM's name wins over BoardRent's "Utils")
```

---

## Task 1: Create the solution skeleton and root files

**Files:**
- Create: `BoardRentAndProperty/BoardRentAndProperty.sln`
- Create: `BoardRentAndProperty/App.config` (copied from PaM)
- Create: `BoardRentAndProperty/SE.ruleset` (copied from PaM)

- [ ] **Step 1.1: Create the merged folder**

```bash
mkdir -p /e/UBB-SE-2026-922-2/BoardRentAndProperty
```

- [ ] **Step 1.2: Create the empty solution file**

Run from the `BoardRentAndProperty/` folder:

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty && dotnet new sln -n BoardRentAndProperty
```

Expected: `The template "Solution File" was created successfully.` and `BoardRentAndProperty.sln` appears.

- [ ] **Step 1.3: Copy `App.config` and `SE.ruleset` to the solution root**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/App.config /e/UBB-SE-2026-922-2/BoardRentAndProperty/App.config
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/SE.ruleset /e/UBB-SE-2026-922-2/BoardRentAndProperty/SE.ruleset
```

- [ ] **Step 1.4: Verify `dotnet build` works on the empty solution**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln
```

Expected: `Build succeeded.` (no projects yet — build is trivially clean).

- [ ] **Step 1.5: Commit**

```bash
git add BoardRentAndProperty/
git commit -m "feat(merge): create BoardRentAndProperty solution skeleton with shared App.config and SE.ruleset"
```

---

## Task 2: Copy `ServerCommunication` and `NotificationServer` verbatim

**Files:**
- Create: `BoardRentAndProperty/ServerCommunication/` (full folder copy)
- Create: `BoardRentAndProperty/NotificationServer/` (full folder copy)
- Modify: `BoardRentAndProperty/BoardRentAndProperty.sln`

- [ ] **Step 2.1: Copy `ServerCommunication` folder**

```bash
cp -r /e/UBB-SE-2026-922-2/PropertyAndManagement/ServerCommunication /e/UBB-SE-2026-922-2/BoardRentAndProperty/ServerCommunication
```

- [ ] **Step 2.2: Copy `NotificationServer` folder**

```bash
cp -r /e/UBB-SE-2026-922-2/PropertyAndManagement/NotificationServer /e/UBB-SE-2026-922-2/BoardRentAndProperty/NotificationServer
```

- [ ] **Step 2.3: Add both csprojs to the solution**

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty && \
dotnet sln add ServerCommunication/ServerCommunication.csproj && \
dotnet sln add NotificationServer/NotificationServer.csproj
```

Expected: both `Project ... was added to the solution.`

- [ ] **Step 2.4: Verify build succeeds**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln
```

Expected: `Build succeeded.` Both `ServerCommunication.dll` and `NotificationServer.exe` produced.

- [ ] **Step 2.5: Commit**

```bash
git add BoardRentAndProperty/ServerCommunication/ BoardRentAndProperty/NotificationServer/ BoardRentAndProperty/BoardRentAndProperty.sln
git commit -m "feat(merge): copy ServerCommunication and NotificationServer projects verbatim"
```

---

## Task 3: Create the merged WinUI app csproj and stub `App.xaml(.cs)` / `MainWindow.xaml(.cs)`

**Files:**
- Create: `BoardRentAndProperty/BoardRentAndProperty/BoardRentAndProperty.csproj`
- Create: `BoardRentAndProperty/BoardRentAndProperty/App.xaml`
- Create: `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs` (stub — full merge in Task 9)
- Create: `BoardRentAndProperty/BoardRentAndProperty/MainWindow.xaml`
- Create: `BoardRentAndProperty/BoardRentAndProperty/MainWindow.xaml.cs`
- Create: `BoardRentAndProperty/BoardRentAndProperty/app.manifest`
- Create: `BoardRentAndProperty/BoardRentAndProperty/Package.appxmanifest`
- Create: `BoardRentAndProperty/BoardRentAndProperty/stylecop.json`
- Create: `BoardRentAndProperty/BoardRentAndProperty/Properties/launchSettings.json`

This task gets the WinUI app to **compile** with stub content. Real merged content comes in later tasks.

- [ ] **Step 3.1: Create the merged WinUI csproj file**

Path: `BoardRentAndProperty/BoardRentAndProperty/BoardRentAndProperty.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <AppxPackage>false</AppxPackage>
    <WindowsPackageType>None</WindowsPackageType>
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <Platforms>x86;x64</Platforms>
    <RootNamespace>BoardRentAndProperty</RootNamespace>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <UseWinUI>true</UseWinUI>
    <EnableMsixTooling>true</EnableMsixTooling>
    <Nullable>enable</Nullable>
    <CodeAnalysisRuleSet>..\SE.ruleset</CodeAnalysisRuleSet>
    <RunAnalyzersDuringBuild>false</RunAnalyzersDuringBuild>
    <RunAnalyzersDuringCompilation>false</RunAnalyzersDuringCompilation>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
    <PackageReference Include="CommunityToolkit.WinUI.UI.Controls" Version="7.1.2" />
    <PackageReference Include="CommunityToolkit.WinUI.UI.Controls.DataGrid" Version="7.1.2" />
    <PackageReference Include="H.NotifyIcon.WinUI" Version="2.3.2" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="7.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.5" />
    <PackageReference Include="Microsoft.Toolkit.Uwp.Notifications" Version="7.1.3" />
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.26100.7705" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.5" />
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ServerCommunication\ServerCommunication.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Include="..\App.config" Link="App.config">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <AdditionalFiles Include="..\SE.ruleset" />
    <AdditionalFiles Include="stylecop.json" />
    <Manifest Include="$(ApplicationManifest)" />
  </ItemGroup>

  <ItemGroup Condition="'$(DisableMsixProjectCapabilityAddedByProject)'!='true' and '$(EnableMsixTooling)'=='true'">
    <ProjectCapability Include="Msix" />
  </ItemGroup>

  <PropertyGroup Condition="'$(DisableHasPackageAndPublishMenuAddedByProject)'!='true' and '$(EnableMsixTooling)'=='true'">
    <HasPackageAndPublishMenu>true</HasPackageAndPublishMenu>
  </PropertyGroup>

  <PropertyGroup>
    <PublishReadyToRun Condition="'$(Configuration)' == 'Debug'">False</PublishReadyToRun>
    <PublishReadyToRun Condition="'$(Configuration)' != 'Debug'">True</PublishReadyToRun>
    <PublishTrimmed Condition="'$(Configuration)' == 'Debug'">False</PublishTrimmed>
    <PublishTrimmed Condition="'$(Configuration)' != 'Debug'">True</PublishTrimmed>
  </PropertyGroup>
</Project>
```

- [ ] **Step 3.2: Copy `app.manifest`, `Package.appxmanifest`, `stylecop.json`**

```bash
mkdir -p /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Properties
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/app.manifest /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/app.manifest
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/Package.appxmanifest /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Package.appxmanifest
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent/stylecop.json /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/stylecop.json
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/Properties/launchSettings.json /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Properties/launchSettings.json
```

- [ ] **Step 3.3: Update `Package.appxmanifest` identity**

Open `BoardRentAndProperty/BoardRentAndProperty/Package.appxmanifest` and replace the `<Identity Name="...">` value with `BoardRentAndProperty` (and the same name in `<Application Id="..." />` if PaM's was project-specific). Keep the publisher, version, and capabilities unchanged.

- [ ] **Step 3.4: Create stub `App.xaml`**

Path: `BoardRentAndProperty/BoardRentAndProperty/App.xaml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<Application
    x:Class="BoardRentAndProperty.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

- [ ] **Step 3.5: Create stub `App.xaml.cs`**

Path: `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`

```csharp
using System;
using Microsoft.UI.Xaml;

namespace BoardRentAndProperty
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
```

- [ ] **Step 3.6: Create stub `MainWindow.xaml`**

Path: `BoardRentAndProperty/BoardRentAndProperty/MainWindow.xaml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window
    x:Class="BoardRentAndProperty.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="BoardRentAndProperty">
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>
    <Grid />
</Window>
```

- [ ] **Step 3.7: Create stub `MainWindow.xaml.cs`**

Path: `BoardRentAndProperty/BoardRentAndProperty/MainWindow.xaml.cs`

```csharp
using Microsoft.UI.Xaml;

namespace BoardRentAndProperty
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
```

- [ ] **Step 3.8: Add the new csproj to the solution**

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty && \
dotnet sln add BoardRentAndProperty/BoardRentAndProperty.csproj
```

- [ ] **Step 3.9: Verify build succeeds**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln
```

Expected: `Build succeeded.` Output `BoardRentAndProperty.exe` produced under `BoardRentAndProperty/BoardRentAndProperty/bin/`.

- [ ] **Step 3.10: Commit**

```bash
git add BoardRentAndProperty/BoardRentAndProperty/ BoardRentAndProperty/BoardRentAndProperty.sln
git commit -m "feat(merge): scaffold merged WinUI app with stub App and MainWindow"
```

---

## Task 4: Copy PaM source files into the merged flat layout

This is mechanical: copy each PaM file to its new flat-layered home and update the namespace from `Property_and_Management.*` to `BoardRentAndProperty.*` (or specific sub-namespace per the merged layout). PaM's `src/Interface/` folder is dissolved — interfaces move next to their implementations in `Repositories/` and `Services/`.

**Files affected:** roughly 80 PaM source files (models, DTOs, mappers, repositories, services, view models, pages). Exact mapping is in the design spec section 4.

- [ ] **Step 4.1: Create the layered folder structure**

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty && \
mkdir -p Models DataTransferObjects Mappers Repositories Services Services/Listeners ViewModels Views Views/Helpers Data Utilities Constants Assets Scripts
```

- [ ] **Step 4.2: Copy PaM models to `Models/`**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Model/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Models/
```

After copy, open each file under `Models/` and change the namespace from `Property_and_Management.Src.Model` to `BoardRentAndProperty.Models`. (One namespace replacement per file.) Update `using` directives in any file that imports from this namespace.

- [ ] **Step 4.3: Copy PaM DTOs to `DataTransferObjects/`**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/DataTransferObjects/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/DataTransferObjects/
```

Update each file's namespace from `Property_and_Management.Src.DataTransferObjects` to `BoardRentAndProperty.DataTransferObjects`.

- [ ] **Step 4.4: Copy PaM mappers to `Mappers/`**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Mapper/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Mappers/
```

Update namespace from `Property_and_Management.Src.Mapper` to `BoardRentAndProperty.Mappers`.

- [ ] **Step 4.5: Copy PaM repositories AND interfaces to `Repositories/`**

```bash
# Implementations:
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Repository/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/

# Interfaces (currently in src/Interface/, dissolved into Repositories/):
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IUserRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IGameRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IRequestRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IRentalRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/INotificationRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/
```

For each implementation file: namespace from `Property_and_Management.Src.Repository` → `BoardRentAndProperty.Repositories`. For each interface file: namespace from `Property_and_Management.Src.Interface` → `BoardRentAndProperty.Repositories`.

- [ ] **Step 4.6: Copy PaM services AND interfaces to `Services/`**

```bash
# Service implementations:
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Service/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/

# Listeners subfolder:
cp -r /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Service/Listeners/* \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/Listeners/

# Service interfaces (from src/Interface/, except IUser/IGame/IRequest/IRental/INotificationRepository which we already moved):
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IUserService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IGameService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IRequestService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IRentalService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/INotificationService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IToastNotificationService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IServerClient.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Interface/IMapper.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Mappers/
```

For each service file: namespace from `Property_and_Management.Src.Service` (or `.Listeners`) → `BoardRentAndProperty.Services` (or `.Listeners`). For each interface file: namespace from `Property_and_Management.Src.Interface` → `BoardRentAndProperty.Services` (or `BoardRentAndProperty.Mappers` for `IMapper.cs`).

After this step the `src/Interface/` folder has no surviving destination — its files have all been distributed inline.

- [ ] **Step 4.7: Copy PaM view models to `ViewModels/`**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Viewmodels/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/ViewModels/
```

Namespace: `Property_and_Management.Src.Viewmodels` → `BoardRentAndProperty.ViewModels`.

- [ ] **Step 4.8: Copy PaM views to `Views/`**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Views/*.xaml \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Views/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Views/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Views/
```

For each `.xaml`: update `x:Class="Property_and_Management.Src.Views.XxxView"` → `x:Class="BoardRentAndProperty.Views.XxxView"`. For each `.xaml.cs`: namespace `Property_and_Management.Src.Views` → `BoardRentAndProperty.Views`.

**Note on `MenuBarView` rename:** the file is `MenuBarPage.xaml` but the class inside is `MenuBarView`. Per the design, rename the class to `MenuBarPage` (XAML `x:Class="BoardRentAndProperty.Views.MenuBarPage"` and code-behind class name) so the file and class names match. Update the one consumer (`App.xaml.cs` will navigate to it in Task 9).

- [ ] **Step 4.9: Copy PaM helpers (`DialogHelper.cs`, `ImageFailureHandler.cs`) to `Views/`**

The previous step already pulled them. Verify their namespaces are updated to `BoardRentAndProperty.Views`.

- [ ] **Step 4.10: Copy PaM utilities to `Utilities/`**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Utilities/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Utilities/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/CurrentUserContext.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Utilities/
```

For each file: namespace updated to `BoardRentAndProperty.Utilities`.

- [ ] **Step 4.11: Copy PaM constants to `Constants/`**

```bash
cp -r /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/src/Constants/* \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Constants/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/Constants.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Constants.cs
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/ConstantsBridge.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/ConstantsBridge.cs
```

Namespaces: `Property_and_Management.Src.Constants` → `BoardRentAndProperty.Constants` and root-level `Property_and_Management` → `BoardRentAndProperty`.

- [ ] **Step 4.12: Copy PaM Scripts and Assets**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/Scripts/* \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Scripts/
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/Assets/* \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Assets/
```

(SQL scripts unchanged. Assets are PaM's tray_icon and default-game-placeholder.)

- [ ] **Step 4.13: Copy `Constants.cs`, `ConstantsBridge.cs`, `NotificationManager.cs`, `InternalsVisibleTo.cs`**

```bash
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/NotificationManager.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/NotificationManager.cs
cp /e/UBB-SE-2026-922-2/PropertyAndManagement/Property_and_Management/InternalsVisibleTo.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/InternalsVisibleTo.cs
```

Update namespaces. `InternalsVisibleTo.cs` uses `[assembly: InternalsVisibleTo("Property_and_Management.Tests")]` — change to `[assembly: InternalsVisibleTo("BoardRentAndProperty.Tests")]` (even though tests are out of scope, keep the placeholder for the future).

- [ ] **Step 4.14: Bulk update remaining `using Property_and_Management.*` statements**

Search-and-replace across all `BoardRentAndProperty/BoardRentAndProperty/**/*.cs` and `**/*.xaml`:
- `using Property_and_Management.Src.Model` → `using BoardRentAndProperty.Models`
- `using Property_and_Management.Src.DataTransferObjects` → `using BoardRentAndProperty.DataTransferObjects`
- `using Property_and_Management.Src.Mapper` → `using BoardRentAndProperty.Mappers`
- `using Property_and_Management.Src.Repository` → `using BoardRentAndProperty.Repositories`
- `using Property_and_Management.Src.Interface` → `using BoardRentAndProperty.Repositories;\nusing BoardRentAndProperty.Services;\nusing BoardRentAndProperty.Mappers` (the safest is to add all three; remove any `Interface` directive unused — the analyzer will flag it but it's not an error)
- `using Property_and_Management.Src.Service` → `using BoardRentAndProperty.Services`
- `using Property_and_Management.Src.Service.Listeners` → `using BoardRentAndProperty.Services.Listeners`
- `using Property_and_Management.Src.Utilities` → `using BoardRentAndProperty.Utilities`
- `using Property_and_Management.Src.Viewmodels` → `using BoardRentAndProperty.ViewModels`
- `using Property_and_Management.Src.Views` → `using BoardRentAndProperty.Views`
- `using Property_and_Management.Src.Constants` → `using BoardRentAndProperty.Constants`
- `using Property_and_Management;` → `using BoardRentAndProperty;`
- `using Property_and_Management.Src;` → `using BoardRentAndProperty;`
- `xmlns:vm="using:Property_and_Management.Src.Viewmodels"` → `xmlns:vm="using:BoardRentAndProperty.ViewModels"`
- All XAML `xmlns:` declarations referencing `using:Property_and_Management.*` → `using:BoardRentAndProperty.*` (corresponding folder).

Recommended approach (PowerShell or sed) for the bulk pass:

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty
find . -name "*.cs" -o -name "*.xaml" | xargs sed -i \
  -e 's|Property_and_Management\.Src\.Model|BoardRentAndProperty.Models|g' \
  -e 's|Property_and_Management\.Src\.DataTransferObjects|BoardRentAndProperty.DataTransferObjects|g' \
  -e 's|Property_and_Management\.Src\.Mapper|BoardRentAndProperty.Mappers|g' \
  -e 's|Property_and_Management\.Src\.Repository|BoardRentAndProperty.Repositories|g' \
  -e 's|Property_and_Management\.Src\.Service\.Listeners|BoardRentAndProperty.Services.Listeners|g' \
  -e 's|Property_and_Management\.Src\.Service|BoardRentAndProperty.Services|g' \
  -e 's|Property_and_Management\.Src\.Utilities|BoardRentAndProperty.Utilities|g' \
  -e 's|Property_and_Management\.Src\.Viewmodels|BoardRentAndProperty.ViewModels|g' \
  -e 's|Property_and_Management\.Src\.Views|BoardRentAndProperty.Views|g' \
  -e 's|Property_and_Management\.Src\.Constants|BoardRentAndProperty.Constants|g' \
  -e 's|Property_and_Management\.Src\.Interface|BoardRentAndProperty.Services|g' \
  -e 's|Property_and_Management\.Src;|BoardRentAndProperty;|g' \
  -e 's|Property_and_Management\.Src\.|BoardRentAndProperty.|g' \
  -e 's|Property_and_Management;|BoardRentAndProperty;|g' \
  -e 's|namespace Property_and_Management\.Src|namespace BoardRentAndProperty|g' \
  -e 's|namespace Property_and_Management|namespace BoardRentAndProperty|g'
```

The `Interface` namespace mapping is intentionally pointed at `Services` — most ex-`Interface` consumers were in service files. Cases where the consumer needs `Repositories` will surface as compile errors that you fix by adding `using BoardRentAndProperty.Repositories;`.

- [ ] **Step 4.15: Try a build to surface remaining issues**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln 2>&1 | head -200
```

Expected: build will FAIL with multiple errors. Common ones:
- `using BoardRent.*` references in BoardRent files we haven't copied yet — ignore for now (Task 5 brings BoardRent in).
- Missing `BoardRent.Core/Data/AppDbContext` — ignore for now.
- Missing `IUnitOfWork` etc. — ignore for now.
- Missing `using BoardRentAndProperty.Repositories;` on a few PaM files — fix those by adding the using.

Fix only the PaM-side errors. BoardRent-side errors will resolve in Task 5.

- [ ] **Step 4.16: Add `Page Update` entries to the merged csproj for every PaM `.xaml`**

Open `BoardRentAndProperty.csproj` and add inside a new `<ItemGroup>`:

```xml
<ItemGroup>
  <Page Update="Views\MenuBarPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\ListingsPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\CreateGameView.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\EditGameView.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\NotificationsPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\RequestsFromOthersPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\RequestsToOthersPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\CreateRequestView.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\RentalsFromOthersPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\RentalsToOthersPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
  <Page Update="Views\CreateRentalView.xaml"><Generator>MSBuild:Compile</Generator></Page>
</ItemGroup>
<ItemGroup>
  <Content Update="Assets\default-game-placeholder.jpg"><CopyToOutputDirectory>Always</CopyToOutputDirectory></Content>
  <Content Update="Assets\tray_icon.ico"><CopyToOutputDirectory>Always</CopyToOutputDirectory></Content>
</ItemGroup>
```

- [ ] **Step 4.17: Commit (PaM-side files merged, build still failing on missing BoardRent — that's expected)**

```bash
git add BoardRentAndProperty/BoardRentAndProperty/
git commit -m "feat(merge): copy PaM source into merged flat layout with namespace updates"
```

---

## Task 5: Copy and rename BoardRent source files (User → Account pass)

This is the rename pass. BoardRent's `BoardRent.Core/` and `BoardRent/` source merges into the same flat folders, with the renamed types per the design spec section 5.

**Class renames:**
- `User` → `Account`
- `UserRole` → `AccountRole`
- `IUserRepository` → `IAccountRepository`
- `UserRepository` → `AccountRepository`
- `IUserService` → `IAccountService`
- `UserService` → `AccountService`
- `UserProfileDataTransferObject` → `AccountProfileDataTransferObject`

**FK property renames:**
- `AccountRole.UserId` → `AccountRole.AccountId`
- `FailedLoginAttempt.UserId` → `FailedLoginAttempt.AccountId`

- [ ] **Step 5.1: Copy BoardRent.Core Domain → `Models/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Domain/User.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Models/Account.cs
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Domain/UserRole.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Models/AccountRole.cs
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Domain/Role.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Models/Role.cs
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Domain/FailedLoginAttempt.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Models/FailedLoginAttempt.cs
```

Open `Models/Account.cs` and:
- Rename `class User` → `class Account`
- Update namespace `BoardRent.Domain` → `BoardRentAndProperty.Models`

Open `Models/AccountRole.cs` and:
- Rename `class UserRole` → `class AccountRole`
- Rename property `Guid UserId` → `Guid AccountId`
- Update namespace

Open `Models/Role.cs`: only update namespace.

Open `Models/FailedLoginAttempt.cs`:
- Rename property `Guid UserId` → `Guid AccountId`
- Update namespace

- [ ] **Step 5.2: Copy BoardRent.Core DTOs → `DataTransferObjects/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/DataTransferObjects/UserProfileDataTransferObject.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/DataTransferObjects/AccountProfileDataTransferObject.cs

cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/DataTransferObjects/ChangePasswordDataTransferObject.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/DataTransferObjects/LoginDataTransferObject.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/DataTransferObjects/RegisterDataTransferObject.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/DataTransferObjects/RoleDataTransferObject.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/DataTransferObjects/UpdateProfileDataTransferObject.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/DataTransferObjects/
```

In `AccountProfileDataTransferObject.cs`:
- Rename `class UserProfileDataTransferObject` → `class AccountProfileDataTransferObject`
- Update namespace `BoardRent.DataTransferObjects` → `BoardRentAndProperty.DataTransferObjects`
- If the class has `Guid UserId` field rename to `AccountId`; if it has a `User` reference rename to `Account`.

For other DTOs: only update namespace.

- [ ] **Step 5.3: Copy BoardRent.Core Data → `Data/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Data/AppDbContext.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Data/IUnitOfWork.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Data/IUnitOfWorkFactory.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Data/UnitOfWork.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Data/UnitOfWorkFactory.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Data/
```

For each file: namespace `BoardRent.Data` → `BoardRentAndProperty.Data`. Update `using BoardRent.Domain;` → `using BoardRentAndProperty.Models;`.

- [ ] **Step 5.4: Update `Data/AppDbContext.cs` SQL strings (rename pass)**

Open `BoardRentAndProperty/BoardRentAndProperty/Data/AppDbContext.cs` and replace SQL strings inside `EnsureCreated()`:

- `IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'User')` → `WHERE name = 'Account'`
- `CREATE TABLE [User] (` → `CREATE TABLE [Account] (`
- `IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')` → `WHERE name = 'AccountRoles'`
- `CREATE TABLE UserRoles (` → `CREATE TABLE AccountRoles (`
- `UserId UNIQUEIDENTIFIER NOT NULL,` (in AccountRoles) → `AccountId UNIQUEIDENTIFIER NOT NULL,`
- `PRIMARY KEY (UserId, RoleId),` → `PRIMARY KEY (AccountId, RoleId),`
- `FOREIGN KEY (UserId) REFERENCES [User](Id)` → `FOREIGN KEY (AccountId) REFERENCES [Account](Id)`
- `IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FailedLoginAttempt')` — unchanged
- Inside `CREATE TABLE FailedLoginAttempt`: `UserId UNIQUEIDENTIFIER PRIMARY KEY,` → `AccountId UNIQUEIDENTIFIER PRIMARY KEY,`
- `FOREIGN KEY (UserId) REFERENCES [User](Id)` (in FailedLoginAttempt) → `FOREIGN KEY (AccountId) REFERENCES [Account](Id)`
- Seed admin INSERT: `INSERT INTO [User] ...` → `INSERT INTO [Account] ...`
- Seed admin role link: `INSERT INTO UserRoles (UserId, RoleId) VALUES (@adminId, @adminRoleId)` → `INSERT INTO AccountRoles (AccountId, RoleId) VALUES (@adminId, @adminRoleId)`
- `IF NOT EXISTS (SELECT * FROM [User] WHERE Username = 'admin')` → `WHERE name from [Account] WHERE Username = 'admin'`

(The two `Role`-table blocks and the `INSERT INTO Role` lines stay unchanged — only `User`/`UserRole` patterns rename.)

- [ ] **Step 5.5: Copy BoardRent.Core Repositories with rename → `Repositories/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Repositories/IUserRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/IAccountRepository.cs
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Repositories/UserRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/AccountRepository.cs
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Repositories/IFailedLoginRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Repositories/FailedLoginRepository.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/
```

Open `IAccountRepository.cs`:
- Rename `interface IUserRepository` → `interface IAccountRepository`
- Update namespace `BoardRent.Repositories` → `BoardRentAndProperty.Repositories`
- Replace every parameter type and return type `User` → `Account`, `UserRole` → `AccountRole`
- Update `using BoardRent.Domain;` → `using BoardRentAndProperty.Models;`

Open `AccountRepository.cs`:
- Rename `class UserRepository : IUserRepository` → `class AccountRepository : IAccountRepository`
- Update namespace
- Replace every `User` reference (variable, type, SQL parameter name) → `Account`
- Replace SQL: `[User]` → `[Account]`, `UserRoles` → `AccountRoles`, FK column `UserId` → `AccountId` in any inline SQL
- `using BoardRent.Domain;` → `using BoardRentAndProperty.Models;`

`IFailedLoginRepository.cs`, `FailedLoginRepository.cs`:
- Update namespace and using directives
- In any signature/SQL, `UserId` → `AccountId`

- [ ] **Step 5.6: Copy BoardRent.Core Services with rename → `Services/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Services/AdminService.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Services/AuthService.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Services/IAdminService.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Services/IAuthService.cs \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Services/IFilePickerService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/

cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Services/IUserService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/IAccountService.cs
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Services/UserService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs
```

In `IAccountService.cs`:
- Rename `interface IUserService` → `interface IAccountService`
- Replace every `User` type / parameter → `Account`
- Replace every `UserProfileDataTransferObject` → `AccountProfileDataTransferObject`
- Update namespace and usings

In `AccountService.cs`:
- Rename `class UserService : IUserService` → `class AccountService : IAccountService`
- Same type/parameter renames as above
- Constructor argument names: `IUserRepository` → `IAccountRepository`
- Update namespace and usings

In `AuthService.cs`, `AdminService.cs`, `IAuthService.cs`, `IAdminService.cs`, `IFilePickerService.cs`:
- Update namespace `BoardRent.Services` → `BoardRentAndProperty.Services`
- Update usings: `BoardRent.Domain` → `BoardRentAndProperty.Models`, `BoardRent.Repositories` → `BoardRentAndProperty.Repositories`, `BoardRent.DataTransferObjects` → `BoardRentAndProperty.DataTransferObjects`, `BoardRent.Utils` → `BoardRentAndProperty.Utilities`
- Replace every `User` → `Account` (parameters, variables, return types)
- Replace every `IUserRepository` → `IAccountRepository`
- Replace every `UserProfileDataTransferObject` → `AccountProfileDataTransferObject`

- [ ] **Step 5.7: Copy BoardRent.Core ViewModels → `ViewModels/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/ViewModels/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/ViewModels/
```

For each VM:
- Namespace `BoardRent.ViewModels` → `BoardRentAndProperty.ViewModels`
- Usings: `BoardRent.Domain` → `BoardRentAndProperty.Models`, `BoardRent.Services` → `BoardRentAndProperty.Services`, `BoardRent.Repositories` → `BoardRentAndProperty.Repositories`, `BoardRent.DataTransferObjects` → `BoardRentAndProperty.DataTransferObjects`, `BoardRent.Utils` → `BoardRentAndProperty.Utilities`, `BoardRent.Views` → `BoardRentAndProperty.Views`
- Type references: `User` → `Account`, `IUserRepository` → `IAccountRepository`, `IUserService` → `IAccountService` (where applicable)
- `App.NavigateTo(typeof(LoginPage))` etc. — these are calls to the merged `App` static helper. Leave as-is; the helper is added in Task 9.

- [ ] **Step 5.8: Copy BoardRent.Core Utils → `Utilities/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent.Core/Utils/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Utilities/
```

Update namespace `BoardRent.Utils` → `BoardRentAndProperty.Utilities`. PaM's `Utilities/` already has files like `CurrentUserContext.cs` from Task 4 — they coexist (different file names, no collision).

- [ ] **Step 5.9: Copy BoardRent app `Views/` and `Services/`**

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent/Views/*.xaml \
   /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent/Views/*.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Views/

cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent/Services/FilePickerService.cs \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/
```

For each `.xaml`: `x:Class="BoardRent.Views.LoginPage"` → `x:Class="BoardRentAndProperty.Views.LoginPage"`. Same for `RegisterPage`, `ProfilePage`, `AdminPage`. Update any `xmlns:` declarations referring to `using:BoardRent.*`.

For each `.xaml.cs`: namespace `BoardRent.Views` → `BoardRentAndProperty.Views`. Replace usings `BoardRent.*` → `BoardRentAndProperty.*` corresponding folder.

For `FilePickerService.cs`: namespace `BoardRent.Services` → `BoardRentAndProperty.Services`. Update usings.

- [ ] **Step 5.10: Add `Page Update` entries for BoardRent views**

In `BoardRentAndProperty.csproj`, append to the existing `<ItemGroup>` with `Page Update` entries:

```xml
<Page Update="Views\LoginPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
<Page Update="Views\RegisterPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
<Page Update="Views\ProfilePage.xaml"><Generator>MSBuild:Compile</Generator></Page>
<Page Update="Views\AdminPage.xaml"><Generator>MSBuild:Compile</Generator></Page>
```

Add asset content entries for BoardRent's logos:

```xml
<ItemGroup>
  <Content Include="Assets\SplashScreen.scale-200.png" />
  <Content Include="Assets\LockScreenLogo.scale-200.png" />
  <Content Include="Assets\Square150x150Logo.scale-200.png" />
  <Content Include="Assets\Square44x44Logo.scale-200.png" />
  <Content Include="Assets\Square44x44Logo.targetsize-24_altform-unplated.png" />
  <Content Include="Assets\StoreLogo.png" />
  <Content Include="Assets\Wide310x150Logo.scale-200.png" />
</ItemGroup>
```

Copy BoardRent's assets into the merged `Assets/` folder (PaM's are already there from Task 4):

```bash
cp /e/UBB-SE-2026-922-2/BoardRent_A1+A2/BoardRent/Assets/*.png \
   /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Assets/
```

- [ ] **Step 5.11: Bulk-replace remaining `BoardRent.*` namespace references**

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty
find . -name "*.cs" -o -name "*.xaml" | xargs sed -i \
  -e 's|BoardRent\.Domain|BoardRentAndProperty.Models|g' \
  -e 's|BoardRent\.DataTransferObjects|BoardRentAndProperty.DataTransferObjects|g' \
  -e 's|BoardRent\.Repositories|BoardRentAndProperty.Repositories|g' \
  -e 's|BoardRent\.Services|BoardRentAndProperty.Services|g' \
  -e 's|BoardRent\.Utils|BoardRentAndProperty.Utilities|g' \
  -e 's|BoardRent\.ViewModels|BoardRentAndProperty.ViewModels|g' \
  -e 's|BoardRent\.Views|BoardRentAndProperty.Views|g' \
  -e 's|BoardRent\.Data|BoardRentAndProperty.Data|g' \
  -e 's|namespace BoardRent\.|namespace BoardRentAndProperty.|g' \
  -e 's|namespace BoardRent$|namespace BoardRentAndProperty|g'
```

This catches any namespace references that survived the manual edits in 5.1–5.10. After this pass, no file should have `BoardRent.*` (the old root) anywhere.

- [ ] **Step 5.12: Bulk-replace remaining `User` ↔ `Account` references in BoardRent files**

This is the riskier rename — `User` is a common word. The safe approach is targeted at known patterns inside files that came from BoardRent (which we just moved):

For each file under `Models/Account.cs`, `Models/AccountRole.cs`, `Models/FailedLoginAttempt.cs`, `Repositories/IAccountRepository.cs`, `Repositories/AccountRepository.cs`, `Repositories/IFailedLoginRepository.cs`, `Repositories/FailedLoginRepository.cs`, `Services/IAccountService.cs`, `Services/AccountService.cs`, `Services/IAuthService.cs`, `Services/AuthService.cs`, `Services/IAdminService.cs`, `Services/AdminService.cs`, `DataTransferObjects/AccountProfileDataTransferObject.cs`, `Utilities/SessionContext.cs`, `Utilities/ISessionContext.cs`, and the BoardRent view models (`LoginViewModel.cs`, `RegisterViewModel.cs`, `ProfileViewModel.cs`, `AdminViewModel.cs`):

Run these sed substitutions (case-sensitive):

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty
for f in \
  Models/Account.cs \
  Models/AccountRole.cs \
  Models/FailedLoginAttempt.cs \
  Repositories/IAccountRepository.cs \
  Repositories/AccountRepository.cs \
  Repositories/IFailedLoginRepository.cs \
  Repositories/FailedLoginRepository.cs \
  Services/IAccountService.cs \
  Services/AccountService.cs \
  Services/IAuthService.cs \
  Services/AuthService.cs \
  Services/IAdminService.cs \
  Services/AdminService.cs \
  DataTransferObjects/AccountProfileDataTransferObject.cs \
  Utilities/SessionContext.cs \
  Utilities/ISessionContext.cs \
  ViewModels/LoginViewModel.cs \
  ViewModels/RegisterViewModel.cs \
  ViewModels/ProfileViewModel.cs \
  ViewModels/AdminViewModel.cs ; do
  if [ -f "$f" ]; then
    sed -i \
      -e 's|UserProfileDataTransferObject|AccountProfileDataTransferObject|g' \
      -e 's|IUserRepository|IAccountRepository|g' \
      -e 's|UserRepository|AccountRepository|g' \
      -e 's|IUserService|IAccountService|g' \
      -e 's|UserService|AccountService|g' \
      "$f"
  fi
done
```

Note: this does **not** rename the type `User` itself — there are too many false positives ("User" inside string literals like `"Username"`, comment text, etc.). The `User → Account` class rename was already done by hand in steps 5.1–5.6 inside the type declarations. After 5.12, find any remaining bare `User` references in BoardRent files and update them manually:

```bash
grep -rn "\\bUser\\b" /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Models/Account.cs \
                       /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/AccountRepository.cs \
                       /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs
```

Replace any remaining `User` type references (NOT string literals, NOT identifier prefixes like `Username`) with `Account`.

- [ ] **Step 5.13: Try the build, fix remaining errors**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln 2>&1 | head -200
```

Expected errors and fixes:
- `'IUserRepository' could not be found` in a BoardRent file → add `using BoardRentAndProperty.Repositories;`
- `'User' could not be found` → check the file is in the rename list above; if so, manually swap the missed reference to `Account`.
- `'IUserService' is not defined` in PaM file (the `CreateRentalViewModel`) → this is PaM's `IUserService` (different from BoardRent's now-renamed one). It still exists in PaM's namespace. Verify the PaM `IUserService` interface and `UserService` class are still at `Services/IUserService.cs` and `Services/UserService.cs` and refer to PaM's user concept. They were copied in Task 4 step 4.6 — they should still be there. Confirm by inspecting the file.
- `'NavigateTo' is not defined on App` in BoardRent VMs → leave for now; resolved in Task 9.
- `'AppDbContext' could not be found` in BoardRent files → add `using BoardRentAndProperty.Data;`.

Iterate until the only remaining errors are `App.NavigateTo` / `App.NavigateBack` / `App.Services` references (Task 9 fixes those).

- [ ] **Step 5.14: Commit**

```bash
git add BoardRentAndProperty/BoardRentAndProperty/
git commit -m "feat(merge): copy BoardRent source with User→Account rename and namespace updates"
```

---

## Task 6: Add the `AccountMapper` and `AccountProfileMapper` (uniformity rule)

The design's uniformity rule says: PaM has explicit mappers, BoardRent does inline mapping inside services — add explicit mappers for BoardRent's account domain too.

**Files:**
- Create: `BoardRentAndProperty/BoardRentAndProperty/Mappers/AccountMapper.cs`
- Create: `BoardRentAndProperty/BoardRentAndProperty/Mappers/AccountProfileMapper.cs`
- Modify: `BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs` (extract inline mapping)

- [ ] **Step 6.1: Inspect `AccountService.cs` to find where it builds `Account` from a row reader and where it builds `AccountProfileDataTransferObject` from `Account`**

```bash
grep -n "new Account\\|new AccountProfileDataTransferObject\\|reader\\[" \
  /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs \
  /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty/Repositories/AccountRepository.cs
```

Identify the existing inline mapping logic.

- [ ] **Step 6.2: Create `AccountMapper.cs`**

Path: `BoardRentAndProperty/BoardRentAndProperty/Mappers/AccountMapper.cs`

```csharp
using System;
using System.Data;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class AccountMapper
    {
        public Account FromReader(IDataRecord reader)
        {
            return new Account
            {
                Id = (Guid)reader["Id"],
                Username = (string)reader["Username"],
                DisplayName = (string)reader["DisplayName"],
                Email = (string)reader["Email"],
                PasswordHash = (string)reader["PasswordHash"],
                PhoneNumber = reader["PhoneNumber"] as string,
                AvatarUrl = reader["AvatarUrl"] as string,
                IsSuspended = (bool)reader["IsSuspended"],
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"],
                StreetName = reader["StreetName"] as string,
                StreetNumber = reader["StreetNumber"] as string,
                Country = reader["Country"] as string,
                City = reader["City"] as string,
            };
        }
    }
}
```

(The exact column list comes from `Account` class — match its fields.)

- [ ] **Step 6.3: Create `AccountProfileMapper.cs`**

Path: `BoardRentAndProperty/BoardRentAndProperty/Mappers/AccountProfileMapper.cs`

```csharp
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Mappers
{
    public class AccountProfileMapper
    {
        public AccountProfileDataTransferObject ToDto(Account account)
        {
            return new AccountProfileDataTransferObject
            {
                AccountId = account.Id,
                Username = account.Username,
                DisplayName = account.DisplayName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                AvatarUrl = account.AvatarUrl,
                Country = account.Country,
                City = account.City,
                StreetName = account.StreetName,
                StreetNumber = account.StreetNumber,
            };
        }

        public void ApplyTo(Account account, AccountProfileDataTransferObject dto)
        {
            account.DisplayName = dto.DisplayName;
            account.Email = dto.Email;
            account.PhoneNumber = dto.PhoneNumber;
            account.AvatarUrl = dto.AvatarUrl;
            account.Country = dto.Country;
            account.City = dto.City;
            account.StreetName = dto.StreetName;
            account.StreetNumber = dto.StreetNumber;
        }
    }
}
```

(Match the property set on `AccountProfileDataTransferObject` exactly — adjust field names if the DTO uses different ones.)

- [ ] **Step 6.4: Refactor `AccountService.cs` and `AccountRepository.cs` to use the mappers**

In `AccountRepository.cs`: replace inline `new Account { ... = reader["..."] }` blocks with a call to `accountMapper.FromReader(reader)`. Inject `AccountMapper` via constructor. Same for any place `AccountRepository` constructs `Account` objects.

In `AccountService.cs`: replace any inline `new AccountProfileDataTransferObject { ... }` with `accountProfileMapper.ToDto(account)`. Replace any inline "apply DTO to entity" logic with `accountProfileMapper.ApplyTo(account, dto)`. Inject `AccountProfileMapper` (and possibly `AccountMapper`) via constructor.

- [ ] **Step 6.5: Verify build succeeds (some errors expected from `App.NavigateTo` etc., resolved in Task 9)**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln 2>&1 | grep -E "error CS|Build succeeded" | head -20
```

Expected: no new errors introduced. The pre-existing `App.NavigateTo` errors remain — they get resolved in Task 9.

- [ ] **Step 6.6: Commit**

```bash
git add BoardRentAndProperty/BoardRentAndProperty/Mappers/ BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs BoardRentAndProperty/BoardRentAndProperty/Repositories/AccountRepository.cs
git commit -m "feat(merge): introduce AccountMapper and AccountProfileMapper (uniformity rule)"
```

---

## Task 7: Add the BoardRent menu entry (`AppPage`, `MenuBarViewModel`, `MenuBarPage`)

**Files:**
- Modify: `BoardRentAndProperty/BoardRentAndProperty/ViewModels/AppPage.cs`
- Modify: `BoardRentAndProperty/BoardRentAndProperty/ViewModels/MenuBarViewModel.cs`
- Modify: `BoardRentAndProperty/BoardRentAndProperty/Views/MenuBarPage.xaml.cs`

- [ ] **Step 7.1: Add `BoardRent` to `AppPage` enum**

Open `BoardRentAndProperty/BoardRentAndProperty/ViewModels/AppPage.cs`. Add the new value:

```csharp
namespace BoardRentAndProperty.ViewModels
{
    public enum AppPage
    {
        Listings,
        RequestsFromOthers,
        RentalsFromOthers,
        RequestsToOthers,
        RentalsToOthers,
        Notifications,
        BoardRent
    }
}
```

- [ ] **Step 7.2: Add `BoardRent` to `MenuBarViewModel.NavigationActionsByMenuLabel`**

Open `BoardRentAndProperty/BoardRentAndProperty/ViewModels/MenuBarViewModel.cs`. Add the entry as the **last** key in the dictionary so it appears at the bottom of the side ListView:

```csharp
NavigationActionsByMenuLabel = new Dictionary<string, Action>
{
    { "My Games",           () => RequestNavigation?.Invoke(AppPage.Listings) },
    { "Others' Requests",   () => RequestNavigation?.Invoke(AppPage.RequestsFromOthers) },
    { "Others' Rentals",    () => RequestNavigation?.Invoke(AppPage.RentalsToOthers) },
    { "My Requests",        () => RequestNavigation?.Invoke(AppPage.RequestsToOthers) },
    { "My Rentals",         () => RequestNavigation?.Invoke(AppPage.RentalsFromOthers) },
    { "Notifications",      () => RequestNavigation?.Invoke(AppPage.Notifications) },
    { "BoardRent",          () => RequestNavigation?.Invoke(AppPage.BoardRent) }
};
```

- [ ] **Step 7.3: Add the `BoardRent` branch to `MenuBarPage.OnViewModelRequestedNavigation`**

Open `BoardRentAndProperty/BoardRentAndProperty/Views/MenuBarPage.xaml.cs`. Modify the navigation handler:

```csharp
private void OnViewModelRequestedNavigation(AppPage page)
{
    if (page == AppPage.BoardRent)
    {
        App.NavigateTo(typeof(LoginPage));
        return;
    }

    if (!PageTypeMap.TryGetValue(page, out var pageType))
    {
        return;
    }

    ContentFrame.Navigate(pageType, injectedGameService);
}
```

Add `using BoardRentAndProperty.Views;` if not already present (so `LoginPage` resolves).

- [ ] **Step 7.4: Add `MenuBarPage.OnNavigatedTo` reset of `SelectedPageName`**

Inside the same `MenuBarPage.xaml.cs`, modify `OnNavigatedTo` to reset the singleton VM's selected state so coming back from BoardRent lands on My Games cleanly:

```csharp
protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
{
    base.OnNavigatedTo(navigationEventArgs);

    if (navigationEventArgs.Parameter is IGameService gameService)
    {
        injectedGameService = gameService;
    }

    // After returning from BoardRent area, reset to My Games to avoid the singleton VM
    // showing "BoardRent" as selected with no content rendered.
    if (ViewModel.SelectedPageName == "BoardRent" || string.IsNullOrEmpty(ViewModel.SelectedPageName))
    {
        ViewModel.SelectedPageName = "My Games";
        ContentFrame.Navigate(typeof(ListingsPage), injectedGameService);
    }
}
```

- [ ] **Step 7.5: Verify build still has only the expected `App.NavigateTo` errors**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln 2>&1 | grep "error CS" | head -10
```

The errors should reference `App.NavigateTo` — these resolve in Task 9.

- [ ] **Step 7.6: Commit**

```bash
git add BoardRentAndProperty/BoardRentAndProperty/ViewModels/AppPage.cs \
       BoardRentAndProperty/BoardRentAndProperty/ViewModels/MenuBarViewModel.cs \
       BoardRentAndProperty/BoardRentAndProperty/Views/MenuBarPage.xaml.cs
git commit -m "feat(merge): add BoardRent entry to MenuBar with branch to outer-frame navigation"
```

---

## Task 8: Add the back button on `LoginPage`

**Files:**
- Modify: `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml`
- Modify: `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs`

- [ ] **Step 8.1: Add the back button at the top of the LoginPage XAML**

Open `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml`. Locate the outer `<Grid>` and prepend a header-area row with a HyperlinkButton:

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <HyperlinkButton Grid.Row="0"
                     Content="← Back to Property &amp; Management"
                     Click="OnBackToPropertyClicked"
                     Margin="20,20,0,0"
                     HorizontalAlignment="Left" />

    <StackPanel Grid.Row="1" HorizontalAlignment="Center" VerticalAlignment="Center" Width="350" Spacing="15">
        <!-- existing login form contents (App Logo, Sign in heading, ErrorMessage, etc.) -->
    </StackPanel>

    <ContentDialog x:Name="ResetPasswordDialog" Grid.RowSpan="2"
                   Title="Password Reset"
                   CloseButtonText="Close"
                   DefaultButton="Close">
        <TextBlock Text="Please contact the Administrator at admin@boardrent.com to reset your password." TextWrapping="Wrap"/>
    </ContentDialog>
</Grid>
```

(Move the existing inner `StackPanel` into `Grid.Row="1"`. Keep all existing form children unchanged.)

- [ ] **Step 8.2: Add the click handler in `LoginPage.xaml.cs`**

Open `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs`. Add the handler method:

```csharp
private void OnBackToPropertyClicked(object sender, RoutedEventArgs e)
{
    var gameService = App.Services.GetRequiredService<Services.IGameService>();
    App.NavigateTo(typeof(MenuBarPage), gameService);
}
```

Add the necessary usings:

```csharp
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
```

- [ ] **Step 8.3: Verify build still only complains about `App.NavigateTo` / `App.Services` (Task 9 fixes them)**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln 2>&1 | grep "error CS" | head
```

- [ ] **Step 8.4: Commit**

```bash
git add BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml \
       BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs
git commit -m "feat(merge): add 'Back to Property & Management' button to LoginPage"
```

---

## Task 9: Merge `App.xaml.cs` (the heart of the integration)

**Files:**
- Modify: `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`

This is the largest single edit. The merged `App.xaml.cs` is PaM's `App.xaml.cs` skeleton plus BoardRent's DI registrations, the BoardRent DB initializer call, the static `NavigateTo`/`NavigateBack` helpers BoardRent VMs use, the `Ioc.Default` bridge, the renamed AUMID, and the new `FindNotificationServerBinDir()` walker.

- [ ] **Step 9.1: Replace the stub `App.xaml.cs` with the merged content**

Path: `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Data;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Services.Listeners;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using BoardRentAndProperty.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;

namespace BoardRentAndProperty
{
    public partial class App : Application
    {
        private const int DefaultUserId = 1;
        private const int UserIdentifierArgumentIndex = 1;
        private const int KeyPartIndex = 0;
        private const int ValuePartIndex = 1;
        private const int SplitKeyValuePartsCount = 2;
        private const int DevModePrimaryUserIdentifier = 1;
        private const int DevModeSecondaryUserIdentifier = 2;
        private const int NoRunningProcessCount = 0;
        private const int SuccessExitCode = 0;

        private const string TwoWindowsEnvironmentKey = "TWO_WINDOWS";
        private const string EnabledEnvironmentValue = "true";
        private const string NotificationNavigationArgumentKey = "navigate";

        public static IServiceProvider Services { get; private set; } = default!;
        public static Window? MainWindow { get; set; }
        public Frame? RootFrame { get; set; }

        public string AppUserModelId { get; }
        public int CurrentUserId { get; }
        public NotificationsViewModel? NotificationsViewModel { get; private set; }

        private TaskbarIcon? trayIcon;
        private static Process? notificationServerProcess;
        private static Process? secondClientProcess;

        private Window? mainWindow;
        private INotificationRepository? notificationRepository;
        private INotificationService? notificationService;
        private IGameRepository? gameRepository;
        private IGameService? gameService;
        private readonly NotificationManager notificationManager;

        public App()
        {
            CurrentUserId = GetUserIdFromArgs();

            DatabaseInitializer.EnsureDatabaseInitialized();

            if (CurrentUserId == DevModePrimaryUserIdentifier && IsTwoWindowsEnabled())
            {
                StartNotificationServer();
                LaunchSecondClient();
            }

            AppUserModelId = $"BoardRentAndProperty -- user-{CurrentUserId}";

            notificationManager = new NotificationManager();
            SetupNotificationManager();
            EnsureSingleInstance(AppUserModelId);

            ConfigureServices();

            // BoardRent DB init runs after DI is built (AppDbContext is resolved from the container).
            Services.GetRequiredService<AppDbContext>().EnsureCreated();

            InitializeServices(CurrentUserId);

            InitializeComponent();
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // PaM mappers
            serviceCollection.AddSingleton<IMapper<User, UserDTO>, UserMapper>();
            serviceCollection.AddSingleton<IMapper<Game, GameDTO>, GameMapper>();
            serviceCollection.AddSingleton<IMapper<Notification, NotificationDTO>, NotificationMapper>();
            serviceCollection.AddSingleton<IMapper<Rental, RentalDTO>, RentalMapper>();
            serviceCollection.AddSingleton<IMapper<Request, RequestDTO>, RequestMapper>();

            // PaM cross-cutting
            serviceCollection.AddSingleton<ICurrentUserContext>(new CurrentUserContext(CurrentUserId));
            serviceCollection.AddSingleton<IToastNotificationService, ToastNotificationService>();
            serviceCollection.AddSingleton<IServerClient, NotificationClient>();

            // PaM repositories
            serviceCollection.AddSingleton<IUserRepository, UserRepository>();
            serviceCollection.AddSingleton<IGameRepository, GameRepository>();
            serviceCollection.AddSingleton<IRequestRepository, RequestRepository>();
            serviceCollection.AddSingleton<IRentalRepository, RentalRepository>();
            serviceCollection.AddSingleton<INotificationRepository, NotificationRepository>();

            // PaM services
            serviceCollection.AddSingleton<IUserService, UserService>();
            serviceCollection.AddSingleton<IGameService, GameService>();
            serviceCollection.AddSingleton<IRentalService, RentalService>();
            serviceCollection.AddSingleton<INotificationService, NotificationService>();
            serviceCollection.AddSingleton<IRequestService, RequestService>();

            // PaM view models
            serviceCollection.AddSingleton<NotificationsViewModel>();
            serviceCollection.AddSingleton<MenuBarViewModel>();
            serviceCollection.AddTransient(serviceProvider => new ListingsViewModel(
                serviceProvider.GetRequiredService<IGameService>(),
                serviceProvider.GetRequiredService<ICurrentUserContext>().CurrentUserId));
            serviceCollection.AddTransient<CreateGameViewModel>();
            serviceCollection.AddTransient<EditGameViewModel>();
            serviceCollection.AddTransient<CreateRequestViewModel>();
            serviceCollection.AddTransient<CreateRentalViewModel>();
            serviceCollection.AddTransient<RequestsFromOthersViewModel>();
            serviceCollection.AddTransient<RequestsToOthersViewModel>();
            serviceCollection.AddTransient<RentalsFromOthersViewModel>();
            serviceCollection.AddTransient<RentalsToOthersViewModel>();

            // BoardRent data layer
            serviceCollection.AddSingleton<AppDbContext>();
            serviceCollection.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

            // BoardRent repositories (account domain)
            serviceCollection.AddSingleton<IAccountRepository, AccountRepository>();
            serviceCollection.AddSingleton<IFailedLoginRepository, FailedLoginRepository>();

            // BoardRent services
            serviceCollection.AddSingleton<IAuthService, AuthService>();
            serviceCollection.AddSingleton<IAccountService, AccountService>();
            serviceCollection.AddSingleton<IAdminService, AdminService>();
            serviceCollection.AddSingleton<IFilePickerService, FilePickerService>();
            serviceCollection.AddSingleton<ISessionContext, SessionContext>();

            // BoardRent mappers (uniformity rule)
            serviceCollection.AddSingleton<AccountMapper>();
            serviceCollection.AddSingleton<AccountProfileMapper>();

            // BoardRent view models
            serviceCollection.AddTransient<LoginViewModel>();
            serviceCollection.AddTransient<RegisterViewModel>();
            serviceCollection.AddTransient<ProfileViewModel>();
            serviceCollection.AddTransient<AdminViewModel>();

            Services = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(Services);
        }

        // Static helpers used by BoardRent view models that call App.NavigateTo / App.NavigateBack.
        public static void NavigateTo(Type pageType, object? parameter = null, bool clearBackStack = false)
        {
            if (Application.Current is not App appInstance) return;
            if (appInstance.RootFrame == null) return;

            appInstance.RootFrame.Navigate(pageType, parameter);
            if (clearBackStack)
            {
                appInstance.RootFrame.BackStack.Clear();
            }
        }

        public static void NavigateBack()
        {
            if (Application.Current is not App appInstance) return;
            if (appInstance.RootFrame != null && appInstance.RootFrame.CanGoBack)
            {
                appInstance.RootFrame.GoBack();
            }
        }

        private int GetUserIdFromArgs()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > UserIdentifierArgumentIndex
                && int.TryParse(commandLineArgs[UserIdentifierArgumentIndex], out var parsedUserIdentifier))
            {
                return parsedUserIdentifier;
            }

            return DefaultUserId;
        }

        #region Two-window dev mode

        private static string? FindRepoRoot()
        {
            var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            while (currentDirectory != null)
            {
                if (Directory.Exists(Path.Combine(currentDirectory.FullName, ".git")))
                {
                    return currentDirectory.FullName;
                }

                currentDirectory = currentDirectory.Parent;
            }
            return null;
        }

        private static string? FindNotificationServerBinDir()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, "NotificationServer", "bin");
                if (Directory.Exists(candidate)) return candidate;
                current = current.Parent;
            }
            return null;
        }

        private static bool IsTwoWindowsEnabled()
        {
            try
            {
                var repoRoot = FindRepoRoot();
                if (repoRoot == null) return false;

                var envPath = Path.Combine(repoRoot, ".env");
                if (!File.Exists(envPath)) return false;

                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith('#') || !trimmed.Contains('=')) continue;

                    var parts = trimmed.Split('=', SplitKeyValuePartsCount);
                    if (parts[KeyPartIndex].Trim() == TwoWindowsEnvironmentKey)
                    {
                        return parts[ValuePartIndex].Trim().Equals(EnabledEnvironmentValue, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static void StartNotificationServer()
        {
            try
            {
                if (Process.GetProcessesByName("NotificationServer").Length > NoRunningProcessCount) return;

                var serverBinDir = FindNotificationServerBinDir();
                if (serverBinDir == null) return;

                var serverExe = Directory.GetFiles(serverBinDir, "NotificationServer.exe", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (serverExe == null) return;

                notificationServerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = serverExe,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Minimized
                });
            }
            catch
            {
            }
        }

        private static void LaunchSecondClient()
        {
            try
            {
                var currentExe = Environment.ProcessPath;
                if (currentExe == null) return;

                secondClientProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = currentExe,
                    Arguments = DevModeSecondaryUserIdentifier.ToString(),
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(currentExe)
                });
            }
            catch
            {
            }
        }

        private static void KillSpawnedChildProcesses()
        {
            try
            {
                if (secondClientProcess != null && !secondClientProcess.HasExited)
                {
                    secondClientProcess.Kill(entireProcessTree: true);
                }
            }
            catch { }

            try
            {
                if (notificationServerProcess != null && !notificationServerProcess.HasExited)
                {
                    notificationServerProcess.Kill(entireProcessTree: true);
                }
            }
            catch { }
        }

        #endregion

        private void SetupNotificationManager()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                notificationManager.Unregister();
                (notificationService as IDisposable)?.Dispose();
                KillSpawnedChildProcesses();
            };

            notificationManager.NotificationClicked += (sender, args) =>
            {
                mainWindow?.DispatcherQueue.TryEnqueue(() =>
                {
                    mainWindow?.Activate();

                    if (args.Arguments.ContainsKey(NotificationNavigationArgumentKey)
                        && args.Arguments[NotificationNavigationArgumentKey] == nameof(NotificationsPage))
                    {
                        ActivateWindow();
                        NavigateToNotificationsWithinShell();
                    }
                });
            };

            notificationManager.Init();
        }

        private void NavigateToNotificationsWithinShell()
        {
            if (RootFrame?.Content is MenuBarPage currentShell)
            {
                currentShell.NavigateToNotifications();
                return;
            }

            void OnShellLoaded(object sender, NavigationEventArgs navigationEventArgs)
            {
                if (navigationEventArgs.Content is MenuBarPage loadedShell)
                {
                    RootFrame!.Navigated -= OnShellLoaded;
                    loadedShell.NavigateToNotifications();
                }
            }

            RootFrame!.Navigated += OnShellLoaded;
            RootFrame.Navigate(typeof(MenuBarPage), gameService);
        }

        private void EnsureSingleInstance(string appUserModelId)
        {
            var appInstance = AppInstance.FindOrRegisterForKey(appUserModelId);
            if (!appInstance.IsCurrent)
            {
                appInstance.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs()).AsTask().Wait();
                Environment.Exit(SuccessExitCode);
            }

            appInstance.Activated += (sender, args) => ActivateWindow();
        }

        private void InitializeServices(int startupUserId)
        {
            RootFrame = new Frame();

            notificationRepository = Services.GetRequiredService<INotificationRepository>();
            notificationService = Services.GetRequiredService<INotificationService>();
            gameRepository = Services.GetRequiredService<IGameRepository>();
            gameService = Services.GetRequiredService<IGameService>();
            NotificationsViewModel = Services.GetRequiredService<NotificationsViewModel>();

            notificationService.StartListening();
            notificationService.SubscribeToServer(startupUserId);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            CreateAndShowMainWindow();

            var rootGrid = new Grid();
            rootGrid.Children.Add(RootFrame);
            MainWindow!.Content = rootGrid;

            RootFrame!.Navigate(typeof(MenuBarPage), gameService);

            CreateTrayIcon();
        }

        private void CreateAndShowMainWindow()
        {
            MainWindow = mainWindow = new MainWindow();
            mainWindow.Content = RootFrame;
            mainWindow.Activate();
            mainWindow.Title = AppUserModelId;
        }

        private void ActivateWindow()
        {
            mainWindow?.DispatcherQueue.TryEnqueue(() =>
            {
                if (mainWindow is MainWindow activatedMainWindow)
                {
                    activatedMainWindow.AppWindow.Show();
                }
                mainWindow?.Activate();
            });
        }

        private void CreateTrayIcon()
        {
            trayIcon = new TaskbarIcon
            {
                ToolTipText = AppUserModelId,
                IconSource = new BitmapImage(new Uri(Constants.AppTrayIconUri)),
            };

            var trayOpenCommand = new XamlUICommand();
            trayOpenCommand.ExecuteRequested += (sender, args) => ActivateWindow();
            var trayOpenMenuItem = new MenuFlyoutItem { Text = "Open", Command = trayOpenCommand };

            var trayExitCommand = new XamlUICommand();
            trayExitCommand.ExecuteRequested += (sender, args) =>
            {
                trayIcon.Dispose();
                Environment.Exit(SuccessExitCode);
            };
            var trayExitMenuItem = new MenuFlyoutItem { Text = "Exit", Command = trayExitCommand };

            trayIcon.ContextFlyout = new MenuFlyout { Items = { trayOpenMenuItem, trayExitMenuItem } };

            if (mainWindow!.Content is Grid rootGrid)
            {
                rootGrid.Children.Add(trayIcon);
            }
        }
    }
}
```

Notes for adapting:
- The `using BoardRentAndProperty.Constants;` line refers to PaM's `Constants.cs`/`ConstantsBridge.cs`. If the constant `Constants.AppTrayIconUri` lives at a different path in your tree, adjust the access (`Constants.AppTrayIconUri` vs `ConstantsBridge.AppTrayIconUri`).
- The PaM DTOs are still suffixed `*DTO` (e.g. `UserDTO`, `GameDTO`) — match whatever names exist in the merged `DataTransferObjects/` folder. If you decided in the design preamble to standardize on `*DataTransferObject`, do the rename in a separate pass before this step.

- [ ] **Step 9.2: Update `MainWindow.xaml`** to PaM's content (the merged window inherits PaM's Mica backdrop)

Path: `BoardRentAndProperty/BoardRentAndProperty/MainWindow.xaml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window
    x:Class="BoardRentAndProperty.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:BoardRentAndProperty"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="BoardRentAndProperty">
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>
    <Grid />
</Window>
```

(`MainWindow.xaml.cs` stays as the simple stub — `App.OnLaunched` builds the Grid + RootFrame.)

- [ ] **Step 9.3: Verify build succeeds**

```bash
dotnet build /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty.sln 2>&1 | grep -E "error CS|Build succeeded" | head -30
```

Expected: `Build succeeded.` If any errors remain, they're either:
- Missing using directives → add them.
- Type name typos in the merged App.xaml.cs → match against the actual type names in the merged folders.
- `Constants.AppTrayIconUri` path mismatch → adjust to the actual constant location in your `Constants.cs` / `ConstantsBridge.cs`.

Iterate until `Build succeeded.`

- [ ] **Step 9.4: Commit**

```bash
git add BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs \
       BoardRentAndProperty/BoardRentAndProperty/MainWindow.xaml
git commit -m "feat(merge): merge App.xaml.cs (PaM host + BoardRent DI + nav helpers + path walker)"
```

---

## Task 10: First runtime smoke test

Now the merged solution should build clean and launch. Before going further, validate the runtime works.

- [ ] **Step 10.1: Run with single window (no `.env` flag)**

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty && dotnet run -- 1
```

Expected:
- Window opens titled `BoardRentAndProperty -- user-1`.
- Side menu has 7 items: My Games, Others' Requests, Others' Rentals, My Requests, My Rentals, Notifications, BoardRent.
- Default page on the right is whatever `MenuBarPage.OnNavigatedTo` lands on (My Games, after Task 7's reset).
- Tray icon appears.

If the app doesn't launch:
- Inspect the console output for unhandled exceptions during `App` construction.
- Common: missing `App.config` connection string → check `BoardRentAndProperty/App.config` has `<add name="..." connectionString="..."/>` matching what `DatabaseInitializer` reads.
- Common: BoardRent's `AppDbContext.EnsureCreated()` connection error → confirm SQL LocalDB is running (`sqllocaldb info`).

- [ ] **Step 10.2: Click each PaM menu item and verify the right pane renders**

Click My Games, Others' Requests, Others' Rentals, My Requests, My Rentals, Notifications. Each should render the existing PaM page content. (Data may be empty for a fresh DB — that's expected.)

- [ ] **Step 10.3: Click the BoardRent menu item**

Expected: outer frame replaces the MenuBar with the LoginPage. The "← Back to Property & Management" button is visible at the top-left.

- [ ] **Step 10.4: Click the back button**

Expected: returns to MenuBar with My Games selected and `ListingsPage` rendered.

- [ ] **Step 10.5: Run with `TWO_WINDOWS=true` to validate the two-window dev mode**

Set `.env` at `E:\UBB-SE-2026-922-2\.env`:

```
TWO_WINDOWS=true
```

Then:

```bash
cd /e/UBB-SE-2026-922-2/BoardRentAndProperty/BoardRentAndProperty && dotnet run -- 1
```

Expected: two `BoardRentAndProperty.exe` windows open (titled `... -- user-1` and `... -- user-2`), and `NotificationServer.exe` is visible in Task Manager.

If `NotificationServer.exe` does not start: check the `FindNotificationServerBinDir()` walker — log the output of `AppContext.BaseDirectory` and confirm `NotificationServer/bin/` exists somewhere up the tree.

- [ ] **Step 10.6: Smoke test BoardRent register + login**

In one window, click BoardRent → click "Create an account" → register a fresh user. Verify it lands somewhere sensible after registration (Profile or back to Login per the existing BoardRent flow). Log in as the new user. Click the back button to return to PaM.

If registration fails with a SQL error: verify `AppDbContext.EnsureCreated()` ran cleanly — drop the `BoardRentDb` from SSMS or `sqlcmd` and re-run.

- [ ] **Step 10.7: Smoke test PaM cross-window notifications**

In window 1, perform a PaM action that fires a notification (e.g., create a request that targets user 2). Verify the notification appears in window 2's Notifications page **and** a Windows toast fires on window 2's side.

If toasts don't fire: confirm `NotificationManager.Init()` ran without exception during startup (check the debug console).

- [ ] **Step 10.8: Smoke test tray icon**

Right-click the tray icon → Open / Exit menu items appear. Click Exit → both processes terminate.

- [ ] **Step 10.9: Commit any quick fixes from the smoke test**

If the smoke test exposed any bugs (typos, missed namespaces, wrong `Constants.AppTrayIconUri` value), fix them and commit:

```bash
git add -p   # stage interactively
git commit -m "fix(merge): resolve smoke-test issues found in first runtime validation"
```

---

## Task 11: Acceptance checklist (matches design spec §9)

Walk through every item from the spec's §9 acceptance / smoke test, in order. For each, mark the box.

- [ ] **Step 11.1: `dotnet build BoardRentAndProperty.sln` succeeds without errors**
- [ ] **Step 11.2: Two windows + NotificationServer process all visible with `TWO_WINDOWS=true`**
- [ ] **Step 11.3: Each window's left ListView shows: My Games, Others' Requests, Others' Rentals, My Requests, My Rentals, Notifications, BoardRent (in that order)**
- [ ] **Step 11.4: Clicking each PaM menu item renders the corresponding PaM page in the right pane**
- [ ] **Step 11.5: Clicking BoardRent → outer frame replaces MenuBar with LoginPage; "Back to Property & Management" visible**
- [ ] **Step 11.6: Back button → returns to MenuBar with My Games selected, ListingsPage rendered**
- [ ] **Step 11.7: BoardRent Login flow works (register, log in with the seed admin account)**
- [ ] **Step 11.8: BoardRent Register / Profile / Admin pages reachable from inside the BoardRent flow**
- [ ] **Step 11.9: Sending a notification from window 1 → toast in window 2**
- [ ] **Step 11.10: Tray icon Open / Exit work**
- [ ] **Step 11.11: With `TWO_WINDOWS` unset / false: single window, no NotificationServer, no second client; menu still includes BoardRent**

If any item fails, file a fix step in the next commit cycle.

- [ ] **Step 11.12: Commit final acceptance state**

```bash
git add -A
git commit -m "feat(merge): merge complete and acceptance smoke tests pass"
```

---

## Self-Review

After finishing, the engineer should re-read the spec and confirm:

1. **Spec coverage:** Every section of the spec has a corresponding task. Section 4 (folder layout) → Tasks 4, 5. Section 5 (rename map) → Task 5. Section 6 (App.xaml.cs composition) → Task 9. Section 7 (MenuBarPage integration) → Tasks 7, 8. Section 8 (config + csproj + path walker) → Tasks 3, 9. Section 9 (acceptance test) → Tasks 10, 11.

2. **Placeholders:** No "TBD", "TODO", "fill in" left in any task body. Every step has a concrete command, a concrete code block, or a concrete file edit.

3. **Type consistency:** `App.NavigateTo(Type, object?, bool)` signature in Task 9 matches the call sites in Tasks 7 (`App.NavigateTo(typeof(LoginPage))` — single arg, defaults) and Task 8 (`App.NavigateTo(typeof(MenuBarPage), gameService)` — two args, default for `clearBackStack`). `MenuBarPage` (renamed from `MenuBarView` in Task 4 step 4.8) is consistent across all subsequent references.

4. **Scope:** Plan covers exactly the spec — building the merged skeleton with PaM-host + BoardRent-folded-in. Out of scope: tests, WebApp/WebApplication/Documentation, DB merging, identity unification — none of these have tasks.

If a gap is found, add or amend the task inline.

---

## Plan complete

Saved to `docs/superpowers/plans/2026-04-28-boardrent-and-property-merge-plan.md`. Two execution options:

1. **Subagent-Driven (recommended)** — Dispatch a fresh subagent per task, review between tasks, fast iteration.
2. **Inline Execution** — Execute tasks in this session using `executing-plans`, batch with checkpoints.

Which approach?
