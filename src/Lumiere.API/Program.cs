using Lumiere.API.Extensions;
using Lumiere.Application.DependencyInjection;
using Lumiere.Infra.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerDocs();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwaggerDocs();
app.UseHttpsRedirection();
app.AddEndpoints();

app.Run();
