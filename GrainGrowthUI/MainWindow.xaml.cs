using System.Windows;

namespace GrainGrowthUI
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int counter = 0;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Start_Click(object sender, RoutedEventArgs e)
        {

            
            if (FileNameTextBox.Text == "" || NumberOfNucleonsTextBox.Text == "" ||
                SizeXTextBox.Text == "" || SizeZTextBox.Text == "" || SizeYTextBox.Text == "")
                return;

            if (MonteCarloRadioButton.IsChecked == true && MonteCarloTextBox.Text == "" 
                && KTTextBox.Text == "" && JTextBox.Text == "")
                return;

            string fileName = FileNameTextBox.Text;
            string sizeX = SizeXTextBox.Text;
            string sizeY = SizeYTextBox.Text;
            string sizeZ = SizeZTextBox.Text;
            string numberOfNucleons = NumberOfNucleonsTextBox.Text;
            string neighbourhood = VonNeumannRadioButton.IsChecked == true ? "VonNeumann" : "Moore";
            string bc = PeriodicRadioButton.IsChecked == true ? "Periodic" : "Nonperiodic";
            string simulation = CARadioButton.IsChecked == true ? "CA" : "MonteCarlo";
            string numberOfIterations = CARadioButton.IsChecked == true ? "0" : MonteCarloTextBox.Text;
            string kt = CARadioButton.IsChecked == true ? "0" : KTTextBox.Text;
            string j = CARadioButton.IsChecked == true ? "0" : JTextBox.Text;

            counter++;

            Simulation mySimulation = new Simulation(counter.ToString(), fileName, sizeX, sizeY, sizeZ, neighbourhood,
                                                     bc, numberOfNucleons, simulation, numberOfIterations, kt, j, SimulationPanel, SimulationListView);
            mySimulation.Run(@"C:\Users\Marcin\source\repos\ConsoleApp1\ConsoleApp1\bin\Release\netcoreapp2.1\win7-x64\ConsoleApp1.exe", 
                            MyTabControl, SimulationListView);
          
        }
    }
}
