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
using Anfang.Powersystem;

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

        private static readonly CustomObservable datagrid_collection = new CustomObservable();
        private static readonly ObservableCollection<Protection> protectiongrid_collection = new ObservableCollection<Protection>();
        private static readonly ObservableCollection<LogicString> logicgrid_collection = new ObservableCollection<LogicString>();
        private static readonly ObservableCollection<TripLevels> tripgrid_collection = new ObservableCollection<TripLevels>();
        private static readonly ObservableCollection<TimerDelays> delaygrid_collection = new ObservableCollection<TimerDelays>();
        private static readonly ObservableCollection<AnalogInputLink> analogsgrid_collection = new ObservableCollection<AnalogInputLink>();
        private static readonly ObservableCollection<BreakerLink> breakersgrid_collection = new ObservableCollection<BreakerLink>();
        public ObservableCollection<PowSysElementBase> powersysgrid_collection = new ObservableCollection<PowSysElementBase>();

        System.Timers.Timer Timer;

        public ObservableCollection<Protection> protections = new ObservableCollection<Protection>();

        public Protection protectiongrid_selecteditem { get; set; }

        List<PowSysElementBase> ElementsToCopy = new List<PowSysElementBase>();

        //public Protection protectiongrid_selecteditem_old = new Protection();

        public MainWindow()
        {
            InitializeComponent();
            branches.CollectionChanged += this.Branches_CollectionChanged;
            //datagrid_collection.Add(new Branch() { Number = 1, Node1 = 0, Node2 = 1, Ohms_Act = 1, Ohms_React = 0, E_Act = 1, E_React = 0 });
            //datagrid_collection.Add(new Branch() { Number = 2, IsBreaker = true, Enabled = true, Node1 = 1, Node2 = 2, Ohms_Act = 1, Ohms_React = 0, E_Act = 1, E_React = 0 });
            //datagrid_collection.Add(new Branch() { Number = 3, Node1 = 2, Node2 = 3, Ohms_Act = 1, Ohms_React = 0, E_Act = 0, E_React = 0 });
            //datagrid_collection.Add(new Branch() { Number = 1, Node1 = 0, Node2 = 1, Ohms_Act = 1, Ohms_React = 0, E_Act = 1, E_React = 0 });
            //datagrid_collection.Add(new Branch() { Number = 2, Node1 = 1, Node2 = 2, Ohms_Act = 1, Ohms_React = 0, E_Act = 0, E_React = 0, IsBreaker = true, Enabled = true });
            //datagrid_collection.Add(new Branch() { Number = 3, Node1 = 2, Node2 = 0, Ohms_Act = 1, Ohms_React = 0, E_Act = 0, E_React = 0 });

            protectiongrid_collection.Add(new Protection() { label = "Prot1", init_label = "Comp1", trip_label = "Discrete1" });

            inputgrid.ItemsSource = datagrid_collection;
            protectiongrid.ItemsSource = protectiongrid_collection;
            logicgrid.ItemsSource = logicgrid_collection;
            tripgrid.ItemsSource = tripgrid_collection;
            delaygrid.ItemsSource = delaygrid_collection;
            analogsgrid.ItemsSource = analogsgrid_collection;
            breakersgrid.ItemsSource = breakersgrid_collection;
            powersysgrid.ItemsSource = powersysgrid_collection;
            powersysgrid_collection.Add(new PowSysElementBase()
            {
                id = 1,
                type = "GEN",
                elnode1 = 0,
                elnode2 = 1,
                property3 = 63.51F,
                property1 = 75,
                property2 = 0.146F,
                grounded = false,
                ground_act = 0,
                ground_react = 0,
                voltage_side2 = 115
            });
            powersysgrid_collection.Add(new PowSysElementBase()
            {
                id = 2,
                type = "TRANM",
                elnode1 = 2,
                elnode2 = 1,
                property1 = 80,
                property2 = 10.5F,
                property3 = 10,
                property4 = 20,
                property5 = 10,
                voltage_side2 = 115,
                grounded = true,
                ground_act = 0,
                ground_react = 0
            });
            powersysgrid_collection.Add(new PowSysElementBase()
            {
                id = 4,
                type = "BRKR",
                elnode1 = 2,
                elnode2 = 3,
                property1 = 1,
                property2 = 1,
                property3 = 1
            });
            powersysgrid_collection.Add(new PowSysElementBase()
            {
                id = 3,
                type = "AN",
                elnode1 = 3,
            });
            foreach (var element in powersysgrid_collection)
            {
                element.PropertyChanged += Element_PropertyChanged;
                //element.ResultsChanged += 
            }
        }

        private void powersysgrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ElementsToCopy.Clear();
            foreach (var item in e.AddedItems)
            {
                ElementsToCopy.Add(item as PowSysElementBase);
            }
        }

        private void Element_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string name = e.PropertyName;
            PowSysElementBase element = sender as PowSysElementBase;
            datagrid_collection.FindCommonID(element.id);
            int i = 0;
            foreach (var branch in datagrid_collection.FindCommonID(element.id))
            {
                datagrid_collection.UpdateProperty(datagrid_collection[i], "Enabled", element.model[i].Enabled, false);
                i++;
            }
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
                headername == "Current" |
                headername == "Voltage_Node1" |
                headername == "Voltage_Node2")
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
            //logicgrid_collection.Clear();
            protections.Clear();
            foreach (var protection in protectiongrid_collection)
            {
                protections.Add(protection);
            }
        }

        private void protectiongrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { // Update all conncected grids - retrive properties from the currently selected protection
            logicgrid_collection.Clear();
            tripgrid_collection.Clear();
            delaygrid_collection.Clear();
            analogsgrid_collection.Clear();
            breakersgrid_collection.Clear();
            if (protectiongrid.SelectedItem != null)
            {
                if (protectiongrid.SelectedItem.ToString() == "Anfang.Protection")
                {
                    protectiongrid_selecteditem = (Protection)protectiongrid.SelectedItem;
                }
                else
                {
                    protectiongrid_selecteditem = null;
                }
            }
            else
            {
                protectiongrid_selecteditem = null;
            }

            if (protectiongrid_selecteditem != null)
            {
                if (protectiongrid_selecteditem.ToString() == "Anfang.Protection")
                {
                    foreach (var item in protections[protections.IndexOf((Protection)protectiongrid.SelectedItem)].logic_config)
                    {
                        logicgrid_collection.Add(new LogicString() { logic_string = item });
                    }
                    foreach (var item in protections[protections.IndexOf((Protection)protectiongrid.SelectedItem)].tripLevels)
                    {
                        tripgrid_collection.Add(new TripLevels() { Trip_Act = item.Real, Trip_React = item.Imaginary });
                    }
                    foreach (var item in protections[protections.IndexOf((Protection)protectiongrid.SelectedItem)].timer_delays)
                    {
                        delaygrid_collection.Add(new TimerDelays { delay = item });
                    }
                    foreach (var item in protections[protections.IndexOf((Protection)protectiongrid.SelectedItem)].analogInputLinks)
                    {
                        analogsgrid_collection.Add(new AnalogInputLink { isVoltage = item.isVoltage, side = item.side, id = item.id, phase = item.phase });
                    }
                    foreach (var item in protections[protections.IndexOf((Protection)protectiongrid.SelectedItem)].breaker_numbers)
                    {
                        breakersgrid_collection.Add(new BreakerLink() { brerakerID = item });
                    }
                }
                else
                {
                    debug_label_2.Content = protectiongrid_selecteditem;
                    protectiongrid_selecteditem = null;
                }
            }
            debug_label_2.Content = protectiongrid_selecteditem;
        }

        private void logicgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void logicgrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (protectiongrid_selecteditem != null)
            {
                protections[protections.IndexOf(protectiongrid_selecteditem)].logic_config.Clear();
                foreach (var item in logicgrid_collection)
                {
                    if (item.logic_string != "")
                    {
                        protections[protections.IndexOf(protectiongrid_selecteditem)].logic_config.Add(item.logic_string);
                    }
                }
            }
        }

        private void logicgrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tripgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void tripgrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (protectiongrid_selecteditem != null)
            {
                protections[protections.IndexOf(protectiongrid_selecteditem)].tripLevels.Clear();
                foreach (var item in tripgrid_collection)
                {
                    if (item.Trip_Act.GetType().ToString() == "System.Single" & item.Trip_React.GetType().ToString() == "System.Single")
                    {
                        protections[protections.IndexOf(protectiongrid_selecteditem)].tripLevels.Add(item.TripLevel);
                    }
                    debug_label_3.Content = item.Trip_React.GetType().ToString();
                }
            }
        }

        private void delaygrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void delaygrid_CurrentCellChanged(object sender, EventArgs e)
        {
            protections[protections.IndexOf(protectiongrid_selecteditem)].timer_delays.Clear();
            foreach (var item in delaygrid_collection)
            {
                if (item.delay.GetType().ToString() == "System.Int32")
                {
                    protections[protections.IndexOf(protectiongrid_selecteditem)].timer_delays.Add(item.delay);
                }
                debug_label_3.Content = item.delay.GetType().ToString();
            }
        }

        private void analogsgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void analogsgrid_CurrentCellChanged(object sender, EventArgs e)
        {
            protections[protections.IndexOf(protectiongrid_selecteditem)].analogInputLinks.Clear();
            foreach (var item in analogsgrid_collection)
            {
                if (item.id.GetType().ToString() == "System.Int32")
                {
                    protections[protections.IndexOf(protectiongrid_selecteditem)].analogInputLinks.Add(
                        new AnalogInputLink() { isVoltage = item.isVoltage, id = item.id, phase = item.phase, side = item.side });
                }
                debug_label_3.Content = item.id.GetType().ToString();
            }
        }

        private void breakersgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void breakersgrid_CurrentCellChanged(object sender, EventArgs e)
        {
            protections[protections.IndexOf(protectiongrid_selecteditem)].breaker_numbers.Clear();
            foreach (var item in breakersgrid_collection)
            {
                if (item.brerakerID.GetType().ToString() == "System.Int32")
                {
                    protections[protections.IndexOf(protectiongrid_selecteditem)].breaker_numbers.Add(item.brerakerID);
                }
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

                }
                branchOps.collection_changed = false;
            }
        }

        public void BuildGlobalModel()
        {
            datagrid_collection.Clear();
            branches.Clear();
            int number = 1;
            foreach (var element in powersysgrid_collection)
            {
                element.BuildModel();
                foreach (var branch in element.model)
                {
                    branch.Number = number;
                    datagrid_collection.Add(branch);
                    branches.Add(branch);
                    number++;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BuildGlobalModel();
            var BranchOps = new BranchOps();
            BranchOps.CalculateNodeVoltages(branches, 0);
            for (int i = 0; i < branches.Count(); i++)
            {
                datagrid_collection[i].Current = branches[i].Current;
                datagrid_collection[i].Voltage_Node1 = branches[i].Voltage_Node1;
                datagrid_collection[i].Voltage_Node2 = branches[i].Voltage_Node2;
            }
            foreach (var element in powersysgrid_collection)
            {
                element.UpdateResults();
            }
            //GraphOps.CanvasDisplayResults(Canvas, branches);
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (powersysgrid.SelectedItem == null)
            {

            }
            //if (inputgrid.SelectedItem != null)
            //{
            //string Uid = "Branch" + datagrid_collection[datagrid_collection.IndexOf((Branch)inputgrid.SelectedItem)].Number;
            //GraphOps.CanvasLeftClick(Canvas, sender, e, Uid, Brushes.Red);
            //}
            if (powersysgrid.SelectedItem != null)
            {
                string Uid = powersysgrid_collection[powersysgrid_collection.IndexOf((PowSysElementBase)powersysgrid.SelectedItem)].GetUid();
                GraphOps.CanvasDrawOrRemoveElement(Canvas, sender, e, Uid, Brushes.Red, false);
            }
        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (powersysgrid.SelectedItem != null)
            {
                string Uid = powersysgrid_collection[powersysgrid_collection.IndexOf((PowSysElementBase)powersysgrid.SelectedItem)].GetUid();
                GraphOps.CanvasHoverOver(Canvas, sender, e, Uid, Brushes.Gray);
            }
        }

        private void Canvas_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Canvas.IsMouseDirectlyOver == false)
            {
                //GraphOps.CanvasClearPreview(Canvas);
            }
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            GraphOps.CanvasClearPreview(Canvas);
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GraphOps.rotation++;
        }

        private void Canvas_Initialized(object sender, EventArgs e)
        {

        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        { // Draw the grid when canvas is loaded.
            GraphOps.gridsize = 20;
            GraphOps.DrawLineGrid(Canvas, GraphOps.gridsize);
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


            Timer = new System.Timers.Timer(200);
            Timer.Elapsed += OnTimedEvent2;
            Timer.AutoReset = true;
            Timer.Enabled = true;

            foreach (var prot in protections)
            {
                prot.Initiate_logic();
            }

            void OnTimedEvent2(Object source, ElapsedEventArgs e)
            {
                if (sim_time == sim_step)
                {
                    branches.Clear();
                    foreach (var branch in inOutOps.GetBranches_before(datagrid_collection))
                    {
                        branches.Add(branch);
                    }
                }
                branchOps.CalcTranCurrents_to_branches(branches, sim_step);
                //if (prot.analogInputs.Count <= 0)
                //{
                //prot.analogInputs.Add(branches[0].Current);
                //prot.analogInputs.Add(branches[1].Current);
                //}
                //prot.analogInputs[0] = branches[0].Current;
                //prot.analogInputs[1] = branches[1].Current;
                foreach (var prot in protections)
                {
                    prot.powersystem = powersysgrid_collection;
                    prot.getAnalogs();
                    prot.sim_time = sim_time;
                    prot.sim_time_step = sim_step;
                    prot.EvaluateLogic();
                }

                Dispatcher.Invoke(() =>
                {
                    if (branches.Count > 0)
                    {

                    }
                    debug_label.Content = protections[0].trip;
                });
                sim_time += sim_step;
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

        private void pastebtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ElementsToCopy)
            {
                powersysgrid_collection.Add(new PowSysElementBase() 
                { 
                    id = item.id,
                    elnode1 = item.elnode1,
                    elnode2 = item.elnode2,
                    type = item.type,
                    property1 = item.property1,
                    property2 = item.property2,
                    property3 = item.property3,
                    property4 = item.property4,
                    property5 = item.property5,
                    property6 = item.property6,
                    grounded = item.grounded,
                    ground_act = item.ground_act,
                    ground_react = item.ground_react,
                    voltage_side1 = item.voltage_side1,
                    voltage_side2 = item.voltage_side2
                });
            }
        }
    }
}
