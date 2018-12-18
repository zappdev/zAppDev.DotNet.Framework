# Manual

## Manual

```
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura -f netcoreapp2.1
dotnet reportgenerator -reports:coverage.cobertura.xml -targetdir:Coverage
```

## Ubuntu

```
sudo apt install libgdiplus 
```