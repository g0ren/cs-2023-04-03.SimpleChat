﻿using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Globalization;

namespace ChatClient
{
    internal class Program
    {

        static List<string> ReceiveBroadcastResponses(int port, int timeout, string message)
        {
            using (var udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;
                udpClient.Client.ReceiveTimeout = timeout;

                byte[] datagram = System.Text.Encoding.UTF8.GetBytes(message);
                udpClient.Send(datagram, datagram.Length, new IPEndPoint(IPAddress.Broadcast, port));

                var responses = new List<string>();
                while (true)
                {
                    try
                    {
                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                        byte[] response = udpClient.Receive(ref remoteEndPoint);

                        string responseString = Encoding.ASCII.GetString(response);
                        responses.Add(responseString);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.TimedOut)
                        {
                            break;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
                return responses;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Enter your nick");
            string nick = Console.ReadLine();

            List<string> availableServers = new List<string>();
            availableServers = ReceiveBroadcastResponses(12345, 1000, "Give me your Endpoint!");
            if (!availableServers.Any())
            {
                Console.WriteLine("No servers found!");
                return;
            }
            Console.WriteLine("Select server from the list");
            for(int i = 0; i < availableServers.Count; i++)
            {
                Console.WriteLine($"{i}. {availableServers[i]}");
            }
            int selection = Convert.ToInt32(Console.ReadLine());
            if (selection >= availableServers.Count)
            {
                Console.WriteLine($"No server #{selection}!");
                return;
            }
            string[] endpoint = availableServers[selection].Split(":");
            IPAddress address = IPAddress.Parse(endpoint[0]);

            using (ChatClient chatClient = new ChatClient(
                address,
                Convert.ToInt32(endpoint[1])))
            {
                chatClient.Connect(nick);
            }
            Console.ReadLine();
        }
    }
}

/*
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public List<string> ReceiveBroadcastResponses(int port, int timeout, string message)
{
    // Create a UDP socket to send and receive datagrams
    using (var socket = new UdpClient())
    {
        socket.EnableBroadcast = true;
        socket.Client.ReceiveTimeout = timeout;
        
        // Send the broadcast message to the local network
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        socket.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));
        
        // Receive responses from all servers that reply within the timeout period
        var responses = new List<string>();
        while (true)
        {
            try
            {
                // Receive a datagram from any IP address on the specified port
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] response = socket.Receive(ref remoteEndPoint);
                
                // Convert the response data to a string and add it to the list
                string responseString = System.Text.Encoding.UTF8.GetString(response);
                responses.Add(responseString);
            }
            catch (SocketException ex)
            {
                // If the timeout expires, exit the loop and return the list of responses
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    break;
                }
                else
                {
                    throw ex;
                }
            }
        }
        
        return responses;
    }
}


 */