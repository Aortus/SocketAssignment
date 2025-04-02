using System.Collections.Immutable;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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

public class DNSRecord
{
    public string? Type { get; set; } // e.g. "A", "AAAA", "CNAME", "MX", etc.
    public string? Name { get; set; } // e.g. "example.com"
    public string? Value { get; set; } // e.g. "192.168.1.10"
}

class ClientUDP
{

    //TODO: [Deserialize Setting.json]
    
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

    public static void start()
    {
        string serverIP = setting.ServerIPAddress;
        int serverPort = setting.ServerPortNumber;
        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ServerEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        string message = "HELLO";
        byte[] data = Encoding.UTF8.GetBytes(message);

        udpSocket.SendTo(data, ServerEP);
        Console.WriteLine($"Sent: {message} to {serverIP}:{serverPort}");

        udpSocket.ReceiveTimeout = 5000;
        byte[] buffer = new byte[1024];
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        int received = udpSocket.ReceiveFrom(buffer, ref remoteEP);
        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
        Console.WriteLine($"Received: {receivedMessage} from {remoteEP}");
        string dnsrecords = @"..\server\DNSrecords.json";
        string dnsrecordsContent = File.ReadAllText(dnsrecords);
        DNSRecord[] dnsRecords = JsonSerializer.Deserialize<DNSRecord[]>(dnsrecordsContent);
        foreach(DNSRecord record in dnsRecords)
        {
            Console.WriteLine($"Sending DNS Record: {record.Name} with Type: {record.Type} and Value: {record.Value}");
            SendDNS(record, udpSocket, ServerEP);
        }
    }

    public static void SendDNS(DNSRecord record, Socket udpSocket, IPEndPoint ServerEP)
    {
        byte[] data = Encoding.UTF8.GetBytes(record.Name);
        udpSocket.SendTo(data, ServerEP);
        byte[] buffer = new byte[1024];
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        int received = udpSocket.ReceiveFrom(buffer, ref remoteEP);
        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
        Console.WriteLine($"Received: {receivedMessage} from {remoteEP}");

        string ackMessage = "ACK";
        byte[] ackData = Encoding.UTF8.GetBytes(ackMessage);
        udpSocket.SendTo(ackData, remoteEP);

        Console.WriteLine("Acknowledgment sent.");

        //TODO: [Receive and print End from server]

    }
}