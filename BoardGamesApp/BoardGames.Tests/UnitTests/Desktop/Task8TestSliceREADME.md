## Task 8 Desktop Test Slice

Files added for the Desktop shell/session unit-test slice:

- `UnitTests/Desktop/Services/SessionContextTests.cs`
- `UnitTests/Desktop/Services/CurrentUserContextTests.cs`
- `UnitTests/Desktop/Services/DesktopAuthorizationServiceTests.cs`
- `UnitTests/Desktop/ViewModels/ShellViewModelTests.cs`
- `Fakes/FakeClientAuthService.cs`
- `Fakes/FakeCurrentUserContext.cs`
- `Fakes/FakeSessionContext.cs`

Existing files also updated because the Task 8 slice depends on them:

- `ViewModels/LoginViewModelTests.cs`
- `ViewModels/RegisterViewModelTests.cs`
- `ViewModels/BaseViewModelTests.cs`
- `BoardGames.Tests.csproj`

To make the slice work in another branch or clone:

1. Add a `ProjectReference` from `BoardGames.Tests` to `BoardGames.Desktop`.
2. Keep NUnit + Moq + `Microsoft.NET.Test.Sdk` + `NUnit3TestAdapter` + `coverlet.collector`.
3. Remove stale legacy test files from the active compile set, or switch the test project to explicit `Compile Include` entries.
4. Include the files listed above in the test project compile items.
5. Run:
   - `dotnet build BoardGames.Desktop/BoardGames.Desktop.csproj -c Debug -p:Platform=x64`
   - `dotnet test BoardGames.Tests/BoardGames.Tests.csproj -c Debug -p:Platform=x64`
   - `dotnet test BoardGames.Tests/BoardGames.Tests.csproj -c Debug -p:Platform=x64 --collect:"XPlat Code Coverage;Format=cobertura"`
