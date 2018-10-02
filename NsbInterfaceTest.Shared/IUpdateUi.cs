using NServiceBus;

namespace NsbInterfaceTest.Shared
{
    public interface IUpdateUi : IEvent
    {
        string Message { get; set; }
    }
}