using Anfang.Powersystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;

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

        public List<Branch> branches = new List<Branch>();

        private static readonly CustomObservable datagrid_collection = new CustomObservable();
        private static readonly ObservableCollection<Protection> protectiongrid_collection = new ObservableCollection<Protection>();
        private static readonly ObservableCollection<LogicString> logicgrid_collection = new ObservableCollection<LogicString>();
        private static readonly ObservableCollection<TripLevels> tripgrid_collection = new ObservableCollection<TripLevels>();
        private static readonly ObservableCollection<TimerDelays> delaygrid_collection = new ObservableCollection<TimerDelays>();
        private static readonly ObservableCollection<AnalogInputLink> analogsgrid_collection = new ObservableCollection<AnalogInputLink>();
        private static readonly ObservableCollection<BreakerLink> breakersgrid_collection = new ObservableCollection<BreakerLink>();
        private static readonly ObservableCollection<ResultsDisplay> resultconfig_collection = new ObservableCollection<ResultsDisplay>();
        public ObservableCollection<PowSysElementBase> powersysgrid_collection = new ObservableCollection<PowSysElementBase>();

        FileInteractions fileInteractions = new FileInteractions();

        public ObservableCollection<Protection> protections = new ObservableCollection<Protection>();

        public Protection protectiongrid_selecteditem { get; set; }

        List<PowSysElementBase> ElementsToCopy = new List<PowSysElementBase>();

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        bool tripEventSubscribed = false;
        bool propertyEventSubscribed = false;

        public MainWindow()
        {
            InitializeComponent();

            protectiongrid_collection.Add(new Protection() { label = "Prot1", init_label = "Comp1", trip_label = "Discrete1" });

            inputgrid.ItemsSource = datagrid_collection;
            protectiongrid.ItemsSource = protectiongrid_collection;
            logicgrid.ItemsSource = logicgrid_collection;
            tripgrid.ItemsSource = tripgrid_collection;
            delaygrid.ItemsSource = delaygrid_collection;
            analogsgrid.ItemsSource = analogsgrid_collection;
            breakersgrid.ItemsSource = breakersgrid_collection;
            powersysgrid.ItemsSource = powersysgrid_collection;
            resultconfig.ItemsSource = resultconfig_collection;
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
                property3 = 310,
                property4 = 70,
                property5 = 0.6F,
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
                id = 5,
                type = "LINE",
                elnode1 = 3,
                elnode2 = 4,
                property1 = 100,
                property2 = 0.435F,
                property3 = 0.121F,
                property4 = 2.6F
            });
            powersysgrid_collection.Add(new PowSysElementBase()
            {
                id = 3,
                type = "AN",
                elnode1 = 3,
            });

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
            if (element.type == "BRKR")
            {
                string breaker = $"BRKR (ID = {element.id})";
                if (e.PropertyName == "property1")
                {
                    LogEvent(breaker, e.PropertyName, element.property1.ToString());
                }
                if (e.PropertyName == "property2")
                {
                    LogEvent(breaker, e.PropertyName, element.property2.ToString());
                }
                if (e.PropertyName == "property3")
                {
                    LogEvent(breaker, e.PropertyName, element.property3.ToString());
                }
                GraphOps.TripBreakers(Canvas, element);
            }

            GraphOps.TripBreakers(Canvas, element);

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
                        tripgrid_collection.Add(new TripLevels() { Trip_Act = item });
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
                    if (item.Trip_Act.GetType().ToString() == "System.Single" | item.Trip_Act.GetType().ToString() == "System.Float")
                    {
                        protections[protections.IndexOf(protectiongrid_selecteditem)].tripLevels.Add(item.TripLevel);
                    }
                }
            }
        }

        private void delaygrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void delaygrid_CurrentCellChanged(object sender, EventArgs e)
        {
            try
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
            catch (System.ArgumentOutOfRangeException)
            {
                resultsbox.Text = "No prot selected!";
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

        }

        private void resultconfig_AutoGeneratedColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            if (headername == "results")
            {
                e.Cancel = true;
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
            branchOps.CalculateNodeVoltages(branches, 0);
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
            DisplayResults();
        }

        public void DisplayResults()
        {
            resultsbox.Clear();
            string newLine = Environment.NewLine;
            resultsbox.Text += $"Time: {sim_time} ms {newLine}";
            foreach (var resultconfig in resultconfig_collection)
            {
                resultconfig.BuildResults(powersysgrid_collection);
                resultsbox.Text += resultconfig.results;
            }
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (powersysgrid.SelectedItem != null)
            {
                string Uid = powersysgrid_collection[powersysgrid_collection.IndexOf((PowSysElementBase)powersysgrid.SelectedItem)].GetUid();
                string LongUid = powersysgrid_collection[powersysgrid_collection.IndexOf((PowSysElementBase)powersysgrid.SelectedItem)].GetLongUid();
                GraphOps.CanvasDrawOrRemoveElement(Canvas, sender, e, Uid, LongUid, Brushes.Red, false);
            }
            foreach (var element in powersysgrid_collection)
            {
                if (element.GetUid() == GraphOps.removed_uid)
                {
                    powersysgrid.SelectedItem = element;
                }
            }
        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (powersysgrid.SelectedItem != null)
            {
                string Uid = powersysgrid_collection[powersysgrid_collection.IndexOf((PowSysElementBase)powersysgrid.SelectedItem)].GetUid();
                string LongUid = powersysgrid_collection[powersysgrid_collection.IndexOf((PowSysElementBase)powersysgrid.SelectedItem)].GetLongUid();
                GraphOps.CanvasHoverOver(Canvas, sender, e, Uid, Brushes.Gray, LongUid);
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
            logger.Text = "";
            logger.Clear();
            tokenSource = new CancellationTokenSource();
            sim_time = 0;
            int sim_step = 20;
            bool logic_OK = false;

            foreach (var prot in protections)
            {
                prot.ResetLogic();
                prot.Initiate_logic();
                try
                {
                    prot.powersystem = powersysgrid_collection;
                    prot.sim_time = sim_time;
                    prot.sim_time_step = sim_step;
                    prot.TryEvaluateLogic();
                }
                catch (Exception)
                {
                    logic_OK = false;
                    resultsbox.Text = $"Logic evaluation failure: {prot.label} {Environment.NewLine}";
                    resultsbox.Text += "Sim aborted.";
                }
                logic_OK = true;
                prot.ResetLogic();
                prot.Initiate_logic();
            }

            if (tripEventSubscribed == false)
            {
                foreach (var prot in protections)
                {
                    prot.Trip += Prot_Trip;
                }
                tripEventSubscribed = true;
            }

            if (propertyEventSubscribed == false)
            {
                foreach (var element in powersysgrid_collection)
                {
                    element.PropertyChanged += Element_PropertyChanged;
                }
                propertyEventSubscribed = true;
            }

            CancellationToken stopSim = tokenSource.Token;

            if (logic_OK)
            {
                Task.Run(Sim);
            }

            async Task Sim()
            {
                while (true)
                {
                    await Tick();
                    await Task.Delay(10, stopSim);
                }
            }

            async Task Tick()
            {
                Dispatcher.Invoke(() =>
                {
                    BuildGlobalModel();
                });

                branchOps.CalculateNodeVoltages(branches, 0);

                foreach (var element in powersysgrid_collection)
                {
                    element.UpdateResults();
                }

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
                    debug_label.Content = protections[0].trip;
                    DisplayResults();
                });
                sim_time += sim_step;
            }
        }

        private void sim_stop_btn_Click(object sender, RoutedEventArgs e)
        { // Stop the simulation and reset everything.
            tokenSource.Cancel();
            sim_time = 0;
            if (tripEventSubscribed)
            {
                foreach (var prot in protections)
                {
                    prot.Trip -= Prot_Trip;
                }
                tripEventSubscribed = false;
            }
            if (propertyEventSubscribed)
            {
                foreach (var element in powersysgrid_collection)
                {
                    element.PropertyChanged -= Element_PropertyChanged;
                }
                propertyEventSubscribed = false;
            }
        }

        public void Prot_Trip(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string property = e.PropertyName;
            Protection protection = sender as Protection;

            LogEvent(protection.label, property, protection.trip.ToString());

        }

        public void LogEvent(string obj, string property, string value)
        {
            Dispatcher.Invoke(() =>
            {
                logger.AppendText($"{ sim_time } ms: {obj} {property} = {value} {Environment.NewLine}");
            });
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

        private void asyncSimBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void savepwsbtn_Click(object sender, RoutedEventArgs e)
        {
            fileInteractions.SaveData(fileInteractions.PowersystemToString(powersysgrid_collection), @"C:\Users\Default\Documents\AnfangPowersystem.txt");
            savepwsbtn.Content = "Saved!";
            loadpwsbtn.Content = "Load Powersystem";
        }

        private void loadpwsbtn_Click(object sender, RoutedEventArgs e)
        {
            fileInteractions.ReconstructPowersystem(fileInteractions.LoadData(@"C:\Users\Default\Documents\AnfangPowersystem.txt"), powersysgrid_collection);
            loadpwsbtn.Content = "Loaded!";
            savepwsbtn.Content = "Save Powersystem";
        }

        private void savepwsbtn_MouseLeave(object sender, MouseEventArgs e)
        {
            savepwsbtn.Content = "Save Powersystem";
        }

        private void loadpwsbtn_MouseLeave(object sender, MouseEventArgs e)
        {
            loadpwsbtn.Content = "Load Powersystem";
        }

        private void protsavebtn_Click(object sender, RoutedEventArgs e)
        {
            fileInteractions.SaveData(fileInteractions.ProtectionsToString(protectiongrid_collection), @"C:\Users\Default\Documents\AnfangProtections.txt");
        }

        private void protloadbtn_Click(object sender, RoutedEventArgs e)
        {
            fileInteractions.ReconstructProtections(fileInteractions.LoadData(@"C:\Users\Default\Documents\AnfangProtections.txt"), protectiongrid_collection);
        }
    }
}
