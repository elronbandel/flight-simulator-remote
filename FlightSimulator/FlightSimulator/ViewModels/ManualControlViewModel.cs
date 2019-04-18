using FlightSimulator.Model.Interface;
using FlightSimulator.Model.EventArgs;
using FlightSimulator.ViewModels.Windows;
using FlightSimulator.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlightSimulator.ViewModels
{
    public class ManualControlViewModel : BaseNotify
    {
        private IFlightSimulatorModel model;
        public ManualControlViewModel(IFlightSimulatorModel model)
        {
            this.model = model;
            PropertyChanged += model.NotifyPropertySet;
        }
        public double Throttle
        {
            get { return model.Throttle; }
            set {
                model.Throttle = value;
                NotifyPropertyChanged("Throttle");
            }
        }
        public double Rudder
        {
            get { return model.Rudder; }
            set
            {
                model.Rudder = value;
                NotifyPropertyChanged("Rudder");
            }
        }
        public double Aileron
        {
            get { return model.Aileron; }
            set
            {
                model.Aileron = value;
                NotifyPropertyChanged("Aileron");
            }
        }
        public double Elevator
        {
            get { return model.Elevator; }
            set
            {
                model.Elevator = value;
                NotifyPropertyChanged("Elevator");
            }
        }

        public void Update(object sender, VirtualJoystickEventArgs args)
        {
            Aileron = args.Aileron;
            Elevator = args.Elevator;
        }
    }
}
