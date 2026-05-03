using Accounts.Presentation;
using Identity.Presentation;
using Identity.Presentation.Endpoints;
using Lending.Presentation;
using MassTransit;
using PersonalLoans.Api.Infrastructure.Authentication;
using PersonalLoans.Api.Infrastructure.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddAccountsModule(builder.Configuration);
builder.Services.AddLendingModule(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddCurrentUser();

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