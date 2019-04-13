using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulator.Model.Interface
{
    public interface IFlightSimulatorModel
    {
        double Aileron { get; set; }
        double Throttle { get; set; }
        double Elevator { get; set; }
        double Rudder { get; set; }
        void ConnectInfoServer(string FlightServerIP, int FlightInfoPort);
        void DisconnectInfoServer();
        void StartInfoServer();
        void ConnectCommandsClient(int FlightCommandPort);
        void DisconnectCommandsClient();
    }
}
