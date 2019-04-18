using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FlightSimulator.Model.Interface
{
    public interface IFlightSimulatorModel : INotifyPropertyChanged
    {
        double Aileron { get; set; }
        double Throttle { get; set; }
        double Elevator { get; set; }
        double Rudder { get; set; }
        double Lon { get; set; }
        double Lat { get; set; }
        void ConnectInfoServer(string FlightServerIP, int FlightInfoPort);
        void DisconnectInfoServer();
        void StartInfoServer();
        void ConnectCommandsClient(string FlightServerIP, int FlightCommandPort);
        void DisconnectCommandsClient();
        IFlightSimulatorModel SetTelnetClient(ITelnetClient telnetClient);
    }
}
