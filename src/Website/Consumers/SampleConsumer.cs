using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Shared.Events;
using Website.HubEvents;
using Website.Hubs;

namespace Website.Consumers
{
    public class SampleConsumer : IConsumer<SampleEvent>
    {
        private readonly IHubContext<SampleHub> _hub;
        public SampleConsumer(IHubContext<SampleHub> hub)
        {
            _hub = hub;
        }

        public async Task Consume(ConsumeContext<SampleEvent> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"{context.Message.Name} - {context.Message.Id}");

            var hubSampleEvent = new HubSampleEvent
            {
                Id = context.Message.Id,
                Name = context.Message.Name
            };

            // await _hub.Clients.All.SendAsync("NewSampleHubMessage", hubSampleEvent);
            await _hub.Clients.All.SendAsync("RecieveSampleHubMessage", hubSampleEvent);
        }
    }
}
