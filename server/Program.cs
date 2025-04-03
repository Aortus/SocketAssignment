using System;
using System.Data;
using System.Data.SqlTypes;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using LibData;

// ReceiveFrom();
class Program
{
    static void Main(string[] args)
    {
        ServerUDP.start();
    }
}

public class Setting
{
    public int ServerPortNumber { get; set; }
    public string? ServerIPAddress { get; set; }
    public int ClientPortNumber { get; set; }
    public string? ClientIPAddress { get; set; }
}


class ServerUDP
{
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

    private static DNSRecord[] ReadDNSRecords()
    {
        string dnsRecords = @"../server/DNSrecords.json";
        string jsonContent = File.ReadAllText(dnsRecords);
        return JsonSerializer.Deserialize<DNSRecord[]>(jsonContent);
    }


    public static void start()
    {
        Console.WriteLine($"Server initialization complete. Awaiting incoming messages.");
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint serverEndPoint = EndPointSetup(setting.ServerPortNumber, setting.ServerIPAddress);
        serverSocket.Bind(serverEndPoint);
        
        byte[] buffer = new byte[1024]; 
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Message receivedMessage = BytesToMessage(buffer, serverSocket.ReceiveFrom(buffer, ref remoteEP));
            Console.WriteLine($"Message Received: ID: {receivedMessage.MsgId}, MsgType: {receivedMessage.MsgType}, Content: {receivedMessage.Content}");

            Message responseMessage = HandleMessages(receivedMessage);
                
            if (responseMessage != null)
            {
                byte[] responseData = MessageToBytes(responseMessage);
                serverSocket.SendTo(responseData, remoteEP);
                Console.WriteLine($"Message Sent: {responseMessage.MsgId}, MsgType: {responseMessage.MsgType}, Content: {FormatContent(responseMessage.Content)} \n");
            }
            else
            {
                Console.WriteLine("Waiting for the next message.");
            }
        }
    }

    private static IPEndPoint EndPointSetup(int port, string IP)
    {
        IPAddress IPAddress = IPAddress.Parse(IP);
        return new IPEndPoint(IPAddress, port);
    }

    private static Message HandleMessages(Message receivedMessage)
    {
        switch (receivedMessage.MsgType)
        {
            case MessageType.Hello:
                return new Message { MsgId = 2, MsgType = MessageType.Welcome, Content = "Welcome!" };
            case MessageType.DNSLookup:
                return DNSLookup(receivedMessage);
            case MessageType.End:
                Console.WriteLine($"Connection with {setting.ClientIPAddress} has been closed.");
                return null;
            case MessageType.Ack:
                Console.WriteLine($"Acknowledgment from {setting.ClientIPAddress} received, it contains: {receivedMessage.Content}");
                return null;
            default:
                return new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.Error, Content = "Unknown request type." };
        }
    }

    private static byte[] MessageToBytes(Message message)
    {
        string serializedMessage = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(serializedMessage);
        return data;
    }

    private static Message BytesToMessage(byte[] buffer, int received)
    {
        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
        Message deserializedMessage = JsonSerializer.Deserialize<Message>(receivedMessage);
        return deserializedMessage;
    }

    private static Message DNSLookup(Message receivedMessage)
    {
        if (receivedMessage.Content is not JsonElement contentElement)
            return new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.Error, Content = "Invalid request." };

        DNSRecord dnsRecord = JsonSerializer.Deserialize<DNSRecord>(contentElement.GetRawText());
        string domainName = dnsRecord.Name;
        string recordType = dnsRecord.Type;

        var dnsRecords = ReadDNSRecords();
        DNSRecord foundRecord = dnsRecords.FirstOrDefault(item => item.Name == domainName && item.Type == recordType);

        if (foundRecord != null)
        {
            return new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.DNSLookupReply, Content = foundRecord };
        }
        else
        {
            return new Message { MsgId = receivedMessage.MsgId + 250000, MsgType = MessageType.Error, Content = "Domain not found." };
        }
    }

    private static string FormatContent(object content)
    {
        try
        {
            if (content is DNSRecord dnsRecord)
            {
                return JsonSerializer.Serialize(dnsRecord, new JsonSerializerOptions());
            }
            return content.ToString() ?? "No content";
        }
        catch
        {
            return "Invalid content";
        }
    }
}