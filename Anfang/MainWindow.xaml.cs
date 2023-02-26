using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MathNet.Numerics;
using System.Collections.ObjectModel;
using System.Timers;
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
        BranchOps branchOps = new BranchOps();

        public CustomObservable branches = new CustomObservable();

        private static readonly ObservableCollection<Branch> datagrid_collection = new ObservableCollection<Branch>();
        private static readonly ObservableCollection<Branch> shockpointgrid_collection = new ObservableCollection<Branch>();
        private static readonly ObservableCollection<Branch> outputgrid_collection = new ObservableCollection<Branch>();
        private static readonly ObservableCollection<Protection> protectiongrid_collection = new ObservableCollection<Protection>();
        private static readonly ObservableCollection<LogicString> logicgrid_collection = new ObservableCollection<LogicString>();

        System.Timers.Timer Timer;

        public ObservableCollection<Protection> protections = new ObservableCollection<Protection>();

        public Protection protectiongrid_selecteditem = new Protection();

        public MainWindow()
        {
            InitializeComponent();
            branches.CollectionChanged += this.Branches_CollectionChanged;
            datagrid_collection.Add(new Branch() { Number = 1, Node1 = 0, Node2 = 1, Ohms_Act = 1, Ohms_React = 0, E_Act = 1, E_React = 0 });
            datagrid_collection.Add(new Branch() { Number = 2, IsBreaker = true, Enabled = true, Node1 = 1, Node2 = 2, Ohms_Act = 1, Ohms_React = 0, E_Act = 1, E_React = 0 });
            datagrid_collection.Add(new Branch() { Number = 3, Node1 = 2, Node2 = 3, Ohms_Act = 1, Ohms_React = 0, E_Act = 0, E_React = 0 });
            datagrid_collection.Add(new Branch() { Number = 4, Node1 = 3, Node2 = 0, Ohms_Act = 1, Ohms_React = 0, E_Act = 0, E_React = 0 });
            inputgrid.ItemsSource = datagrid_collection;
            shockpointgrid.ItemsSource = shockpointgrid_collection;
            outputgrid.ItemsSource = outputgrid_collection;
            protectiongrid.ItemsSource = protectiongrid_collection;
            logicgrid.ItemsSource = logicgrid_collection;
        }

        private void Inputgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            if (headername == "Number" |
                headername == "IsBreaker" |
                headername == "Enabled" |
                headername == "Node1" |
                headername == "Node2" |
                headername == "Ohms_Act" |
                headername == "Ohms_React" |
                headername == "E_Act" |
                headername == "E_React" |
                headername == "Current")
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void inputgrid_CurrentCellChanged(object sender, EventArgs e)
        { // Updates the list of branches from datagrids using InOutOps.GetBranches_after.
            var InOutOps = new InOutOps();
            if (branches.Count() == 0)
            {
                foreach (var branch in InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection))
                {
                    branches.Add(branch);
                }
            }
            else
            {
                List<Branch> branches_to_remove = new List<Branch>();
                foreach (var branch in branches)
                {
                    int index = InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection).
                        ToList<Branch>().FindIndex(x => x.Number == branch.Number);
                    if (index == -1)
                    {
                        branches_to_remove.Add(branch);
                    }
                }
                foreach (var branch in branches_to_remove)
                {
                    branches.Remove(branch);
                }
                foreach (var branch in InOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection))
                {
                    int index = branches.ToList<Branch>().FindIndex(x => x.Number == branch.Number);
                    if (index == -1)
                    {
                        branches.Add(branch);
                    }
                    else
                    {
                        branches.UpdateBranch(branches[index], branch);
                    }
                }
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

        private void Outputgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            if (headername == "Number" |
                headername == "Current")
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void protectiongrid_AutoGeneratingColumn_1(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            if (headername == "label" |
                headername == "trip_label" |
                headername == "init_label")
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void protectiongrid_CurrentCellChanged_1(object sender, EventArgs e)
        {
            logicgrid_collection.Clear();
            protections.Clear();
            foreach (var protection in protectiongrid_collection)
            {
                protections.Add(protection);
            }
            try
            {
                if (protectiongrid.SelectedItem != null)
                {
                    protectiongrid_selecteditem = (Protection)protectiongrid.SelectedItem;
                    foreach (var item in protections[protections.IndexOf(protectiongrid_selecteditem)].logic_config)
                    {
                        logicgrid_collection.Add(new LogicString() { logic_string = item });
                    }
                }
            }
            catch (System.InvalidCastException)
            {

            }
        }

        private void logicgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void logicgrid_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in logicgrid_collection)
                {
                    if (item.logic_string != "")
                    {
                        try
                        {

                        }
                        catch (System.InvalidCastException)
                        {

                        }
                        protections[protections.IndexOf(protectiongrid_selecteditem)].logic_config.Add(item.logic_string);
                    }
                }
            }
            catch (System.InvalidCastException)
            {

            }
        }

        private void Branches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Timer != null)
            { // set "collection_changed" property to true so branchOps can know that collection was modified.
                if (Timer.Enabled == true)
                {
                    branchOps.collection_changed = true;
                }
            }
            else
            {
                if (branches.Count > 0)
                {
                    outputgrid_collection.Clear();
                    foreach (var branch in branches)
                    {
                        outputgrid_collection.Add(branch);
                    }
                }
                branchOps.collection_changed = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var BranchOps = new BranchOps();
            try
            {
                BranchOps.CalcCurrents_to_branches(branches, true);
            }
            catch (TopologyException TopologyException)
            {
                debug_label.Content = TopologyException;
            }
            GraphOps.CanvasDisplayResults(Canvas, branches);
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { // Canvas interaction using GraphOps.CanvasLeftClick.
            if (inputgrid.SelectedItem == null)
            {
                GraphOps.CanvasLeftClick(Canvas, sender, e, "", Brushes.Blue);
            }
            if (inputgrid.SelectedItem != null)
            {
                string Uid = "Branch" + datagrid_collection[datagrid_collection.IndexOf((Branch)inputgrid.SelectedItem)].Number;
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

        private void sim_start_btn_Click(object sender, RoutedEventArgs e)
        { // Simulation initiation.
            if (Timer != null)
            {
                if (Timer.Enabled == true)
                {
                    Timer.Enabled = false;
                    Timer.Close();
                    branchOps.ResetTran();
                    sim_time = 0;
                }
            }
            branchOps.ResetTran();
            sim_time = 0;
            branches.Clear();
            InOutOps inOutOps = new InOutOps();
            foreach (var branch in inOutOps.GetBranches_before(datagrid_collection))
            {
                branches.Add(branch);
            }

            int sim_step = 100;

            Protection prot = new Protection();
            List<String> logic_config = new List<String>();
            logic_config.Add("analog1");
            logic_config.Add("comp1");
            logic_config.Add("analog2");
            logic_config.Add("comp2");
            logic_config.Add("or1");
            logic_config.Add("timer1");
            //logic_config.Add("invert1");
            logic_config.Add("discrete1");
            prot.init_label = "or1";
            prot.trip_label = "discrete1";
            prot.logic_config = logic_config;
            prot.sim_time_step = sim_step;
            prot.timer_delays.Add(1000);
            prot.tripLevels.Add(new Complex32((float)0.5, 0));
            prot.tripLevels.Add(new Complex32((float)0.5, 0));
            prot.Initiate_logic();


            Timer = new System.Timers.Timer(200);
            Timer.Elapsed += OnTimedEvent2;
            Timer.AutoReset = true;
            Timer.Enabled = true;

            void OnTimedEvent2(Object source, ElapsedEventArgs e)
            {
                if (sim_time == sim_step)
                {
                    branches.Clear();
                    foreach (var branch in inOutOps.GetBranches_after(datagrid_collection, shockpointgrid_collection))
                    {
                        branches.Add(branch);
                    }
                }
                branchOps.CalcTranCurrents_to_branches(branches, sim_step);
                if (prot.analogInputs.Count <= 0)
                {
                    prot.analogInputs.Add(branches[0].Current);
                    prot.analogInputs.Add(branches[1].Current);
                }
                prot.analogInputs[0] = branches[0].Current;
                prot.analogInputs[1] = branches[1].Current;
                prot.sim_time = sim_time;
                prot.EvaluateLogic();

                Dispatcher.Invoke(() =>
                {
                    if (branches.Count > 0)
                    {
                        outputgrid_collection.Clear();
                        foreach (var branch in branches)
                        {
                            outputgrid_collection.Add(branch);
                        }
                    }
                    debug_label.Content = prot.trip;
                });
                sim_time += sim_step;
                if (prot.trip == true)
                { // Test of the live changes in the topology.
                    if (branches[1].Enabled)
                    {
                        branches.UpdateProperty(branches[1], "Enabled", false, true);
                    }
                }
            }

        }

        public object GetInstance(string strFullyQualifiedName)
        { // Currently unused. Left for reference purposes.
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
            branchOps.ResetTran();
            sim_time = 0;
        }
    }
}
