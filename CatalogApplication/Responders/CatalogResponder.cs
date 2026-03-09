using CatalogApplication.Services;
using DataTransferObjects.CatalogDomain;
using IntegrationHandlers.Requests;
using IntegrationHandlers.Responses;
using MessageBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatalogApplication.Responders;

public class CatalogResponder : BackgroundService
{
    private readonly IBusMessage busMessage;
    private readonly IServiceProvider provider;
    private readonly ILogger<CatalogResponder> logger;
    private IDisposable? _responderDisposable;

    public CatalogResponder(
        IBusMessage message,
        IServiceProvider serviceProvider,
        ILogger<CatalogResponder> log)
    {
        busMessage = message;
        provider = serviceProvider;
        logger = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Aguardando RabbitMQ estar pronto...");
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        logger.LogInformation("Registrando CatalogResponder...");

        // ✅ AGUARDAR o RespondAsync e manter o IDisposable
        _responderDisposable = await busMessage.RespondAsync<ObterServicosPlanoRequest, ObterServicosPlanoResponse>(
            async (request) =>
            {
                using var scope = provider.CreateScope();
                var catalogService = scope.ServiceProvider.GetRequiredService<ICatalogService>();

                logger.LogInformation("Recebida requisição para obter plano: {CodPlano}", request.CodPlano);

                var resultado = await catalogService.ObterPlanoAsync(request.CodPlano);

                if (resultado.PossuiErros)
                {
                    logger.LogWarning("Plano {CodPlano} não encontrado", request.CodPlano);
                    return new ObterServicosPlanoResponse("CodPlano", string.Join(",", resultado.Errors));
                }

                logger.LogInformation("Plano {CodPlano} retornado com sucesso", request.CodPlano);

                if (resultado.ResultObject is not PlanoDto plano)
                {
                    logger.LogWarning("Plano {CodPlano} é nulo", request.CodPlano);
                    return new ObterServicosPlanoResponse("CodPlano", "Plano retornado é nulo");
                }

                return new ObterServicosPlanoResponse()
                {
                    Plano = plano
                };
            });

        logger.LogInformation("CatalogResponder registrado com sucesso!");

        // Manter o serviço vivo
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _responderDisposable?.Dispose();
        base.Dispose();
    }
}
