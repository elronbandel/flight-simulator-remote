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
using System.Threading;
using FlightSimulator.Model;

namespace FlightSimulator.ViewModels
{
    public class AutopilotControlViewModel : BaseNotify
    {
        private IFlightSimulatorModel model;
        public AutopilotControlViewModel(IFlightSimulatorModel model)
        {
            this.model = model;
        }
        #region Properties
        private String commandsText;
        public String CommandsText
        {
            get { return commandsText; }
            set
            {
                commandsText = value;
                NotifyPropertyChanged("CommandsText");
            }
        }

        private bool executing;
        public bool Executing
        {
            get { return executing; }
            set
            {
                executing = value;
                NotifyPropertyChanged("Executing");
                NotifyPropertyChanged("ExecutingFinshed");
            }
        }
        public bool ExecutingFinshed
        {
            get { return !executing; }
            set
            {
                executing = !value;
                NotifyPropertyChanged("Executing");
                NotifyPropertyChanged("ExecutingFinshed");
            }
        }
        public void ExecuteCommands()
        {
            //split the commands to seprate commands:
            string[] commands = commandsText.Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.None);
            Executing = true;
            foreach (string command in commands)
            {
                model.Execute(command);
                Thread.Sleep(2000);
            }
            
            Executing = false;
        }
        #endregion
        #region Commands
        #region ClearCommand
        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new CommandHandler(() => OnClear()));
            }
        }
        private void OnClear()
        {
            CommandsText = "";
        }
        #endregion

        #region ExecutionCommand
        private ICommand _executionCommand;
        public ICommand ExecutionCommand
        {
            get
            {
                return _executionCommand ?? (_executionCommand = new CommandHandler(() => OnExecution()));
            }
        }
        private void OnExecution()
        {
            new Thread(ExecuteCommands).Start();
        }
        #endregion
        #endregion

    }
}
