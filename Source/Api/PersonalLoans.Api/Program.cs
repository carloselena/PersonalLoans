using Accounts.Application.DependencyInjection;
using Accounts.Presentation;
using Identity.Application.DependencyInjection;
using Identity.Presentation;
using Identity.Presentation.Endpoints;
using Lending.Presentation;
using PersonalLoans.Api.Infrastructure.Authentication;
using PersonalLoans.Api.Infrastructure.ExceptionHandling;
using PersonalLoans.Api.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddAccountsModule(builder.Configuration);
builder.Services.AddLendingModule(builder.Configuration);

// Authentication
builder.Services.AddCurrentUser();

// Messaging Infrastructure
builder.Services.AddMassTransitModules(
    builder.Configuration,
    assemblies:
    [
        typeof(IdentityMassTransitModule).Assembly,
        typeof(AccountsMassTransitModule).Assembly
    ]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api/v1");

api
    .MapAuthEndpoints()
    .MapAllLendingEndpoints();

app.Run();