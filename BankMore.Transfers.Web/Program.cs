using BankMore.Transfers.Web.Configs;
using BankMore.Transfers.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var startupLogger = loggerFactory.CreateLogger<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(startupLogger);

builder.Services.AddServiceConfigs(startupLogger, builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapTransferenciaEndpoints();

app.Run();
