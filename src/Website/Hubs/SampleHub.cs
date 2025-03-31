using Microsoft.AspNetCore.SignalR;
using Website.HubEvents;

namespace Website.Hubs
{
    public class SampleHub : Hub
    {

        public async Task NewSampleHubMessage(HubSampleEvent e)
        {
            Console.WriteLine("dsfsjdfhsdk");
            await Clients.All.SendAsync("RecieveSampleHubMessage", e);
        }
    }
}
