dotnet test --no-build --no-restore /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator "-reports:coverage.cobertura.xml" "-targetdir:coveragereport"