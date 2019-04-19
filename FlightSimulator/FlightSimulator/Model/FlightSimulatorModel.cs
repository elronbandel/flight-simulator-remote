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
    public class FlightSimulatorModel : BaseNotify, IFlightSimulatorModel
    {

        volatile Boolean stop;
        private double changeTreshold;
        private Boolean connectionClient, connectionServer;
        private ITelnetClient tc;
        private TcpListener listener;
        private TcpClient client;
        private Boolean isTelnetSet = false;
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
            connectionClient = false;
            connectionServer = false;
            changeTreshold = 0.001;
            demoNotificationsCounter = 0;
        }
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

                if (Math.Abs(lon - value) > changeTreshold)
                {
                    lon = value;
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
                //check if current value is different from former value.
                return lon;
            }
        }
        private double lat;
        public double Lat
        {
            set
            {
                if (Math.Abs(lat - value) > changeTreshold)
                {                    
                    lat = value;
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
        public IFlightSimulatorModel SetTelnetClient(ITelnetClient telnet)
        {
            if (!isTelnetSet)
            {
                tc = telnet;
            }
            return this;
        }

        public void ConnectCommandsClient(string FlightServerIP, int FlightCommandPort)
        {
            if (connectionClient)
            {
                DisconnectCommandsClient();
            }
            tc.Connect(FlightServerIP, FlightCommandPort);
            connectionClient = true;

        }

        public void ConnectInfoServer(string FlightServerIP, int FlightInfoPort)
        {
            if (connectionServer)
            {
                DisconnectInfoServer();
            }
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(FlightServerIP), FlightInfoPort);
            listener = new TcpListener(ep);
            listener.Start();
            client = listener.AcceptTcpClient();
            connectionServer = true;
        }

        public void DisconnectCommandsClient()
        {
            tc.Disconnect();
            connectionClient = false;
        }

        public void DisconnectInfoServer()
        {
            client.Close();
            listener.Stop();
            connectionServer = false;
        }

        public void StartInfoServer()
        {
            new Thread(() =>
            {
                client.ReceiveBufferSize = 1024;
                NetworkStream stream = client.GetStream();
                byte[] bytes = new byte[client.ReceiveBufferSize];
                int byteNum;
                var remainder = "";
                while (!stop)
                {
                    byteNum = stream.Read(bytes, 0, client.ReceiveBufferSize);
                    var msg = Encoding.ASCII.GetString(bytes, 0, byteNum);
                    String[] data_sets = msg.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    if (data_sets.Length > 0)
                        data_sets[0] = remainder + data_sets[0];
                    while (data_sets.Length > 1)
                    {
                        ParseLonLat(data_sets.First());
                        data_sets = data_sets.Skip(1).ToArray();
                    }
                    remainder = data_sets.First();
                }

                if (connectionClient)
                {
                    DisconnectCommandsClient();
                }
                if (connectionServer)
                {
                    DisconnectInfoServer();
                }
            }).Start();
            //close connections.

        }

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
