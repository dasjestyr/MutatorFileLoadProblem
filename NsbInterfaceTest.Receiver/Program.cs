using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.MessageMutator;

namespace NsbInterfaceTest.Receiver
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var config = new EndpointConfiguration("NsbInterfaceTest.Receiver");
            config.UseTransport<LearningTransport>();
            config.UsePersistence<LearningPersistence>();
            config.RegisterMessageMutator(new Mutator());

            var endpoint = await Endpoint.Start(config).ConfigureAwait(false);

            Console.ReadKey();
        }
    }
}
