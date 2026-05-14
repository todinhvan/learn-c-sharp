using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres-server")
                      .WithImageTag("18")
                      .WithVolume("postgres-data", "/var/lib/postgresql")
                      .WithLifetime(ContainerLifetime.Persistent);

var coreBankingDb = postgres.AddDatabase("corebanking-db", "CoreBankingDb");

var migrationService = builder.AddProject<CoreBanking_MigrationService>("migration-service")
                              .WithReference(coreBankingDb)
                              .WaitFor(postgres);

builder.AddProject<CoreBanking_API>("corebanking-api")
       .WithReference(coreBankingDb)
       .WaitFor(postgres)
       .WaitForCompletion(migrationService);

builder.Build().Run();
