using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class QueueService : IQueueService, IDisposable
{
    public Task PublishMessageAsync(string routingKey, object body)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}