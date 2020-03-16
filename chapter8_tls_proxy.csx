// dotnet exec CANAPE.Cli.dll proxy/script.csx          (use name script)
#load "parser.csx"

using static System.Console;
using static CANAPE.Cli.ConsoleUtils;

// Create Proxy Template localport of 4444 to destination 127.0.0.1:12345
var template = new FixedProxyTemplate();
template.LocalPort = 4443;
template.Host = "127.0.0.1";
template.Port = 12345;

// adding TLS
var tls = new TlsNetworkLayerFactory();
tls.Config.SpecifyServerCert = true;
tls.Config.ServerCertificateSubject = "CN=127.0.0.1";
tls.Config.ServerProtocol = System.Security.Authentication.SslProtocols.Tls12;

CertificateManager.SetRootCert("ca.pfx");
template.AddLayer(tls);
template.AddLayer<Parser>();

// Create proxy instance and start
var service = template.Create();

// Add an event handler to log a packet. Just print to console.
// service.LogPacketEvent += (s,e) => WritePacket(e.Packet);

// Print to console when a connection is created or closed.
// service.NewConnectionEvent += (s,e) =>
//   WriteLine("New Connection: {0}", e.Description);
// service.CloseConnectionEvent += (s,e) =>
//   WriteLine("Closed Connection: {0}", e.Description);

service.Start();

WriteLine("Creater{0}", service);
WriteLine("Press Enter to exit..");
ReadLine();
service.Stop();

WriteLine("Writing Outbound Packets to packets.bin");
service.Packets.WriteToFile("packets.bin", "Out");

