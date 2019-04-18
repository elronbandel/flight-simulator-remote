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
        private Boolean connectionClient, connectionServer;
        private ITelnetClient tc;
        private TcpListener listener;
        private TcpClient client;
        private Boolean isTelnetSet = false;
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
            aileron = 0;
            rudder = 0;
            elevator = 0;
            throttle = 0;
        }
        public void Execute(string command)
        {
            tc.Write(command);
        }

        #region Properties
        private double aileron;
        public double Aileron
        {
            get { return aileron; }
            set
            {
                aileron = value;
            }
        }
        //do here as we did with Aileron property.
        private double throttle;
        public double Throttle
        {
            get { return throttle; }
            set
            {
                throttle = value;
            }
        }
        //do here as we did with Aileron property.
        private double elevator;
        public double Elevator
        {
            get { return elevator; }
            set
            {
                elevator = value;
            }
        }
        //do here as we did with Aileron property.
        private double rudder;
        public double Rudder
        {
            get
            {
                return rudder;
            }
            set
            {
                rudder = value;
            }
        }
        private double lon;
        public double Lon
        {
            set
            {
                lon = value;
                NotifyPropertyChanged("Lon");
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
                lat = value;
                NotifyPropertyChanged("Lat");
            }
            get
            {
                return lat;
            }
        }
        String CommandsText
        {
            set
            {
                string val = value;
                int index = 0;
                index = val.IndexOf('\n');
                while (index != -1)
                {
                    string sub = val.Substring(0, index);
                    val.Remove(0, index + 1);
                    tc.Write(sub);
                    index = val.IndexOf('\n');
                }
                tc.Write(val);
            }
        }
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
                int EndOfLine = 0;
                string result = "";
                string remainder = "";
                bool IsEndOfLine;

                while (!stop)
                {
                    byteNum = stream.Read(bytes, 0, client.ReceiveBufferSize);
                    String msg = Encoding.ASCII.GetString(bytes, 0, byteNum);
                    IsEndOfLine = true;
                    result = remainder;
                    while (IsEndOfLine)
                    {
                        EndOfLine = msg.IndexOf('\n');
                        if (EndOfLine != -1)
                        {
                            result += msg.Substring(0, EndOfLine);
                            msg.Remove(EndOfLine + 1);
                            ParseLonLat(result);
                            result = "";
                            remainder = msg;
                        }
                        else
                        {
                            remainder += msg;
                            IsEndOfLine = false;
                        }
                    }
                }
            }).Start();
            //close connections.
            if (connectionClient)
            {
                DisconnectCommandsClient();
            }
            if (connectionServer)
            {
                DisconnectInfoServer();
            }
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
