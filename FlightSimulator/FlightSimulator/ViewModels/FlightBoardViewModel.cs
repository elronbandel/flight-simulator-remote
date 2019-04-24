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
using System.ComponentModel;
using System.Threading;

namespace FlightSimulator.ViewModels
{
    //a class which represents the flightboard viewmodel.
    public class FlightBoardViewModel : BaseNotify
    {
        private IFlightSimulatorModel m_model;
        public FlightBoardViewModel(IFlightSimulatorModel model)
        {
            this.m_model = model;
            //notify when the model is notifying a change.
            m_model.PropertyChanged += delegate (object s, PropertyChangedEventArgs e)
            { NotifyPropertyChanged(e.PropertyName); };
        }
        #region Properties
        //ports and ip getters so we can send them as parameters to connect function.
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
        //Lon getter.
        public double Lon
        {
            get
            {
                return m_model.Lon;

            }
        }
        //Lat getter.
        public double Lat
        {
            get
            {
                return m_model.Lat;

            }
        }
        #endregion
        #region commands
        #region ConnectCommand
        //a command that make all of the connections and start the server.
        private ICommand _connectCommand;
        //getter that return the command after creating a new commandhandler with the function.
        public ICommand ConnectCommand
        {
            get { return _connectCommand ?? (_connectCommand = new CommandHandler(() => OnConnect())); }
        }
        //the function that invokes when we call the command.
        void OnConnect()
        {
            //create connections and start the server in a new thread.
            new Thread(() =>
            {
                m_model.ConnectInfoServer(FlightServerIP, FlightInfoPort);
                m_model.ConnectCommandsClient(FlightServerIP, FlightCommandPort);
                m_model.StartInfoServer();
            }).Start();
        }
        #endregion
        #region SettingCommand
        // a command that opens the settings window.
        private ICommand _settingCommand;
        public ICommand SettingCommand
        {
            get { return _settingCommand ?? (_settingCommand = new CommandHandler(() => OnSetting())); }
        }
        //function to open the settings window and making its viewModel datacontext.
        void OnSetting()
        {
            var swvm = new SettingsWindowViewModel(ApplicationSettingsModel.Instance);
            var sw = new SettingsWindow() { DataContext = swvm };
            swvm.OnRequestClose += (s, e) => sw.Close();
            sw.Show();
        }
        #endregion
        #region DisconnectCommand
        //a command that stops the server and close all connections to end the program.
        private ICommand _disconnectCommand;
        public ICommand DisconnectCommand
        {
            get { return _disconnectCommand ?? (_disconnectCommand = new CommandHandler(() => OnDisconnect())); }
        }
        //stop the server and close connections.
        void OnDisconnect()
        {
            m_model.StopInfoServer();
            m_model.DisconnectCommandsClient();
            m_model.DisconnectInfoServer();
        }
        #endregion
        #endregion
    }
}
