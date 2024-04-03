using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

namespace SpeechLive.SagaUnit.HostService
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SpeechLive.SagaUnit.HostService", Version = "v1" });
            });

            services.AddDbContext<JobServiceSagaDbContext>(builder =>
                builder.UseSqlServer(_configuration.GetConnectionString("DictationUnitSagaDb"),
                    m =>
                    {
                        m.MigrationsAssembly("SpeechLive.SagaUnit.WebHost");
                        m.MigrationsHistoryTable($"__{nameof(JobServiceSagaDbContext)}");
                    }));

            services.AddMassTransit(cfg =>
            {
                cfg.AddDelayedMessageScheduler();

                cfg.AddSagaRepository<JobSaga>()
                    .EntityFrameworkRepository(c =>
                    {
                        c.ExistingDbContext<JobServiceSagaDbContext>();
                        c.LockStatementProvider = new SqlServerLockStatementProvider();
                        c.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    });
                cfg.AddSagaRepository<JobTypeSaga>()
                    .EntityFrameworkRepository(c =>
                    {
                        c.ExistingDbContext<JobServiceSagaDbContext>();
                        c.LockStatementProvider = new SqlServerLockStatementProvider();
                        c.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    });
                cfg.AddSagaRepository<JobAttemptSaga>()
                    .EntityFrameworkRepository(c =>
                    {
                        c.ExistingDbContext<JobServiceSagaDbContext>();
                        c.LockStatementProvider = new SqlServerLockStatementProvider();
                        c.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    });
                cfg.SetKebabCaseEndpointNameFormatter();

                cfg.UsingAzureServiceBus((ctx, config) =>
                {

                    config.Host(_configuration.GetConnectionString("ServiceBus"));
                    config.UseDelayedMessageScheduler();
                });
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpeechLive.DictationUnit.HostService v1"));
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
