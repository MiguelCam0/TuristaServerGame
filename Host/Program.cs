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
using Services.DataBaseManager;
using System.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using log4net;
using log4net.Config;
using System.IO;

namespace Host
{
    internal class Program
    {
        public static List<InstanceContext> activeInstanceContexts = new List<InstanceContext>();
        
        static void Main(string[] args)
        {
            Console.WriteLine($"Directorio de Trabajo Actual: {Environment.CurrentDirectory}");
            using (ServiceHost host = new ServiceHost(typeof(Services.DataBaseManager.PlayerManager)))
            {
                var playerManager = new PlayerManager();
                playerManager.startLog();
                //Console.WriteLine("Correo = " + playerManager.SendEmail("Hola", "yusgus02@gmail.com"));
                host.Open();
                Console.WriteLine("Server is running. Press Enter to exit.");
                Console.ReadLine();
            }
        }

        public static void UpdateBaseAddressesInAppConfig()
        {
            try
            {
                string hostName = Dns.GetHostName();
                string newIpAddress = Dns.GetHostByName(hostName).AddressList[0].ToString();
                string configFilePath = "D:\\Proyectos .NET\\Juego\\TuristaServerGame\\Host\\App.config";
                XDocument doc = XDocument.Load(configFilePath);

                var baseAddresses = doc.Descendants("baseAddresses").Elements("add");

                foreach (var baseAddressElement in baseAddresses)
                {
                    string oldBaseAddress = baseAddressElement.Attribute("baseAddress").Value;
                    Uri oldUri = new Uri(oldBaseAddress);
                    string oldScheme = oldUri.Scheme;
                    string oldHost = oldUri.Host;
                    Uri newUri = new UriBuilder(oldScheme, newIpAddress)
                    {
                        Path = oldUri.PathAndQuery,
                        Port = oldUri.Port
                    }.Uri;
                    string newBaseAddress = newUri.ToString();
                    baseAddressElement.SetAttributeValue("baseAddress", newBaseAddress);
                }

                doc.Save(configFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating base addresses: " + ex.Message);
            }
        }

    }
}
