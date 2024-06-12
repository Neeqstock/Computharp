using Computharp.Modules;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Computharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer DispatcherTmr;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rack.DMIBox = new ComputharpDmiBox(this);

            DispatcherTmr = new DispatcherTimer();
            DispatcherTmr.Interval = new TimeSpan(150);
            DispatcherTmr.Tick += Dispatcher_Tick;
            DispatcherTmr.Start();

            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();
        }

        private void Dispatcher_Tick(object? sender, EventArgs e)
        {
            prb_Pressure.Value = Rack.DMIBox.Pressure;
            lbl_Test.Content = Rack.DMIBox.Pressure;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}