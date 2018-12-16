# Manual

## Manual

```
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura -f netcoreapp3.0.0
dotnet reportgenerator -reports:coverage.cobertura.xml -targetdir:Coverage
```

## Ubuntu

```
sudo apt install libgdiplus 
```