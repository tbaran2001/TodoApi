using System.Reflection;
using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Todo.Api.Common.Behaviors;
using Todo.Api.Data;
using Todo.Api.Data.Repos;
using Microsoft.OpenApi.Models;
using Todo.Api.Common.Exceptions;
using Todo.Api.Strategies;
using Todo.Api.Providers;
using System.Text.Json.Serialization;
using Todo.Api.Common.Options;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = new List<OpenApiServer>
        {
            new() { Url = "http://localhost:5000" },
            new() { Url = "https://localhost:7289" }
        };
        return Task.CompletedTask;
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<TodoValidationOptions>(builder.Configuration.GetSection("Validation:Todo"));

builder.Services.AddCarter();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
    configuration.AddOpenBehavior(typeof(SaveChangesBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));
ValidatorOptions.Global.LanguageManager.Enabled = false;
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});

builder.Services.AddScoped<ITodosRepository, TodosRepository>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<IIncomingTodosRangeProvider, IncomingTodosRangeProvider>();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<IIncomingTodosRangeStrategy>()
    .AddClasses(classes => classes.AssignableTo<IIncomingTodosRangeStrategy>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/scalar");
        return;
    }

    await next();
});

app.MapCarter();

app.Run();