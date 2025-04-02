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

            Console.WriteLine($"Received message: Type={receivedMessage.MsgType}");
                
            Message responseMessage = null;
            
            switch (receivedMessage.MsgType)
            {
                case MessageType.Hello:
                    responseMessage = new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.Welcome, Content = "Welcome!" };
                    break;
                case MessageType.DNSLookup:
                    string domainName = receivedMessage.Content?.ToString().Trim();
                    Console.WriteLine(domainName);
                    var dnsRecords = ReadDNSRecords();
                    foreach(DNSRecord item in dnsRecords)
                    {
                        Console.WriteLine(item.Name);
                        if (item.Name.Trim() == domainName)
                        {
                            responseMessage = new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.DNSLookupReply, Content = item };
                            Console.WriteLine("MESSAGE MADEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEee");
                            break;
                        }
                    }

                    if (responseMessage is null)
                    {
                        responseMessage = new Message { MsgId = receivedMessage.MsgId, MsgType = MessageType.Error, Content = "Domain not found" };            
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
                Console.WriteLine("No response sent. Waiting for the next message...");
            }
        }
    }
} 


//                     if (receivedMessage.MsgType == MessageType.DNSLookup)
//             {
//                 string requestedDomain = receivedMessage.Content?.ToString() ?? "";
//                 DNSRecord? foundRecord = ReadDNSRecords().FirstOrDefault(r => r.Name == requestedDomain);

//                 if (foundRecord != null)
//                 {
//                     // Send DNSLookupReply
//                     Message responseMessage = new Message
//                     {
//                         MsgId = receivedMessage.MsgId,
//                         MsgType = MessageType.DNSLookupReply,
//                         Content = foundRecord
//                     };

//                     SendResponse(serverSocket, remoteEP, responseMessage);
//                 }
//                 else
//                 {
//                     // Send Error: Domain not found
//                     SendErrorMessage(serverSocket, remoteEP, "Domain not found");
//                 }
//             }

// }



        // TODO:[Receive and print DNSLookup]


        // TODO:[Query the DNSRecord in Json file]

        // TODO:[If found Send DNSLookupReply containing the DNSRecord]
                // TODO:[Receive and print Hello]
        
                        // byte[] buffer = new byte[1000];
                        // string data = null;

                        // EndPoint remoteEP = (EndPoint)serverEndPoint;
                        // int b = serverSocket.ReceiveFrom(buffer, ref remoteEP);
                        // data = Encoding.ASCII.GetString(buffer, 0, b);
                        // Console.WriteLine("A message received from "+ remoteEP.ToString() + " " + data);

                // TODO:[Send Welcome to the client]


        // TODO:[If not found Send Error]


        // TODO:[Receive Ack about correct DNSLookupReply from the client]


        // TODO:[If no further requests receieved send End to the client]

    

        // int port = setting.ServerPortNumber;
        // string serverIP = setting.ServerIPAddress;


        // byte[] buffer = new byte[1000];
        // byte[] msg = Encoding.ASCII.GetBytes("From server: Your message delivered\n");

        // string data = null;
        // int MsgCounter = 0;
        // IPAddress ipAddress = IPAddress.Parse(serverIP);
        // IPEndPoint localEndpoint = new IPEndPoint(ipAddress, port);
        // IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        // EndPoint remoteEP = (EndPoint)sender;


        // try
        // {
        //     Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //     sock.Bind(localEndpoint);
        //     while (MsgCounter < 10)
        //     {
        //         Console.WriteLine("\n Waiting for the next client message..");
        //         int b = sock.ReceiveFrom(buffer, ref remoteEP);
        //         data = Encoding.ASCII.GetString(buffer, 0, b);
        //         Console.WriteLine("A message received from "+remoteEP.ToString()+ " " + data);
        //         sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);
        //         MsgCounter++;
        //     }
        //     sock.Close();
        // }
        // catch
        // {
        //     Console.WriteLine("\n Socket Error. Terminating");
        // }



            // byte[] buffer = new byte[1000];
            // byte[] msg = Encoding.ASCII.GetBytes("From server: Your message delivered\n");
            // string data = null;

            // IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            // IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 32000);

            // Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // sock.Bind(localEndpoint);
            // sock.Listen(5);

            // Console.WriteLine("\nWaiting for clients...");
            // Socket newSock = sock.Accept();

            // while (true)
            // {
            //     int b = newSock.Receive(buffer);
            //     data = Encoding.ASCII.GetString(buffer, 0, b);


