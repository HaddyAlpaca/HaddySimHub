using System.Windows;

namespace HaddySimHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Exit application
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Do not close but minimize
            var frm = (MainWindow)sender;
            frm.WindowState = WindowState.Minimized;
            frm.ShowInTaskbar = false;
            e.Cancel = true;
        }
    }
}
