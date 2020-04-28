using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpServer;
using HttpServer.Sessions;

namespace ArchBench.PlugIns.DispatcherServer
{
    public class PlugInDispatcherServer : IArchBenchHttpPlugIn
    {
        public string Name { get; } = "Dispatcher Server Plug-in";
        public string Description { get; } = "Implements de registration behaviors";
        public string Author { get; } = "Ruben Freitas";
        public string Version { get; } = "1.0";

        public bool OnService { get; set; }
        public bool Enabled
        {
            get => OnService;
            set => Registration(value);
        }

        public IArchBenchPlugInHost Host { get; set; }

        public IArchBenchSettings Settings { get; } = new ArchBenchSettings();

        public void Initialize()
        {
            Settings["DispatcherAddress"] = "127.0.0.1:9000";
            Settings["ServerPort"] = "8081";
        }

        public void Dispose()
        {
        }

        private void Registration(bool aOnService)
        {
            if (aOnService == OnService) return;
            OnService = aOnService;

            try
            {
                if (string.IsNullOrEmpty(Settings["DispatcherAddress"]))
                {
                    Host.Logger.WriteLine("The Dispatcher's Address is not defined.");
                    return;
                }

                var parts = Settings["DispatcherAddress"].Split(':');
                if (parts.Length != 2)
                {
                    Host.Logger.Write(
                        "The Dispatcher Address format is not well defined (must be <ip>:<port>)");
                    Host.Logger.WriteLine($"{ Settings["DispatcherAddress"] }");
                    return;
                }

                if (!int.TryParse(parts[1], out int port))
                {
                    Host.Logger.Write(
                        "The Dispatcher Address format is not well defined (must be <ip>:<port>)");
                    Host.Logger.WriteLine($"A number is expected on <port> : { parts[1] }");
                }

                var client = new TcpClient(parts[0], port);

                var operation = OnService ? '+' : '-';
                var data = Encoding.ASCII.GetBytes(
                    $"{ operation }:{ GetIP() }:{ Settings["ServerPort"] }");

                var stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                client.Close();
            }
            catch (SocketException e)
            {
                Host.Logger.WriteLine("SocketException: {0}", e);
            }
        }

        private static string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
            }
            return "0.0.0.0";
        }

        public bool Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            throw new NotImplementedException();
        }
    }
}
