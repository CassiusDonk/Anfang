using System;
using System.Collections.Generic;
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
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Data;
using System.Collections.ObjectModel;
using System.Timers;
using System.Globalization;
using System.Windows.Threading;

namespace Anfang
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        GraphOps GraphOps = new GraphOps();
        List<Branch> branches = new List<Branch>();
        private static readonly ObservableCollection<Branch> datagrid_collection = new ObservableCollection<Branch>();
        private static readonly ObservableCollection<Branch> shockpointgrid_collection = new ObservableCollection<Branch>();
        private List<Branch> drawn_items = new List<Branch>();
        System.Timers.Timer Timer;
        Transient Transient = new Transient(100);
        Vector<Complex32> currents_start = Vector<Complex32>.Build.Random(1);
        Vector<Complex32> currents_shock = Vector<Complex32>.Build.Random(1);
        public MainWindow()
        {
            InitializeComponent();
            inputgrid.ItemsSource = datagrid_collection;
            shockpointgrid.ItemsSource = shockpointgrid_collection;
            datagrid_collection.Add(new Branch() { Number = 1, Node1 = 0, Node2 = 1, Ohms_Act = 1, Ohms_React = 0, E_Act = 1, E_React = 0 });
            datagrid_collection.Add(new Branch() { Number = 2, Node1 = 1, Node2 = 2, Ohms_Act = 1, Ohms_React = 0, E_Act = 0, E_React = 0 });
            datagrid_collection.Add(new Branch() { Number = 3, Node1 = 2, Node2 = 0, Ohms_Act = 1, Ohms_React = 0, E_Act = 0, E_React = 0 });
        }

        private void Inputgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            if (headername == "Direction" | headername == "Reversed" | headername == "E" | headername == "Ohms")
            {
                e.Cancel = true;
            }
        }

        private void Shockpointgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            if (headername == "Node1" | headername == "Ohms_Act" | headername == "Ohms_React")
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var BranchOps = new BranchOps();
            try
            {
                Vector<Complex32> currents = BranchOps.CalcCurrents(branches);
                debug_label.Content = currents.ToString();
                GraphOps.CanvasDisplayResults(Canvas, currents);
            }
            catch (TopologyException TopologyException)
            {
                debug_label.Content = TopologyException;
            }
        }
        public string BranchesToString(List<Branch> branches)
        {
            string output = "";
            foreach (var branch in branches)
            {
                output = output + branch.Number + ", ";
            }
            return output;
        }
        public Rectangle branch(int width, int height, Brush color)
        {
            System.Windows.Shapes.Rectangle rect = new Rectangle();
            rect.Width = width;
            rect.Height = height;
            rect.Fill = color;
            return rect;
        }
        public Ellipse node(int width, int height, Brush color)
        {
            System.Windows.Shapes.Ellipse ell = new Ellipse();
            ell.Width = width;
            ell.Height = height;
            ell.Fill = color;
            return ell;
        }
        public Label text(string text, Brush color)
        {
            Label label = new Label();
            label.Content = text;
            label.Foreground = color;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            return label;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // used to draw the graph
        {
            if (inputgrid.SelectedItem == null)
            {
                GraphOps.CanvasLeftClick(Canvas, sender, e, "", Brushes.Blue);
            }
            if (inputgrid.SelectedItem != null)
            {
                Branch branch = new Branch();
                foreach (var item in datagrid_collection)
                {
                    if (inputgrid.SelectedItem == item)
                    {
                        branch = item;
                        int number = item.Number;
                        break;
                    }
                }
                string Uid = "Branch" + branch.Number.ToString();
                GraphOps.CanvasLeftClick(Canvas, sender, e, Uid, Brushes.Red);
            }
        }

        private void Canvas_Initialized(object sender, EventArgs e)
        {

        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e) // used to display the grid
        {
            GraphOps.DrawLineGrid(Canvas, 50);
        }

        private void inputgrid_CurrentCellChanged(object sender, EventArgs e) // updates branches form datagrids when they change
        {
            var InOutOps = new InOutOps();
            branches.Clear();
            branches = InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection);
        }

        private void sim_start_btn_Click(object sender, RoutedEventArgs e)
        {
            var InOutOps = new InOutOps();
            branches.Clear();
            branches.TrimExcess();
            branches = InOutOps.GetBranches_before(datagrid_collection);

            var BranchOps = new BranchOps();

            currents_start = BranchOps.CalcCurrents(branches);

            branches.Clear();
            branches.TrimExcess();
            branches = InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection);

            currents_shock = BranchOps.CalcCurrents(branches);


            debug_label.Content = currents_start.ToString();
            debug_label_2.Content = currents_shock.ToString();

            int sim_time = 0;
            int sim_step = 100;

            Timer = new System.Timers.Timer(100);
            Timer.Elapsed += OnTimedEvent2;
            Timer.AutoReset = true;
            Timer.Enabled = true;

            void OnTimedEvent2(Object source, ElapsedEventArgs e)
            {
                Transient.Do_a_Step_Linear(currents_start, currents_shock);
                Dispatcher.Invoke(() =>
                {
                    debug_label.Content = Transient.currents_now.ToString();
                    debug_label_2.Content = sim_time.ToString();
                });
                sim_time += sim_step;
            }

        }
    }
}
