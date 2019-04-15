using FlightSimulator.Model.Interface;
using FlightSimulator.Model;
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
    public class FlightBoardViewModel : BaseNotify
    {
        private IFlightSimulatorModel m_model;
        public FlightBoardViewModel(IFlightSimulatorModel model)
        {
            this.m_model = model;
        }
        public string FlightServerIP
        {
            get { return Properties.Settings.Default.FlightServerIP; }
        }
        public int FlightCommandPort
        {
            get { return Properties.Settings.Default.FlightCommandPort; }
        }

        public int FlightInfoPort
        {
            get { return Properties.Settings.Default.FlightInfoPort; }
        }
        public double Lon
        {
            get
            {
                return m_model.Lon;
            }
        }

        public double Lat
        {
            get
            {
                return m_model.Lat;
            }
        }

        #region ConnectCommand
        private ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get { return _connectCommand ?? (_connectCommand = new CommandHandler(() => OnConnect())); }
        }
        void OnConnect()
        {
            m_model.ConnectInfoServer(FlightServerIP, FlightInfoPort);
            m_model.ConnectCommandsClient(FlightServerIP,FlightCommandPort);
            m_model.StartInfoServer();
        }
        #endregion
        #region SettingCommand
        private ICommand _settingCommand;
        public ICommand SettingCommand
        {
            get { return _settingCommand ?? (_settingCommand = new CommandHandler(() => OnSetting())); }
        }
        void OnSetting()
        {
           var swvm = new SettingsWindowViewModel(ApplicationSettingsModel.Instance);
           var sw = new SettingsWindow() {DataContext = swvm};
           swvm.OnRequestClose += (s, e) => sw.Close();
           sw.Show();
        }
        #endregion
    }
}
