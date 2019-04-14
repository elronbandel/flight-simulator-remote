using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightSimulator.Model.Interface;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FlightSimulator.Model
{
    public class TelnetClient : ITelnetClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        public void Connect(string ip, int port)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new TcpClient();
            client.Connect(ep);
            stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }
        public void Disconnect()
        {
            client.Close();
            //need to close the stream\reader\writer?
        }

        public string Read()
        {
            string value = reader.ReadString();
            return value;
        }

        public void Write(string command)
        {
            writer.Write(command + "\r\n");
        }
    }
}
