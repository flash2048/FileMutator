using FileMutator.infrastructure;

var builder = DistributedApplication.CreateBuilder(args);

var isTest = args.Contains("--test", comparer: StringComparer.OrdinalIgnoreCase);

var db = builder.AddSqlServer("sql")
    .WithLifetime(isTest ? ContainerLifetime.Session : ContainerLifetime.Persistent)
    .AddDatabase("db");

var connectionStringExpression = db.Resource.ConnectionStringExpression;

var migration = builder.AddProject<Projects.FileMutator_DbMigrate>("migration").WaitFor(db)
    .WithEnvironment(FileMutatorConstants.DbConnectionStringName, connectionStringExpression);

builder.AddProject<Projects.FileMutator_Web>("apiservice")
    .WithReference(db)
    .WithEnvironment(FileMutatorConstants.DbConnectionStringName, connectionStringExpression)
    .WaitForCompletion(migration);

builder.Build().Run();
