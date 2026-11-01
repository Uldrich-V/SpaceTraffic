using Avalonia.Controls;
using System;

namespace SpaceTraffic
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this("default")
        {
        }
        /// <summary>
        /// Initializes new MainWindow without any specified scenario
        /// </summary>
        public MainWindow(string scenarioName)
        {
            InitializeComponent();
            if (this.Content is MainView mainView)
            {
                mainView.NactiScenar(scenarioName);
            }
        }

    }
}