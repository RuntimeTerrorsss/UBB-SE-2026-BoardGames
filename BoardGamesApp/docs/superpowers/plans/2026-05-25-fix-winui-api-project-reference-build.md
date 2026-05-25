# Fix WinUI API Project Reference Build Bug Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `BoardGames.Desktop` build by removing illegal executable project references while preserving the intended final architecture where Desktop, API, and Web run side by side.

**Architecture:** `BoardGames.Desktop` must not compile `BoardGames.Api` into the WinUI app. The desktop app should consume API functionality through HTTP clients/contracts from `BoardGames.Shared`, while `BoardGames.Api` runs as a separate startup project and owns server-side business logic plus database access.

**Tech Stack:** .NET 8, WinUI 3 / Windows App SDK, ASP.NET Core Web API, EF Core SQL Server, Visual Studio multi-startup projects, MSBuild project references.

---

## Root Cause

The Visual Studio errors are caused by `BoardGames.Desktop` being configured as a self-contained WinUI executable while referencing other executable projects.

Evidence:

- `BoardGames.Desktop/BoardGames.Desktop.csproj` has `<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>`.
- `BoardGames.Desktop/BoardGames.Desktop.csproj` references `..\BoardGames.Api\BoardGames.Api.csproj`.
- `BoardGames.Desktop/BoardGames.Desktop.csproj` references `..\ServerCommunication\ServerCommunication.csproj`.
- `BoardGames.Api/BoardGames.Api.csproj` uses `Microsoft.NET.Sdk.Web`, so it is an ASP.NET Core executable.
- `ServerCommunication/ServerCommunication.csproj` has `<OutputType>Exe</OutputType>`.

Why multiple startup projects do not fix it:

- Multiple startup projects decide which apps run.
- Project references decide what gets compiled into another project.
- Desktop should run beside API, not reference API as a compile-time dependency.

Correct relationship:

```text
BoardGames.Desktop  --HTTP-->  BoardGames.Api
BoardGames.Web      --HTTP-->  BoardGames.Api
BoardGames.Api      --> BoardGames.Data
BoardGames.Shared   --> DTOs/contracts/API clients used by Desktop/Web
NotificationServer  --> ServerCommunication
BoardGames.Desktop  --> ServerCommunication only if ServerCommunication is a library
```

Incorrect current relationship:

```text
BoardGames.Desktop --> BoardGames.Api executable
BoardGames.Desktop --> ServerCommunication executable
```

## Scope

This plan fixes the build bug shown in Visual Studio. It does not finish the whole merge requirement. After this bug is fixed, additional work is still needed for API dependency injection, duplicate controllers, database migrations, API URL alignment, and removing old/dummy WinUI paths.

## File Structure

- Modify `BoardGames.Desktop/BoardGames.Desktop.csproj`
  - Remove the illegal `BoardGames.Api` project reference.
  - Keep only references that are valid for the WinUI client.

- Modify `ServerCommunication/ServerCommunication.csproj`
  - Convert it from executable to class library, because it contains shared message contracts/helpers used by `NotificationServer` and Desktop.

- Review `BoardGames.Desktop/App.xaml.cs`
  - Identify compile errors caused by removing the direct API reference.
  - This file currently constructs server-side API services directly inside WinUI, which violates the final architecture.

- Review `BoardGames.Desktop/App.xaml2.cs`
  - This contains the better API-client DI shape, but it is not the active app shell.
  - Do not switch shells in this bug fix unless the minimal build fix still leaves Desktop unable to compile.

- No database or migration file changes are needed for this specific bug.

## Task 1: Remove Illegal API Reference From Desktop

**Files:**
- Modify: `BoardGames.Desktop/BoardGames.Desktop.csproj`

- [ ] **Step 1: Remove the API project reference**

Change the project reference block from:

```xml
<ItemGroup>
  <ProjectReference Include="..\BoardGames.Data\BoardGames.Data.csproj" />
  <ProjectReference Include="..\BoardGames.Shared\BoardGames.Shared.csproj" />
  <ProjectReference Include="..\BoardGames.Api\BoardGames.Api.csproj" />
  <ProjectReference Include="..\ServerCommunication\ServerCommunication.csproj" />
</ItemGroup>
```

to:

```xml
<ItemGroup>
  <ProjectReference Include="..\BoardGames.Data\BoardGames.Data.csproj" />
  <ProjectReference Include="..\BoardGames.Shared\BoardGames.Shared.csproj" />
  <ProjectReference Include="..\ServerCommunication\ServerCommunication.csproj" />
</ItemGroup>
```

- [ ] **Step 2: Build Desktop to verify this specific error changes**

Run:

```powershell
dotnet build .\BoardGames.Desktop\BoardGames.Desktop.csproj --no-restore
```

Expected:

- The `BoardGames.Api.csproj is a non self-contained executable` error disappears.
- Compile errors may appear in `App.xaml.cs` or viewmodels because Desktop currently imports server-side API service namespaces. Those are expected and become the next architectural cleanup.

## Task 2: Convert ServerCommunication To A Library

**Files:**
- Modify: `ServerCommunication/ServerCommunication.csproj`

- [ ] **Step 1: Remove executable output type**

Change:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

to:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

- [ ] **Step 2: Build Desktop again**

Run:

```powershell
dotnet build .\BoardGames.Desktop\BoardGames.Desktop.csproj --no-restore
```

Expected:

- The `ServerCommunication.csproj is a non self-contained executable` error disappears.

- [ ] **Step 3: Build NotificationServer**

Run:

```powershell
dotnet build .\NotificationServer\NotificationServer.csproj --no-restore
```

Expected:

- `NotificationServer` still builds because it can reference a class library.

## Task 3: Replace Desktop's Direct API-Service Usage

**Files:**
- Modify: `BoardGames.Desktop/App.xaml.cs`
- Possibly modify: `BoardGames.Desktop/BoardGames.Desktop.csproj`
- Prefer existing client files under `BoardGames.Shared/ProxyServices`

- [ ] **Step 1: Build and collect compile errors**

Run:

```powershell
dotnet build .\BoardGames.Desktop\BoardGames.Desktop.csproj --no-restore
```

Expected:

- Errors should point to server-side types imported from `BoardGames.Api.Services`, such as `RentalService`, `ReceiptService`, `CardPaymentService`, `BookingService`, `SearchAndFilterService`, or `UserService`.

- [ ] **Step 2: Remove server-side API service construction from Desktop**

In `BoardGames.Desktop/App.xaml.cs`, remove the pattern:

```csharp
using BoardGames.Api.Services;
```

and stop constructing API server services directly:

```csharp
RentalService = new RentalService(RentalRepository, GameRepository);
ReceiptService = new ReceiptService(UserRepository, RentalService, GameRepository);
CardPaymentService = new CardPaymentService(PaymentRepository, UserRepository, ReceiptService, RentalService);
ServicePayment = new ServicePayment(HistoryRepository, ReceiptService, RentalService, conversationService);
CashPaymentService = new CashPaymentService(PaymentRepository, new CashPaymentMapper(), ReceiptService);
BookingService = new BookingService(GameRepository, RentalRepository, UserRepository);
SearchAndFilterService = new SearchAndFilterService(GameRepository, UserRepository, RentalRepository, GlobalGeographicalService);
UserService = new UserService(UserRepository);
```

Replace only the pieces needed for Desktop to compile with existing shared API clients or keep repository-style HTTP proxies temporarily. The smallest valid temporary architecture is:

```text
Desktop ViewModels -> Shared HTTP proxies -> BoardGames.Api
```

Do not re-add `BoardGames.Api` as a project reference.

- [ ] **Step 3: Build Desktop**

Run:

```powershell
dotnet build .\BoardGames.Desktop\BoardGames.Desktop.csproj --no-restore
```

Expected:

- No `NETSDK1150` errors.
- Remaining errors, if any, are normal compile errors and should be handled without reintroducing executable project references.

## Task 4: Verify API Is Separate And Runnable

**Files:**
- Review: `BoardGames.Api/Program.cs`
- Review: `BoardGames.Api/Properties/launchSettings.json`

- [ ] **Step 1: Build API separately**

Run:

```powershell
dotnet build .\BoardGames.Api\BoardGames.Api.csproj --no-restore
```

Expected:

- API builds independently.
- If it fails due to duplicate controllers or missing DI namespaces, fix those separately. Do not solve API failures by referencing API from Desktop.

- [ ] **Step 2: Run API separately**

Run:

```powershell
dotnet run --project .\BoardGames.Api\BoardGames.Api.csproj --launch-profile http
```

Expected:

- API starts on `http://localhost:5018`.
- Swagger is available at `http://localhost:5018/swagger`.

## Task 5: Configure Desktop To Call The Running API

**Files:**
- Modify: `BoardGames.Desktop/App.xaml.cs` or the active Desktop configuration file
- Review: `BoardGames.Desktop/App.xaml2.cs`

- [ ] **Step 1: Align API base URL**

The current active Desktop shell contains:

```csharp
public static readonly string BaseApiUrl = "http://localhost:5000/api/";
public static readonly string RemoteApiUrl = "http://172.30.250.124:5000/api/";
public static readonly System.Net.Http.HttpClient Client = new System.Net.Http.HttpClient { BaseAddress = new Uri(RemoteApiUrl) };
```

For local development, use the API launch profile port:

```csharp
public static readonly string BaseApiUrl = "http://localhost:5018/api/";
public static readonly System.Net.Http.HttpClient Client = new System.Net.Http.HttpClient { BaseAddress = new Uri(BaseApiUrl) };
```

- [ ] **Step 2: Run API and Desktop as separate startup projects**

In Visual Studio:

1. Right-click solution.
2. Select `Configure Startup Projects`.
3. Choose `Multiple startup projects`.
4. Set `BoardGames.Api` to `Start`.
5. Set `BoardGames.Desktop` to `Start`.
6. Set `BoardGames.Web` to `Start` only when checking the full merged app.

Expected:

- Desktop and API run as separate processes.
- Desktop HTTP calls target API, not a compiled API reference.

## Task 6: Final Verification For This Bug

**Files:**
- Verify: `BoardGames.Desktop/BoardGames.Desktop.csproj`
- Verify: `ServerCommunication/ServerCommunication.csproj`

- [ ] **Step 1: Confirm Desktop no longer references API**

Run:

```powershell
Select-String -Path .\BoardGames.Desktop\BoardGames.Desktop.csproj -Pattern 'BoardGames.Api'
```

Expected:

- No matches.

- [ ] **Step 2: Confirm ServerCommunication is not an executable**

Run:

```powershell
Select-String -Path .\ServerCommunication\ServerCommunication.csproj -Pattern 'OutputType'
```

Expected:

- No matches.

- [ ] **Step 3: Build the two projects involved in the screenshot error**

Run:

```powershell
dotnet build .\ServerCommunication\ServerCommunication.csproj --no-restore
dotnet build .\BoardGames.Desktop\BoardGames.Desktop.csproj --no-restore
```

Expected:

- No `NETSDK1150` errors.

- [ ] **Step 4: Build the API separately**

Run:

```powershell
dotnet build .\BoardGames.Api\BoardGames.Api.csproj --no-restore
```

Expected:

- API build status is known independently from Desktop.
- Any API errors are handled in API wiring tasks, not by re-adding Desktop-to-API references.

## Acceptance Criteria

- The Visual Studio errors about `BoardGames.Api.csproj` and `ServerCommunication.csproj` being non-self-contained executables are gone.
- `BoardGames.Desktop.csproj` does not reference `BoardGames.Api.csproj`.
- `ServerCommunication.csproj` is a class library, or Desktop no longer references it.
- Desktop, API, and Web are configured to run as separate startup projects.
- No database setup is required to resolve this specific build bug.
- Any remaining build errors are separate merge issues, not the screenshot's `NETSDK1150` bug.

## Follow-Up Work After This Bug

These are outside this build-bug spec but still required for the assignment:

- Register API DbContext, repositories, mappers, and services in `BoardGames.Api/Program.cs`.
- Remove duplicate old/new API controllers such as `GamesController` and `GamesController2`.
- Choose one active WinUI shell and remove/retire the other.
- Move server-side business logic fully out of Desktop.
- Remove Desktop database bootstrap/mock seeding.
- Align API base URLs in Desktop and Web.
- Run EF migrations against one shared database.
- Update tests to target the final merged architecture.
