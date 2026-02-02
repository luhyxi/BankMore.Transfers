using BankMore.Transfers.Application.Services;
using BankMore.Transfers.Domain.Interfaces;
using BankMore.Transfers.Infrastructure;

namespace BankMore.Transfers.Web.Configs;
public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructure(builder.Configuration, logger)
            .AddMediatorSourceGen(logger);
    
    services.Configure<ContaCorrenteOptions>(builder.Configuration.GetSection("CheckingAccount"));
    services.AddHttpClient<IContaCorrenteService, ContaCorrenteService>();
    logger.LogInformation("services registered");

    return services;
  }

    
}
