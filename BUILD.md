# Build No Glow Expanded

Use a .NET 8 SDK or newer, a local Valheim installation, and a Valheim profile with BepInEx and Jötunn. From the repository root, run:

```powershell
dotnet build .\NoGlowExpanded.csproj -c Release `
  "-p:ValheimPath=C:\path\to\Valheim" `
  "-p:BepInExCore=C:\path\to\BepInEx\core" `
  "-p:JotunnDll=C:\path\to\Jotunn.dll"
```

The output is `bin/Release/netstandard2.1/NoGlowExpanded.dll`. For Thunderstore, place it under `plugins/` in the ZIP. Put the root README, changelog, and license and the files from `release/` at the root of the ZIP. Valheim, BepInEx, and Jötunn assemblies are local build references.

The standard build includes the optional ValheimArmory integration in the same DLL. ValheimArmory is detected at runtime and is unnecessary for building the project. Use `-p:DiagnosticBuild=true` and a separate output folder to build the effect audit plugin.
