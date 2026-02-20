using LicenseNexus.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LicenseNexus.Infrastructure.Services;

public class EventProcessorBackgroundService : BackgroundService
{
    private readonly InMemoryEventBus _eventBus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventProcessorBackgroundService> _logger;

    public EventProcessorBackgroundService(
        InMemoryEventBus eventBus, 
        IServiceScopeFactory scopeFactory,
        ILogger<EventProcessorBackgroundService> logger)
    {
        _eventBus = eventBus;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Event Processor is starting.");
        
        await foreach (var domainEvent in _eventBus.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var productSyncService = scope.ServiceProvider.GetRequiredService<IProductSyncService>();
                Task updateTask = domainEvent switch
                {
                    VendorUpdatedEvent v => productSyncService.UpdateVendorAsync(v.vendor, stoppingToken),
                    _ => Task.CompletedTask
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event");
            }
        }
    }
}