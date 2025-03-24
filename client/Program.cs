using System.Collections.Immutable;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LibData;

// SendTo();
class Program
{
    static void Main(string[] args)
    {
        ClientUDP.start();
    }
}

public class Setting
{
    public int ServerPortNumber { get; set; }
    public string? ServerIPAddress { get; set; }
    public int ClientPortNumber { get; set; }
    public string? ClientIPAddress { get; set; }
}

class ClientUDP
{

    //TODO: [Deserialize Setting.json]
    
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

    public static void start()
    {
        //TODO: [Create endpoints and socket]
        int port = setting.ServerPortNumber;
        string serverIp = setting.ServerIPAddress;

        using (Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);

            string message = "HELLO";
            byte[] data = Encoding.UTF8.GetBytes(message);

            udpSocket.SendTo(data, serverEndPoint);
            Console.WriteLine($"Sent: {message} to {serverIp}:{port}");

            udpSocket.ReceiveTimeout = 5000;
            byte[] buffer = new byte[1024];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            int received = udpSocket.ReceiveFrom(buffer, ref remoteEP);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Received: {receivedMessage} from {remoteEP}");
        }
        // TODO: [Create and send DNSLookup Message]
        string dnsrecords = @"../DNSRecords.json";
        string dnsrecordsContent = File.ReadAllText(dnsrecords);
        DNSRecord[] dnsRecords = JsonSerializer.Deserialize<DNSRecord[]>(dnsrecordsContent);
        


        //TODO: [Receive and print DNSLookupReply from server]


        //TODO: [Send Acknowledgment to Server]

        // TODO: [Send next DNSLookup to server]
        // repeat the process until all DNSLoopkups (correct and incorrect onces) are sent to server and the replies with DNSLookupReply

        //TODO: [Receive and print End from server]





    }
}