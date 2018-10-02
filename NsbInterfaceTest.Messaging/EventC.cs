using NServiceBus;

namespace NsbInterfaceTest.Messaging
{
    public class EventC : IEvent
    {
        public string Message { get; set; }
    }
}