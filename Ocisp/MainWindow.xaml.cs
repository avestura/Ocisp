using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using OcispCore;
using Microsoft.Win32;
using System.Diagnostics;

namespace Ocisp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public OcispProblem Problem { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Date Set File|*.clq"
            };

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var meta = OcispProblem.Parse(dialog.FileName);

                StatusText.Text = "";
                StatusText.Text += $"Nodes: {meta.Nodes}\n";
                StatusText.Text += $"Edges: {meta.Edges}\n";

                var problem = new OcispProblem()
                {
                    ProblemGraph = meta.Graph
                };

                Problem = problem;

                ProcessGA();

                ShowResults();
            }
        }

        public void ShowResults()
        {
            Task.Run(async () =>
            {
                while (true)
                {

                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            var current = Problem.CurrentGeneration.Max(x => x.Evaluate());
                            Current.Text = $"Current Evaluate: {current}";

                            if (Expander.IsExpanded)
                            {
                                GenerationStat.Text = "";
                                foreach (var item in Problem.CurrentGenerationDebugger)
                                {
                                    GenerationStat.Text += $"[{string.Join(", ", item.NumbersEncoding().Select(x => x + 1))}]\n";
                                }
                            }

                            Best.Text = $"Best Evaluate: {Problem.BestEverChromosome.Evaluate()}";
                            SelectedV.Text = $"Selected: [{string.Join(", ", Problem.BestEverChromosome.NumbersEncoding().Select(x => x+1))}]";
                        }
                        catch (Exception ex) { Console.WriteLine($"Error + {ex.Message}"); }

                    });
                    await Task.Delay(500);
                }

            });
        }

        public void ProcessGA()
        {
            Task.Run(() =>
            {
                Problem.GeneticAlgorithm();
            });
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
