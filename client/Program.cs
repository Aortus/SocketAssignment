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
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

    public static void start()
    {
        string serverIP = setting.ServerIPAddress;
        int serverPort = setting.ServerPortNumber;
        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ServerEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        Message msg = new()
        {
            MsgId = 1,
            MsgType = MessageType.Hello,
            Content = "Hello from client!"
        };
        byte[] data = MessageToBytes(msg);

        udpSocket.SendTo(data, ServerEP);
        Console.WriteLine($"Sent: {msg.Content} to {serverIP}:{serverPort}");

        udpSocket.ReceiveTimeout = 5000;
        byte[] buffer = new byte[1024];
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        int received = udpSocket.ReceiveFrom(buffer, ref remoteEP);
        Message deserializedMessage = BytesToMessage(buffer, received);
        Console.WriteLine($"Received: {deserializedMessage.Content} from {remoteEP}");
        Console.ReadLine();

        DNSRecord[] dnsRecords = GetRecords();
        int count = 10;
        foreach(DNSRecord record in dnsRecords)
        {
            count++;
            Console.WriteLine($"Sending DNS Record: {record.Name}");
            SendDNS(record, udpSocket, ServerEP, count);
            Console.WriteLine($"Press enter to continue to the next record...");
            Console.ReadLine(); // buffer to see the output
        }
        End(udpSocket);
    }

    public static void SendDNS(DNSRecord record, Socket udpSocket, IPEndPoint ServerEP, int count)
    {
        // {"Type":"A","Name":"www.outlook.com","Value":"192.168.1.10","TTL":3600,"Priority":null}
        Message newmsg = new()
        {
            MsgId = count,
            MsgType = MessageType.DNSLookup,
            Content = new DNSRecord
            {
                Type = record.Type,
                Name = record.Name,
                Value = null,
                TTL = null,
                Priority = null
            }
        };
        byte[] data = MessageToBytes(newmsg);
        udpSocket.SendTo(data, ServerEP);

        byte[] buffer = new byte[1024];
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        int received = udpSocket.ReceiveFrom(buffer, ref remoteEP);
        DNSRecord receivedMessage = MSGtoDNS(buffer, received);

        if (receivedMessage == null)
        {
            Console.WriteLine("Received message is null.");
            return;
        }
        
        if (receivedMessage != null)
        {
            Console.WriteLine($"Received: Type={receivedMessage.Type}, Name={receivedMessage.Name}, Value={receivedMessage.Value}, TTL={receivedMessage.TTL}, Priority={receivedMessage.Priority} from {remoteEP}");
        }
        else
        {
            Console.WriteLine("Failed to parse received DNS record.");
            return;
        }

        Message Ackmsg = new()
        {
            MsgId = 1000 + count,
            MsgType = MessageType.Ack,
            Content = $"{newmsg.MsgId} has been received."
        };
        byte[] ackData = MessageToBytes(Ackmsg);
        udpSocket.SendTo(ackData, remoteEP);

        Console.WriteLine("Acknowledgment sent.");

    }

    public static DNSRecord[] GetRecords()
    {
        string dnsrecords = @"..\server\DNSrecords.json";
        string dnsrecordsContent = File.ReadAllText(dnsrecords);
        DNSRecord[] dnsRecords = JsonSerializer.Deserialize<DNSRecord[]>(dnsrecordsContent);
        return dnsRecords;
    }

    public static byte[] MessageToBytes(Message message)
    {
        string serializedMessage = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(serializedMessage);
        return data;
    }

    public static Message BytesToMessage(byte[] buffer, int received)
    {
        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
        Message deserializedMessage = JsonSerializer.Deserialize<Message>(receivedMessage);
        return deserializedMessage;
    }

    public static DNSRecord MSGtoDNS(byte[] buffer, int received)
    {
        try
        {
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Received Message: {receivedMessage}");

            Message receivedMsg = JsonSerializer.Deserialize<Message>(receivedMessage);

            if (receivedMsg?.Content == null)
            {
                Console.WriteLine("Error: Received message does not contain valid content.");
                return null;
            }

            // Deserialize Content into DNSRecord
            DNSRecord dnsRecord = JsonSerializer.Deserialize<DNSRecord>(receivedMsg.Content.ToString());
            
            return dnsRecord;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
            return null;
        }
    }

    public static void End(Socket udpSocket)
    {
        // Close the socket and release resources
        udpSocket.Shutdown(SocketShutdown.Both);
        Console.WriteLine("Socket shutdown.");
    }
}
