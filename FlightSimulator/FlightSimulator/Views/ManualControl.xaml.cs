using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlightSimulator.Model;
using FlightSimulator.ViewModels;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using FlightSimulator.ViewModels;
using FlightSimulator.Model;

namespace FlightSimulator.Views
{
    public partial class ManualControl : System.Windows.Controls.UserControl
    {
        public ManualControl()
        {
            this.InitializeComponent();
            var vm = new ManualControlViewModel(FlightSimulatorModel.Instance);
            DataContext = vm;
            Joystick.Moved += vm.update;
        }
    }
}

