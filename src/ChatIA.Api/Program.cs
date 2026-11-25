using ChatIA.Api.Filters;
using ChatIA.Application;
using ChatIA.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Composition root — mesmo padrão do CashFlow.Api
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Filtro global de exceções (padrão CashFlow)
builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));

// Permite que o Blazor (outro origin/porta) chame a Api no desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7042",
                "http://localhost:5009")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necessário para WebApplicationFactory nos testes de integração (padrão CashFlow)
public partial class Program { }
