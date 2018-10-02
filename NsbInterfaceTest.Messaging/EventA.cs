using NsbInterfaceTest.Shared;

namespace NsbInterfaceTest.Messaging
{
    public class EventA : IUpdateUi
    {
        public int Prop1 { get; set; }

        public string Message { get; set; }
    }
}
