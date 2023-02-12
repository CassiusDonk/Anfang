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
        int sim_time = 0;

        GraphOps GraphOps = new GraphOps();

        public ObservableCollection<Branch> branches = new ObservableCollection<Branch>();
        public ObservableCollection<Branch> branches_end = new ObservableCollection<Branch>();

        private static readonly ObservableCollection<Branch> datagrid_collection = new ObservableCollection<Branch>();
        private static readonly ObservableCollection<Branch> shockpointgrid_collection = new ObservableCollection<Branch>();

        private List<Branch> drawn_items = new List<Branch>();
        System.Timers.Timer Timer;
        Transient Transient = new Transient(100);
        public Vector<Complex32> currents_start = Vector<Complex32>.Build.Random(1);
        public Vector<Complex32> currents_shock = Vector<Complex32>.Build.Random(1);


        public MainWindow()
        {
            InitializeComponent();
            branches.CollectionChanged += this.Branches_CollectionChanged;
            branches_end.CollectionChanged += this.Branches_end_CollectionChanged;
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
                Vector<Complex32> currents = BranchOps.CalcCurrents(branches.ToList<Branch>());
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
        { // currently unused
            System.Windows.Shapes.Rectangle rect = new Rectangle();
            rect.Width = width;
            rect.Height = height;
            rect.Fill = color;
            return rect;
        }
        public Ellipse node(int width, int height, Brush color)
        { // currently unused
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

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { // Canvas interaction using GraphOps.CanvasLeftClick.
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

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        { // Draw the grid when canvas is loaded.
            GraphOps.DrawLineGrid(Canvas, 50);
        }

        private void inputgrid_CurrentCellChanged(object sender, EventArgs e)
        { // Updates the list of branches from datagrids using InOutOps.GetBranches_after.
            var InOutOps = new InOutOps();
            branches.Clear();
            branches = InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection);
        }

        private void sim_start_btn_Click(object sender, RoutedEventArgs e)
        { // Simulation initiation.
            sim_time = 0;
            var InOutOps = new InOutOps();
            branches.Clear();
            branches = InOutOps.GetBranches_before(datagrid_collection);

            var BranchOps = new BranchOps();

            currents_start = BranchOps.CalcCurrents(branches.ToList<Branch>());

            if (branches_end.Count > 0)
            {
                branches_end.Clear();
                foreach (var branch in InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection))
                {
                    branches_end.Add(branch);
                }
                // this will automatically calculate currents by throwing "Branches_end_CollectionChanged" event.
            }
            else
            {
                foreach (var branch in InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection))
                {
                    branches_end.Add(branch);
                }
                // this will automatically calculate currents by throwing "Branches_end_CollectionChanged" event.
            }

            debug_label.Content = currents_start.ToString();
            debug_label_2.Content = currents_shock.ToString();

            int sim_step = 100;

            List<LogicDevices.BaseLogic> logic = new List<LogicDevices.BaseLogic>();
            logic.Add(new LogicDevices.Comparator("Kek"));
            logic.Add(new LogicDevices.Timer("lol"));
            logic[0].triplevel = new Complex32((float)0.5, (float)0);
            logic[1].delay = 500;
            logic[1].sim_time_step = 100;

            Timer = new System.Timers.Timer(100);
            Timer.Elapsed += OnTimedEvent2;
            Timer.AutoReset = true;
            Timer.Enabled = true;

            void OnTimedEvent2(Object source, ElapsedEventArgs e)
            {
                Transient.Do_a_Step_Linear(currents_start, currents_shock);
                logic[0].input_complex = Transient.currents_now[0];
                logic[1].sim_time = sim_time;
                logic[1].input_bool = logic[0].output;

                Dispatcher.Invoke(() =>
                {
                    debug_label.Content = Transient.currents_now.ToString();
                    debug_label_2.Content = sim_time.ToString();
                    debug_label_3.Content = logic[0].output.ToString() + logic[1].output.ToString();
                });
                sim_time += sim_step;
                if (sim_time == 3000)
                { // Test of the live changes in the topology.
                    branches_end.RemoveAt(branches_end.Count() - 1);
                }
            }

        }

        public object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return Activator.CreateInstance(type);
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }

        private void sim_stop_btn_Click(object sender, RoutedEventArgs e)
        { // Stop the simulation and reset everything.
            Timer.Enabled = false;
            Timer.Close();
            Transient.Reset();
        }

        private void Branches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void Branches_end_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        { // React to change in topology, restart the transient calculation
            var BranchOps = new BranchOps();
            if (sim_time > 0)
            {
                currents_start = Transient.currents_now.Clone();
            }
            try
            {
                currents_shock = BranchOps.CalcCurrents(branches_end.ToList<Branch>());
            }
            catch (TopologyException)
            {

            }
            Transient.Reset();
        }
    }
}
