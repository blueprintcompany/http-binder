# Contributing

Thanks for helping improve Blueprint.HttpBinder.

## Local setup

Install the .NET 9 SDK, then run:

```bash
dotnet restore Blueprint.HttpBinder.slnx
dotnet test --project src/Blueprint.HttpBinder.Generator.Tests/Blueprint.HttpBinder.Generator.Tests.csproj --no-restore
```

## Pull requests

- Keep public API changes intentional. Attribute names, namespaces, package ID, and diagnostic IDs are public contract.
- Add or update tests for generator behavior, analyzer diagnostics, and sample integration when behavior changes.
- Keep generated code readable. Users will inspect generated binders when debugging Minimal API binding.
- Update `README.md` when package usage, supported types, diagnostics, or release behavior changes.

## Packaging checks

Before release-oriented changes, verify package contents:

```bash
dotnet pack -c Release src/Blueprint.HttpBinder.Generator/Blueprint.HttpBinder.Generator.csproj -o ./nupkgs /p:Version=1.0.0 --no-restore
```

The package should contain the README, icon, analyzer DLLs, abstractions DLL/XML, MIT license metadata, and correct repository URLs.
