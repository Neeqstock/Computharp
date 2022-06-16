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
        private DispatcherTimer Dispatcher;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rack.DMIBox = new ComputharpDmiBox(this);

            Dispatcher = new DispatcherTimer();
            Dispatcher.Interval = new TimeSpan(150);
            Dispatcher.Tick += Dispatcher_Tick;
            Dispatcher.Start();
        }

        private void Dispatcher_Tick(object? sender, EventArgs e)
        {
            prb_Pressure.Value = Rack.DMIBox.Pressure;
            lbl_Test.Content = Rack.DMIBox.Pressure;
        }
    }
}