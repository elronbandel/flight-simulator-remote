using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightSimulator.Model.Interface;
using System.ComponentModel;
using System.Threading;
using FlightSimulator.ViewModels;

namespace FlightSimulator.Model
{
    //A class that will function as a model.
    public class FlightSimulatorModel : BaseNotify, IFlightSimulatorModel
    {

        volatile Boolean stop;
        private static Mutex mut;
        private NetworkStream stream;
        private double changeTreshold;
        volatile Boolean connectionClient, connectionServer;
        private ITelnetClient tc;
        private TcpListener listener;
        private TcpClient client;
        volatile Boolean isTelnetSet = false;
        private int demoNotificationsCounter;
        #region Singleton
        private static IFlightSimulatorModel m_Instance = null;
        public static IFlightSimulatorModel Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new FlightSimulatorModel();
                }
                return m_Instance;
            }

        }
        #endregion
        //constructor
        private FlightSimulatorModel()
        {
            stop = false;
            mut = new Mutex();
            connectionClient = false;
            connectionServer = false;
            changeTreshold = 0.001;
            demoNotificationsCounter = 0;
        }
        //a method to execute the string commands.
        public void Execute(string command)
        {
            if (connectionClient)
                tc.Write(command);
        }

        #region Properties
        #region ControlProperties

        public double Aileron { set; get; }
        public double Throttle { set; get; }
        public double Elevator { set; get; }
        public double Rudder { set; get; }
        #endregion
        #region TrackingProperties

        private double lon;
        public double Lon
        {
            set
            {
                //change lon only if the change is significant.
                if (Math.Abs(lon - value) > changeTreshold)
                {
                    lon = value;
                    //notify property changed only when the data is legit.
                    if (demoNotificationsCounter > 2)
                    {
                        NotifyPropertyChanged("Lon");
                    }
                    else
                    {
                        demoNotificationsCounter++;
                    }

                }

            }
            get
            {
                return lon;
            }
        }
        private double lat;
        public double Lat
        {
            set
            {
                //change lat only if the change is significant.
                if (Math.Abs(lat - value) > changeTreshold)
                {
                    lat = value;
                    //notify property changed only when the data is legit.
                    if (demoNotificationsCounter > 2)
                    {
                        NotifyPropertyChanged("Lat");
                    }
                    else
                    {
                        demoNotificationsCounter++;
                    }
                }
            }
            get
            {
                return lat;
            }
        }
        #endregion
        #endregion
        #region TCP
        //A setter for telnet client.
        public IFlightSimulatorModel SetTelnetClient(ITelnetClient telnet)
        {
            //set it only once because this is a singleton.
            if (!isTelnetSet)
            {
                tc = telnet;
            }
            return this;
        }
        //A function to connect as a client to the simulator.
        public void ConnectCommandsClient(string FlightServerIP, int FlightCommandPort)
        {
            //disconnect from former connections.
            DisconnectCommandsClient();
            //connect if disconnected successfully
            if (!connectionClient)
            {
                tc.Connect(FlightServerIP, FlightCommandPort);
                connectionClient = true;
            }
        }
        //A function to create a server and wait for a connection.
        public void ConnectInfoServer(string FlightServerIP, int FlightInfoPort)
        {
            //disconnect from former connections.
            DisconnectInfoServer();
            //connect if disconnected successfully.
            if (!connectionServer)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(FlightServerIP), FlightInfoPort);
                listener = new TcpListener(ep);
                listener.Start();
                client = listener.AcceptTcpClient();
                connectionServer = true;
            }

        }
        //A function to disconnect as a client.
        public void DisconnectCommandsClient()
        {
            if (connectionClient)
            {
                tc.Disconnect();
                connectionClient = false;
            }

        }
        //A function to disconnect as a server.
        public void DisconnectInfoServer()
        {
            if (connectionServer)
            {
                mut.WaitOne();
                stream.Close();
                client.Close();
                listener.Stop();
                connectionServer = false;
                mut.ReleaseMutex();
            }
        }
        /*while the server is connected,
         * get values from simulator in another thread. 
         */
        public void StartInfoServer()
        {
            new Thread(() =>
            {
                //default buffer size of 1 kilo byte.
                client.ReceiveBufferSize = 1024;
                stream = client.GetStream();
                byte[] bytes = new byte[client.ReceiveBufferSize];
                int byteNum = 0;
                var remainder = "";
                //while loop to get values from the simulator.
                while (!stop)
                {
                    //try reading only when there is a connected client.
                    mut.WaitOne();
                    if (connectionServer)
                    {
                        //read to the buffer.
                        byteNum = stream.Read(bytes, 0, client.ReceiveBufferSize);
                    }
                    else
                    {
                        break;
                    }
                    mut.ReleaseMutex();

                    //encode the message into a string.
                    var msg = Encoding.ASCII.GetString(bytes, 0, byteNum);
                    //split the string to an array of string by line endings.
                    String[] data_sets = msg.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    //line is the remainder of former line and the first new line.
                    if (data_sets.Length > 0)
                        data_sets[0] = remainder + data_sets[0];
                    //parse the lon and the lat from the simulator message.
                    while (data_sets.Length > 1)
                    {
                        ParseLonLat(data_sets.First());
                        data_sets = data_sets.Skip(1).ToArray();
                    }
                    //the last line is the remainder of the text.
                    remainder = data_sets.First();
                }
            }).Start();
        }
        //Stop the server from running.
        public void StopInfoServer()
        {
            stop = true;
        }
        //A function to parse lon and lat out of the simulator data.
        private void ParseLonLat(string msg)
        {
            int index = msg.IndexOf(',');
            string sub = msg.Substring(0, index);
            Lon = Double.Parse(sub);
            sub = msg.Substring(index + 1);
            index = sub.IndexOf(',');
            sub = sub.Substring(0, index);
            Lat = Double.Parse(sub);
        }
        #endregion
        //a function that executes a command when a property was set.
        public void NotifyPropertySet(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Throttle")
            {
                Execute($"set /controls/engines/current-engine/throttle {Throttle}");
            }
            if (e.PropertyName == "Rudder")
            {
                Execute($"set /controls/flight/rudder {Rudder}");
            }
            if (e.PropertyName == "Aileron")
            {
                Execute($"set /controls/flight/aileron {Aileron}");
            }
            if (e.PropertyName == "Elevator")
            {
                Execute($"set /controls/flight/elevator {Elevator}");
            }
        }
    }
}
