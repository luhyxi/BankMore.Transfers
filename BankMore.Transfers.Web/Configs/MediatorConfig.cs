using BankMore.Transfers.Application.Transferencia.Command.Create;

namespace BankMore.Transfers.Web.Configs;

public static class MediatorConfig
{
  public static IServiceCollection AddMediatorSourceGen(this IServiceCollection services,
    ILogger logger)
  {
    logger.LogInformation("Registering Mediator SourceGen and Behaviors");
    services.AddMediator(options =>
    {
      options.ServiceLifetime = ServiceLifetime.Scoped;

      options.Assemblies =
      [
        typeof(CreateTransferenciaCommand),
      ];
    });

    return services;
  }
}
