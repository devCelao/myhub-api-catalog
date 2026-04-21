using CatalogApplication.Services;
using IntegrationHandlers.Requests;
using MessageBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatalogApplication.Responders;

public class CatalogResponder(
    IBusMessage message,
    IServiceProvider serviceProvider,
    ILogger<CatalogResponder> log) : BackgroundService
{
    private readonly IBusMessage busMessage = message;
    private readonly IServiceProvider provider = serviceProvider;
    private readonly ILogger<CatalogResponder> logger = log;
    private IDisposable? _responderDisposable;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Aguardando RabbitMQ estar pronto...");
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        logger.LogInformation("Registrando CatalogResponder...");

        _responderDisposable = await busMessage.RespondAsync<ObterServicosPlanoRequest, ObterServicosPlanoResponse>(
            async (request) =>
            {
                using var scope = provider.CreateScope();
                var planoService = scope.ServiceProvider.GetRequiredService<IPlanoApplicationService>();

                logger.LogInformation("Recebida requisicao para obter plano: {CodPlano}", request.CodPlano);

                var resultado = await planoService.ObterPlanoAsync(request.CodPlano);

                if (!resultado.IsSuccess)
                {
                    logger.LogWarning("Plano {CodPlano} não encontrado: {Errors}", request.CodPlano, string.Join(",", resultado.Errors));
                    return new ObterServicosPlanoResponse("CodPlano", resultado.Errors);
                }

                logger.LogInformation("Plano {CodPlano} retornado com sucesso", request.CodPlano);

                return new ObterServicosPlanoResponse()
                {
                    Plano = resultado.Data
                };
            });

        logger.LogInformation("CatalogResponder registrado com sucesso!");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _responderDisposable?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
