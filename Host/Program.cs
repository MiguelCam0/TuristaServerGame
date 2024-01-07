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
                XmlConfigurator.Configure(new FileInfo("D:\\yusgu\\Documents\\UV\\Quinto Semestre\\Tecnologias\\GAMEFINAL\\TuristaServerGame\\Host\\Logs\\XMLFile1.xml"));
                host.Open();
                Console.WriteLine("Server is running. Press Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
