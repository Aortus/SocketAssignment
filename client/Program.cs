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
class UDPClient
{
    private readonly Socket _socket;
    private readonly IPEndPoint _serverEP;

    public UDPClient(string serverIP, int serverPort)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _serverEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
    }

    public void Send(Message message)
    {
        byte[] data = MessageHandler.SerializeMessage(message);
        _socket.SendTo(data, _serverEP);
    }

    public void Send(DNSRecord dnsRecord, int count)
    {
        Message message = new()
        {
            MsgId = count,
            MsgType = MessageType.DNSLookup,
            Content = new DNSRecord
            {
                Type = dnsRecord.Type,
                Name = dnsRecord.Name,
                Value = null,
                TTL = null,
                Priority = null
            }
        };
        Send(message);
    }

    public Message Receive()
    {
        byte[] buffer = new byte[1024];
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        int received = _socket.ReceiveFrom(buffer, ref remoteEP);
        return MessageHandler.DeserializeMessage(buffer, received);
    }

    public void Close()
    {
        Message endMessage = new()
        {
            MsgId = 0,
            MsgType = MessageType.End,
            Content = "End of communication."
        };
        Send(endMessage);
        _socket.Shutdown(SocketShutdown.Both);
        Console.WriteLine("Socket closed.");
    }
}

class MessageHandler
{
    public static Message CreateMessage(int msgId, MessageType msgType, object content)
    {
        return new Message
        {
            MsgId = msgId,
            MsgType = msgType,
            Content = content
        };
    }

    public static byte[] SerializeMessage(Message message)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    }

    public static Message DeserializeMessage(byte[] buffer, int received)
    {
        string json = Encoding.UTF8.GetString(buffer, 0, received);
        Message? message = JsonSerializer.Deserialize<Message>(json);
        if (message != null)
        {
            message.Content = FormatContent(message.Content);
        }
        return message ?? new Message { MsgId = 0, MsgType = MessageType.Error, Content = "Invalid message received." };
    }

    private static object FormatContent(object? content)
    {
        if (content is JsonElement jsonElement)
        {
            return jsonElement.GetRawText();
        }
        return content ?? "No content";
    }
}

class DNSService
{
    private readonly DNSRecord[] _dnsRecords;

    public DNSService()
    {
        string filePath = @"..\server\DNSrecords.json";
        string content = File.ReadAllText(filePath);
        _dnsRecords = JsonSerializer.Deserialize<DNSRecord[]>(content);
    }
}


class ClientUDP
{    
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

    public static void start()
    {
        UDPClient udpClient = new UDPClient(setting.ServerIPAddress, setting.ServerPortNumber);
        Message msg = MessageHandler.CreateMessage(1, MessageType.Hello, "Hello from client!");
        udpClient.Send(msg);

        Console.WriteLine($"Sent: {msg.Content} to {setting.ServerIPAddress}:{setting.ServerIPAddress}");

        Message receivemessage = udpClient.Receive();
        // byte[] buffer = new byte[1024];
        // EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        // int received = udpSocket.ReceiveFrom(buffer, ref remoteEP);
        // Message deserializedMessage = BytesToMessage(buffer, received);
        Console.WriteLine($"Received: {receivemessage.Content}");
        Console.ReadLine();

        DNSRecord[] dnsRecords = GetRecords();

        SendingDNSRecords(udpClient, dnsRecords);
        udpClient.Close();
    }

        private static void SendingDNSRecords(UDPClient udpClient, DNSRecord[] dnsRecords)
        {
        int count = 10;
        foreach(DNSRecord record in dnsRecords)
        {
            count++;
            Console.WriteLine($"Sending DNS Record: {record.Name}");
            udpClient.Send(record, count);
            Message returnmessage = udpClient.Receive();
            Console.WriteLine($"Received:");
            Console.WriteLine($"{FormatContent(returnmessage.Content)}");
            udpClient.Send(new Message
            {
                MsgId = count + 1000,
                MsgType = MessageType.Ack,
                Content = $"{returnmessage.MsgId}"
            });
            Console.WriteLine($"Press enter to continue to the next record...");
            Console.ReadLine(); // buffer to see the output
        }

    }

    public static DNSRecord[] GetRecords()
    {
        string dnsrecords = @"..\server\DNSrecords.json";
        string dnsrecordsContent = File.ReadAllText(dnsrecords);
        DNSRecord[] dnsRecords = JsonSerializer.Deserialize<DNSRecord[]>(dnsrecordsContent);
        // Adding some wrong records for testing
        DNSRecord wrongrec1 = new() { Type = "A", Name = "www.newsite.com", Value = "192.168.1.60", TTL = 3600 };
        DNSRecord wrongrec2 =new() { Type = "MX", Name = "newdomain.com", Value = "mail.newdomain.com", Priority = 5, TTL = 3600 };
        dnsRecords = dnsRecords.Append(wrongrec1).Append(wrongrec2).ToArray();
        return dnsRecords;
    }

    private static string FormatContent(object content)
    {
        try
        {
            JsonDocument doc = JsonDocument.Parse((string)content);
            StringBuilder formatted = new StringBuilder();
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                formatted.AppendLine($"{property.Name}: {property.Value}");
            }
            return formatted.ToString();
        }
        catch
        {
            return content.ToString() ?? "No content";
        }
    }
}
