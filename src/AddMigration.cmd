@if "%1"==""; echo Specify migration name please && pause && exit /b 1
set FileMutatorConnectionString=Add Connection String
cd filemutator.dbmigrate
dotnet ef migrations add %1 --context FileMutatorDbContext -o Migrations
cd ../..