using System;
using System.Threading.Tasks;
using NsbInterfaceTest.Shared;
using NServiceBus;

namespace NsbInterfaceTest.Receiver
{
    public class Handler : IHandleMessages<IUpdateUi>
    {
        public async Task Handle(IUpdateUi message, IMessageHandlerContext context)
        {
            await Console.Out.WriteLineAsync(message.Message);
        }
    }
}
