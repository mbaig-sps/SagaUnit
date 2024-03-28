using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.ExtensionsLoggingIntegration;
using MassTransit.JobService;
using MassTransit.JobService.Configuration;
using MassTransit.ServiceBus.Configuration;
using MassTransit.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DictationSlotStateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DictationUnitSagaDb")));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
