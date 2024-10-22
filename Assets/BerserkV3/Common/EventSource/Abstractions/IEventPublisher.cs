using System.Threading.Tasks;

namespace BerserkV3.Common.EventSource
{
    public interface IEventPublisher
    {
        void Publish(object value);
        Task PublishAsync(object value);
        Task PublishParallelAsync(object value);
    }
}