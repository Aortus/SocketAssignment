﻿using System;
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
    public static List<DNSRecord> ReadDNSRecords()
    {
        string jsonContent = File.ReadAllText("DNSrecords.json");
        return JsonSerializer.Deserialize<List<DNSRecord>>(jsonContent) ?? new List<DNSRecord>();
    }



    public static void start()
    {
        // TODO: [Create a socket and endpoints and bind it to the server IP address and port number]
        // Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        // IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 8080);
        // serverSocket.Bind(serverEndPoint);

        // byte[] buffer = new byte[1000];

        // TODO:[Receive and print a received Message from the client]
        // EndPoint remoteEP = (EndPoint)sender;
        // int b = sock.ReceiveFrom(buffer, ref remoteEP);
        // data = Encoding.ASCII.GetString(buffer, 0, b);
        // Console.WriteLine("A message received from "+ remoteEP.ToString() + " " + data);

        // TODO:[Receive and print Hello]
        
        int port = setting.ServerPortNumber;
        string serverIP = setting.ServerIPAddress;


        byte[] buffer = new byte[1000];
        byte[] msg = Encoding.ASCII.GetBytes("From server: Your message delivered\n");

        string data = null;
        int MsgCounter = 0;
        IPAddress ipAddress = IPAddress.Parse(serverIP);
        IPEndPoint localEndpoint = new IPEndPoint(ipAddress, port);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteEP = (EndPoint)sender;


        try
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(localEndpoint);
            while (MsgCounter < 10)
            {
                Console.WriteLine("\n Waiting for the next client message..");
                int b = sock.ReceiveFrom(buffer, ref remoteEP);
                data = Encoding.ASCII.GetString(buffer, 0, b);
                Console.WriteLine("A message received from "+remoteEP.ToString()+ " " + data);
                sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);
                MsgCounter++;
            }
            sock.Close();
        }
        catch
        {
            Console.WriteLine("\n Socket Error. Terminating");
        }


        // TODO:[Send Welcome to the client]


        // TODO:[Receive and print DNSLookup]


        // TODO:[Query the DNSRecord in Json file]

        // TODO:[If found Send DNSLookupReply containing the DNSRecord]



        // TODO:[If not found Send Error]


        // TODO:[Receive Ack about correct DNSLookupReply from the client]


        // TODO:[If no further requests receieved send End to the client]

    }

//  byte[] buffer = new byte[1000];
//  byte[] msg = Encoding.ASCII.GetBytes("From server:
//  Your message delivered\n");
//  string data = null;
//  Socket sock;
//  int MsgCounter = 0;
//  IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
//  IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 32000);
//  IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
//  EndPoint remoteEP = (EndPoint)sender;

//  try
//  {
//  sock = new Socket(AddressFamily.InterNetwork,
//  SocketType.Dgram, ProtocolType.Udp);
//  sock.Bind(localEndpoint);
//  while (MsgCounter < 10)
//  {
//  Console.WriteLine("\n Waiting for the next
//  client message..");
//  int b = sock.ReceiveFrom(buffer, ref remoteEP);
//  data = Encoding.ASCII.GetString(buffer, 0, b);
//  Console.WriteLine("A message received
//  from "+remoteEP.ToString()+ " " + data);
//  sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);
//  MsgCounter++;
//  }
//  sock.Close();
//  }
//  catch
//  {
//  Console.WriteLine("\n Socket Error. Terminating");
//  }
//  }


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


}