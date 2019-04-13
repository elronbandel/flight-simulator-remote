using FlightSimulator.Model.Interface;
using FlightSimulator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlightSimulator.ViewModels
{
    public class FlightBoardViewModel : BaseNotify
    {
        private IFlightSimulatorModel m_model;
        public FlightBoardViewModel(IFlightSimulatorModel model)
        {
            this.m_model = model;
        }
        public double Lon
        {
            get;
        }

        public double Lat
        {
            get;
        }

        #region SettingsCommand
        
        #endregion
    }
}
