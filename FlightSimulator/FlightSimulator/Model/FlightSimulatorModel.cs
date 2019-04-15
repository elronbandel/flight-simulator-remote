using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightSimulator.Model.Interface;
using System.ComponentModel;

namespace FlightSimulator.Model
{
    public class FlightSimulatorModel : IFlightSimulatorModel 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        volatile Boolean stop;
        private Boolean connectionClient, connectionServer;
        private ITelnetClient tc;
        private TcpListener listener;
        private TcpClient client;
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
        }

        private FlightSimulatorModel setTelnetClient(ITelnetClient telnet)
        {
            tc = telnet;
            return this;
        }
        #region Properties
        private double aileron;
        public double Aileron {
            get
            {
                tc.Write("get aileron"); //check how to write commands to the simulator.
                return double.Parse(tc.Read());
            }
            set
            {
                aileron = value;
                tc.Write($"set aileron {value}"); //check how to write commands to the simulator.
            }
        }
        //do here as we did with Aileron property.
        private double throttle;
        public double Throttle {
            get => throw new NotImplementedException(); set => throw new NotImplementedException();
        }
        //do here as we did with Aileron property.
        private double elevator;
        public double Elevator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //do here as we did with Aileron property.
        private double rudder;
        public double Rudder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private double lon;
        public double Lon
        {
            get
            {
                tc.Write("get lon"); //check how to write commands to the simulator.
                return double.Parse(tc.Read());
            }
        }
        private double lat;
        public double Lat
        {
            get
            {
                tc.Write("get lat"); //check how to write commands to the simulator.
                return double.Parse(tc.Read());
            }
        }
        #endregion
        #region TCP
        public void ConnectCommandsClient(string FlightServerIP, int FlightCommandPort)
        {
            if (!connectionClient)
            {
                tc.Connect(FlightServerIP, FlightCommandPort);
                connectionClient = true;
            }
            
        }

        public void ConnectInfoServer(string FlightServerIP, int FlightInfoPort)
        {
            if (!connectionServer)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(FlightServerIP), FlightInfoPort);
                listener = new TcpListener(ep);
                listener.Start();
                client = listener.AcceptTcpClient(); // this is how we should accept the client or just use the TelnetClient?
                connectionServer = true;
            }
            
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
            //close former connections before creating a new one.
            if (connectionClient)
            {
                DisconnectCommandsClient();
            }
            if (connectionServer)
            {
                DisconnectInfoServer();
            }
            
        }
        #endregion
    }
}
