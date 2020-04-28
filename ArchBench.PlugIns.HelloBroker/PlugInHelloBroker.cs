using System;
using System.IO;
using System.Text;
using HttpServer;
using HttpServer.Sessions;

namespace ArchBench.PlugIns.HelloBroker
{
    public class PlugInHelloBroker : IArchBenchHttpPlugIn
    {
        public string Name { get; } = "PlugIn Hello";
        public string Description { get; } =
            "Responde 'Hi!', sempre que recebe um pedido de '/hello'";
        public string Author { get; } = "Ruben";
        public string Version { get; } = "1.0";

        public bool Enabled { get; set; } = false;
        public IArchBenchPlugInHost Host { get; set; }
        public IArchBenchSettings Settings { get; } = new ArchBenchSettings();

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public bool Process(
            IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            var writer = new StreamWriter(aResponse.Body);
            writer.WriteLine("Hi!");
            writer.Flush();

            var data = "teste";

            var result = Encoding.UTF8.GetBytes(data);

            aResponse.Body.Write(result, 0, result.Length);
            aResponse.Body.Flush();
            aResponse.Send();

            return true;
        }
    }
}
