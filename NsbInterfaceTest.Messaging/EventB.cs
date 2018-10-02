using System;
using NsbInterfaceTest.Shared;

namespace NsbInterfaceTest.Messaging
{
    public class EventB : IUpdateUi
    {
        public Guid Prop1 { get; set; }

        public string Message { get; set; }
    }
}