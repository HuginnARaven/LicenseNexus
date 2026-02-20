using System.Threading.Channels;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Infrastructure.Services;

public class InMemoryEventBus : IEventPublisher
{
    private readonly Channel<IEntityUpdatedEvent> _channel = Channel.CreateUnbounded<IEntityUpdatedEvent>();
    
    public ChannelReader<IEntityUpdatedEvent> Reader => _channel.Reader;

    public async ValueTask PublishAsync(IEntityUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(domainEvent, cancellationToken);
    }
}
