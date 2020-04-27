using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HttpServer;
using HttpServer.Sessions;

namespace ArchBench.PlugIns.Broker
{
    public class PlugInBroker : IArchBenchHttpPlugIn
    {
        public string Name => "Broker Pattern";
        public string Description => "Implements the Broker Architectural Pattern.";
        public string Author => "Ruben Freitas";
        public string Version => "1.0";

        public bool Enabled { get; set; } = false;
        public IArchBenchPlugInHost Host { get; set; }
        public IArchBenchSettings Settings { get; } = new ArchBenchSettings();

        public TcpListener Listener { get; private set; }
        public Thread Thread { get; private set; }

        public List<KeyValuePair<string, int>> Servers { get; } = new List<KeyValuePair<string, int>>();
        private int NextServer { get; set; }

        public void Initialize()
        {
            Listener = new TcpListener(IPAddress.Any, 9000);
            Thread = new Thread(ReceiveThreadFunction) { IsBackground = true };
            Thread.Start();
        }

        public void Dispose()
        {
        }

        bool IArchBenchHttpPlugIn.Process(IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession)
        {
            var index = GetNextServer();
            if (index == -1) return false;

            string sourceUrl = $"{aRequest.Uri.Host}:{aRequest.Uri.Port}";
            string targetUrl = $"http://{Servers[index].Key}:{Servers[index].Value}{aRequest.UriPath}";

            Host.Logger.WriteLine($"Passing from {sourceUrl} to {targetUrl}");

            //Host.Logger.WriteLine($"Dispatching to server on port { Servers[index].Value }");


            Uri url = new Uri(targetUrl);
            byte[] final = null;
            WebClient webClient = new WebClient();

            try
            {
                //Passar headers
                if (aRequest.Headers["Cookie"] != null)
                {
                    webClient.Headers.Add("Cookie", aRequest.Headers["Cookie"]);
                }

                //Se POST manda os valores do form, caso contrário 
                if (aRequest.Method == Method.Post)
                {
                    NameValueCollection formValues = new NameValueCollection();
                    formValues = GetFormValues(aRequest.Form);
                    final = webClient.UploadValues(url, formValues);
                }
                else
                {
                    final = webClient.DownloadData(url);
                }

                aResponse.ContentType = webClient.ResponseHeaders[HttpResponseHeader.ContentType];

                if (webClient.ResponseHeaders["Set-Cookie"] != null)
                {
                    aResponse.AddHeader("Set-Cookie", webClient.ResponseHeaders["Set-Cookie"]);
                    // guardar o registo, para daí em diante associar a este Server
                }

                if (aResponse.ContentType.Contains("text") || aResponse.ContentType.Contains("html"))
                {
                    var data = Encoding.UTF8.GetString(final, 0, final.Length);
                    data = data.Replace($"http://{Servers[index].Key}:{Servers[index].Value}/", "/");
                    /*data = data.Replace( "href=\"/", "href=\"/" + aRequest.UriParts[0] + "/" );
                    data = data.Replace( "src=\"/", "src=\"/" + aRequest.UriParts[0] + "/" );
                    data = data.Replace( "action=\"/", "action=\"/" + aRequest.UriParts[0] + "/" );*/
                    final = Encoding.UTF8.GetBytes(data);
                    aResponse.AddHeader("Content-Length", final.Length.ToString());
                }
 
                aResponse.Body.Write(final, 0, final.Length);
                aResponse.Body.Flush();
                aResponse.Send();

            }
            catch (Exception e)
            {
                Host.Logger.WriteLine("Error on plugin Broker : {0}", e.Message);
            }

            return true;
        }

        private int GetNextServer()
        {
            if (Servers.Count == 0) return -1;
            NextServer = (NextServer + 1) % Servers.Count;
            return NextServer;
        }

        private void ReceiveThreadFunction()
        {
            try
            {
                // Start listening for client requests.
                Listener.Start();

                // Buffer for reading data
                byte[] bytes = new byte[256];

                // Enter the listening loop.
                while (true)
                {
                    // Perform a blocking call to accept requests.
                    var client = Listener.AcceptTcpClient();

                    // Get a stream object for reading and writing
                    var stream = client.GetStream();

                    int count = stream.Read(bytes, 0, bytes.Length);
                    if (count != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        string data = Encoding.ASCII.GetString(bytes, 0, count);

                        var parts = data.Split(':');
                        switch (parts[0])
                        {
                            case "+":
                                Regist(parts[1], int.Parse(parts[2]));
                                break;
                            case "-":
                                Regist(parts[1], int.Parse(parts[2]));
                                break;
                        }
                    }

                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Host.Logger.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                Listener.Stop();
            }
        }

        private void Regist(string aAddress, int aPort)
        {
            if (Servers.Any(p => p.Key == aAddress && p.Value == aPort)) return;
            Servers.Add(new KeyValuePair<string, int>(aAddress, aPort));
            Host.Logger.WriteLine("Added server {0}:{1}.", aAddress, aPort);
        }

        private void Unregist(string aAddress, int aPort)
        {
            Host.Logger.WriteLine(
                Servers.Remove(new KeyValuePair<string, int>(aAddress, aPort))
                ? "Removed server {0}:{1}."
                : "The server {0}:{1} is not registered.", aAddress, aPort);
        }

        private NameValueCollection GetFormValues(IEnumerable form)
        {
            var collection = new NameValueCollection();
            foreach (HttpInputItem el in form)
            {
                collection.Add(el.Name, el.Value);
            }

            return collection;
        }
    }
}
