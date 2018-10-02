using System;
using System.Reflection;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.MessageMutator;

namespace NsbInterfaceTest.Receiver
{
    public class Mutator : IMutateIncomingTransportMessages
    {
        public async Task MutateIncoming(MutateIncomingTransportMessageContext context)
        {
            if (!context.Headers.TryGetValue(Headers.EnclosedMessageTypes, out var messageType))
                return;

            var type = Type.GetType(
                typeName: messageType,
                assemblyResolver: AssemblyResolver,
                typeResolver: TypeResolver,
                throwOnError: true);

            if (type == null)
            {
                throw new Exception($"Could not determine type: {messageType}");
            }

            await Console.Out.WriteLineAsync($"Resolved {type.AssemblyQualifiedName}");
            context.Headers[Headers.EnclosedMessageTypes] = type.AssemblyQualifiedName;
        }

        private Type TypeResolver(Assembly assembly, string typeName, bool assemblyPassed)
        {
            if (typeName == "EventA")
            {
                return typeof(MutatedEventA);
            }

            if (typeName == "EventB")
            {
                return typeof(MutatedEventB);
            }

            if (assemblyPassed)
            {
                return assembly.GetType(typeName);
            }

            return Type.GetType(typeName);
        }

        private Assembly AssemblyResolver(AssemblyName assemblyName)
        {
            if (assemblyName.Name == "NsbInterfaceTest.Messaging")
            {
                return Assembly.Load("NsbInterfaceTest.Receiver");
            }

            return Assembly.Load(assemblyName);
        }
    }

    public class MutatedEventA : IMessage
    {
        public int Prop1 { get; set; }

        public string Message { get; set; }
    }

    public class MutatedEventB : IMessage
    {
        public Guid Prop1 { get; set; }

        public string Message { get; set; }
    }
}
