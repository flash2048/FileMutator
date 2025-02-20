
using AutoMapper.EquivalencyExpression;
using FileMutator.infrastructure;
using FileMutator.infrastructure.EF;
using FileMutator.Tools;
using FileMutator.Tools.Interfaces;
using FileMutator.Web.Mapping;
using Microsoft.EntityFrameworkCore;

namespace FileMutator.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var allowSpecificOrigins = "_EnableCORS";

            // Add services to the container.

            builder.Services.AddScoped<IMutatorService, MutatorService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(allowSpecificOrigins, policyBuilder =>
                {
                    // TODO need to use correctly, not only in the test mode
                    policyBuilder.WithOrigins().AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(_ => true)
                        .AllowCredentials().Build();
                });
            });

            AddAutoMapper(builder);

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            ConfigureDatabaseServices(builder);

            var app = builder.Build();
            app.MapStaticAssets();
            app.UseCors(allowSpecificOrigins);

            AddOpenApi(app);

            app.UseHttpsRedirection();

            //TODO use Authorization
            //app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void AddOpenApi(WebApplication app)
        {
            app.MapOpenApi().CacheOutput();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "File Mutator V1");
                options.RoutePrefix = "";
            });
        }

        private static void AddAutoMapper(WebApplicationBuilder builder)
        {
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.AllowNullCollections =
                    true; // This makes behavior of runtime mapping similar to projection, otherwise mapper will replace null collection with empty one for runtime mapping
            }, typeof(WebProfile)); // Scan current and infrastructure assemblies
        }

        private static void ConfigureDatabaseServices(WebApplicationBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable(FileMutatorConstants.DbConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"The '{FileMutatorConstants.DbConnectionStringName}' don't contains value!");

            builder.Services.AddDbContext<FileMutatorDbContext>(options =>
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__MigrationsHistory")
                        .EnableRetryOnFailure()
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));

        }
    }
}
