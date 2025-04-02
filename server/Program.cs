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

    // TODO: [Read the JSON file and return the list of DNSRecords]
    public static DNSRecord[] ReadDNSRecords()
    {
        string dnsRecords = @"../server/DNSrecords.json";
        string jsonContent = File.ReadAllText(dnsRecords);
        return JsonSerializer.Deserialize<DNSRecord[]>(jsonContent);
    }


    public static void start()
    {
        // TODO: [Create a socket and endpoints and bind it to the server IP address and port number]
        Console.WriteLine("Waiting");
        int port = setting.ServerPortNumber;
        string serverIP = setting.ServerIPAddress;
        string clientIP = setting.ClientIPAddress;
        int portClient = setting.ClientPortNumber;
        IPAddress ipAddressServer = IPAddress.Parse(serverIP);
        IPAddress iPAddressClient = IPAddress.Parse(clientIP);

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint serverEndPoint = new IPEndPoint(ipAddressServer, port);
        serverSocket.Bind(serverEndPoint);
        // TODO:[Receive and print a received Message from the client]
        
        byte[] buffer = new byte[1000]; 
        IPEndPoint clientEndPoint = new IPEndPoint(iPAddressClient, portClient); 
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            int receivedBytes = serverSocket.ReceiveFrom(buffer, ref remoteEP);
            string receivedJson = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message receivedMessage = JsonSerializer.Deserialize<Message>(receivedJson);
            Console.WriteLine($"Received Content Type: {receivedMessage.Content.GetType()}");
            Console.WriteLine($"Received message: Type={receivedMessage.MsgType} {receivedMessage.Content}");

            Message responseMessage = null;

            
            switch (receivedMessage.MsgType)
            {
                case MessageType.Hello:
                    responseMessage = new Message { MsgId = 2, MsgType = MessageType.Welcome, Content = "Welcome!" };
                    break;
                case MessageType.DNSLookup:
                    if (receivedMessage.Content is JsonElement contentElement)
                    {
                        DNSRecord dnsRecord = JsonSerializer.Deserialize<DNSRecord>(contentElement.GetRawText());
                        string domainName = dnsRecord.Name;
                        string recordType = dnsRecord.Type;

                            var dnsRecords = ReadDNSRecords();
                            foreach(DNSRecord item in dnsRecords)
                            {
                                Console.WriteLine(item.Name);
                                if (item.Name == domainName && item.Type == recordType)
                                {
                                    responseMessage = new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.DNSLookupReply, Content = item };
                                    break;
                                }
                            }
                            if (responseMessage is null)
                            {
                                responseMessage = new Message { MsgId = receivedMessage.MsgId + 250000, MsgType = MessageType.Error, Content = "Domain not found" };            
                            }
                    }
                    else
                    {
                        responseMessage = new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.Error, Content = "Content is wrong"};
                    }
                    break;
                case MessageType.End:
                    break;
                case MessageType.Ack:
                    break;
                default:
                    responseMessage = new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.Error, Content = "Unknown request type." };
                    break;
            }
                
            if (responseMessage != null)
            {
                string responseJson = JsonSerializer.Serialize(responseMessage);
                Console.WriteLine($"Sending response: {responseJson}");
                byte[] responseData = Encoding.UTF8.GetBytes(responseJson);
                serverSocket.SendTo(responseData, remoteEP);
            }
            else
            {
                Console.WriteLine("Waiting for the next message...");
            }
        }
    }
}