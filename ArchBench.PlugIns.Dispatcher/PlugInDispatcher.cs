using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HttpServer;
using HttpServer.Sessions;

namespace ArchBench.PlugIns.Dispatcher
{
    public class PlugInDispatcher : IArchBenchHttpPlugIn
    {
        public string Name => "Dispatcher Pattern";
        public string Description => "Implements the Dispatcher Architectural Pattern.";
        public string Author => "Ruben Freitas";
        public string Version => "1.0";

        public bool Enabled { get; set; } = false;
        public IArchBenchPlugInHost Host { get; set; }
        public IArchBenchSettings Settings { get; } = new ArchBenchSettings();

        public TcpListener Listener { get; private set; }
        public Thread Thread { get; private set; }

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

            Host.Logger.WriteLine(
                $"Dispatching to server on port { Servers[index].Value }");

            var redirection = new StringBuilder();
            redirection.Append($"http://{ Servers[index].Key }:{ Servers[index].Value }");
            redirection.Append(aRequest.Uri.AbsolutePath);

            var count = aRequest.QueryString.Count();
            if (count > 0)
            {
                redirection.Append('?');
                foreach (HttpInputItem item in aRequest.QueryString)
                {
                    redirection.Append($"{ item.Name }={ item.Value }");
                    if (--count > 0) redirection.Append('&');
                }
            }

            aResponse.Redirect(redirection.ToString());

            return true;
        }

        public List<KeyValuePair<string, int>> Servers { get; }
        = new List<KeyValuePair<string, int>>();

        private int NextServer { get; set; }

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

    }
}
