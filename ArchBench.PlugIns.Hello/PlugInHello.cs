using System;
using System.IO;
using HttpServer;
using HttpServer.Sessions;

namespace ArchBench.PlugIns.Hello
{
    public class PlugInHello : IArchBenchHttpPlugIn
    {
        public string Name { get; }  = "PlugIn Hello";
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
            if (aRequest.Uri.AbsolutePath.StartsWith(
             "/hello", StringComparison.InvariantCultureIgnoreCase))
            {
                var writer = new StreamWriter(aResponse.Body);
                writer.WriteLine("Hi!");
                writer.Flush();

                return true;
            }
            return false;
        }
    }
}
