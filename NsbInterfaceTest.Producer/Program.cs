using System;
using System.Threading.Tasks;
using NsbInterfaceTest.Messaging;
using NServiceBus;

namespace NsbInterfaceTest.Producer
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.Title = "Producer";

            var config = new EndpointConfiguration("NsbInterfaceTest.Producer");
            config.UseTransport<LearningTransport>();
            config.UsePersistence<LearningPersistence>();

            var endpoint = await Endpoint.Start(config).ConfigureAwait(false);

            await endpoint.Publish<EventA>(e =>
            {
                e.Prop1 = 1;
                e.Message = "I'm event A";
            });

            Console.ReadKey();
        }
    }
}
