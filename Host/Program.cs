using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace Host
{
    internal class Program
    {
        public static List<InstanceContext> activeInstanceContexts = new List<InstanceContext>();
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(Services.DataBaseManager.PlayerManager)))
            {
                host.Description.Behaviors.Add(new ConnectionLoggingBehavior());
                host.Open();
                Console.WriteLine("Server is running. Press Enter to exit.");
                Console.ReadLine();
            }
        }
    }

    public class ConnectionLoggingBehavior : IServiceBehavior
    {
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new ConnectionLoggingMessageInspector());
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }

    public class ConnectionLoggingMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            OperationContext context = OperationContext.Current;
            if (context != null)
            {
                var endpointProperty = context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (endpointProperty != null)
                {
                    string clientIpAddress = endpointProperty.Address;
                    Console.WriteLine("Nueva conexión entrante desde " + clientIpAddress);

                    // Agregar el InstanceContext a la lista de instancias activas
                    Program.activeInstanceContexts.Add(instanceContext);
                }
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}
