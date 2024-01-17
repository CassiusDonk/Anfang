using Anfang.Powersystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        public List<Branch> branches = new List<Branch>();

        private static readonly ObservableCollection<ProtectionDevice> protectiongrid_collection = new ObservableCollection<ProtectionDevice>();
        private static readonly ObservableCollection<LogicString> logicgrid_collection = new ObservableCollection<LogicString>();
        private static readonly ObservableCollection<TripLevels> tripgrid_collection = new ObservableCollection<TripLevels>();
        private static readonly ObservableCollection<TimerDelays> delaygrid_collection = new ObservableCollection<TimerDelays>();
        private static readonly ObservableCollection<AnalogInputLink> analogsgrid_collection = new ObservableCollection<AnalogInputLink>();
        private static readonly ObservableCollection<BreakerLink> breakersgrid_collection = new ObservableCollection<BreakerLink>();
        private static readonly ObservableCollection<ResultsDisplay> resultconfig_collection = new ObservableCollection<ResultsDisplay>();
        public PowSys powersysgrid_collection = new PowSys();

        FileInteractions fileInteractions = new FileInteractions();

        public ObservableCollection<ProtectionDevice> protections = new ObservableCollection<ProtectionDevice>();

        public ProtectionDevice protectiongrid_selecteditem { get; set; }

        List<PowSysElementBase> ElementsToCopy = new List<PowSysElementBase>(); // this is a container used to store elements for copying.

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        bool tripEventSubscribed = false;
        bool propertyEventSubscribed = false;
        bool startEventSubscribed = false;

        public MainWindow()
        {
            InitializeComponent();

            protectiongrid_collection.Add(new ProtectionDevice() 
            {   
                label = "Prot1", 
                description = "Test protection"

            });

            protectiongrid.ItemsSource = protectiongrid_collection;
            powersysgrid.ItemsSource = powersysgrid_collection;
            resultconfig.ItemsSource = resultconfig_collection;
        }

        private void powersysgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            if (headername == "elnode1") { e.Column.Header = "У1"; }
            if (headername == "elnode2") { e.Column.Header = "У2"; }
            if (headername == "type") { e.Column.Header = "тип"; }
            if (headername == "property1") { e.Column.Header = "S/L"; }
            if (headername == "property2") { e.Column.Header = "x''/x_уд/Uk%"; }
            if (headername == "property3") { e.Column.Header = "E''/r_уд/Pk%"; }
            if (headername == "property4") { e.Column.Header = "x2г/Pх/b_уд"; }
            if (headername == "property5") { e.Column.Header = "x0г/iхх"; }
            if (headername == "grounded") { e.Column.Header = "зазем"; }
            if (headername == "ground_act") { e.Column.Header = "r_зазем"; }
            if (headername == "ground_react") { e.Column.Header = "x_зазем"; }
            if (headername == "voltage_side1") { e.Column.Header = "Uном1"; }
            if (headername == "voltage_side2") { e.Column.Header = "Uном2"; }
            if (headername == "currents_side1" | headername == "currents_side2")
            {
                e.Cancel = true;
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
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                string name = e.PropertyName;
                PowSysElementBase element = sender as PowSysElementBase;
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
            });
        }

        private void protectiongrid_AutoGeneratingColumn_1(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void protectiongrid_CurrentCellChanged_1(object sender, EventArgs e) // refresh protections on update of protectiongrid.
        {
            protections.Clear();
            foreach (var protection in protectiongrid_collection)
            {
                protections.Add(protection);
            }
        }

        private void protectiongrid_SelectionChanged(object sender, SelectionChangedEventArgs e) // this retrieves properties of the selected protection upon selection change and updates associated tables.
        {

        }
        private void breakersgrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }

        private void breakersgrid_CurrentCellChanged(object sender, EventArgs e)
        {

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
            branches.Clear();
            int number = 1;
            foreach (var element in powersysgrid_collection)
            {
                element.BuildModel();
                foreach (var branch in element.model)
                {
                    branch.Number = number;
                    branches.Add(branch);
                    number++;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // calculation routine
        {
            BuildGlobalModel();
            var BranchOps = new BranchOps();
            branchOps.CalculateNodeVoltages(branches, 0);
            foreach (var element in powersysgrid_collection)
            {
                element.UpdateResults();
            }
            DisplayResults();
            foreach (PowSysElementBase element in powersysgrid_collection)
            {
                if (element.type == "ABCN" |
                    element.type == "AN" |
                    element.type == "BN" |
                    element.type == "CN" |
                    element.type == "AB" |
                    element.type == "BC" |
                    element.type == "AC" |
                    element.type == "ABN" |
                    element.type == "BCN" |
                    element.type == "ACN")
                {
                    int node = element.elnode1;
                    foreach (PowSysElementBase element1 in powersysgrid_collection)
                    {
                        string Uid = element.GetUid();
                        if (element1.elnode1 == node)
                        {
                            GraphOps.DrawnItemPosition position = GraphOps.positions.Find(x => x.Uid == element1.GetUid());
                            if (position != null)
                            {
                                GraphOps.DrawShortCircuit(Canvas, (int)position.X1, (int)position.Y1, 0, 0, 5, Brushes.Red, Brushes.Red, true, element.GetUid(), element.GetLongUid());
                            }
                        }
                        if (element1.elnode2 == node)
                        {
                            GraphOps.DrawnItemPosition position = GraphOps.positions.Find(x => x.Uid == element1.GetUid());
                            if (position != null)
                            {
                                GraphOps.DrawShortCircuit(Canvas, (int)position.X2, (int)position.Y2, 0, 0, 5, Brushes.Red, Brushes.Red, true, element.GetUid(), element.GetLongUid());
                            }
                        }
                    }
                }
            }
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
            log_time.Text = "";
            logger.Clear();
            tokenSource = new CancellationTokenSource();
            sim_time = 0;
            int sim_step = 20;

            if (tripEventSubscribed == false)
            {
                foreach (var prot in protections)
                {
                    prot.Trip += Prot_Trip;
                }
                tripEventSubscribed = true;
            }

            if (startEventSubscribed == false)
            {
                foreach (var prot in protections)
                {
                    prot.Start += Prot_Start;
                }
                startEventSubscribed = true;
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

            Task.Run(Sim);

            async Task Sim()
            {
                while (true)
                {
                    await Tick();
                    await Task.Delay(200, stopSim);
                }
            }

            async Task Tick() // this is the main simulation routine performed on each cycle.
            {
                Dispatcher.Invoke(() =>
                {
                    BuildGlobalModel();
                });

                branchOps.CalculateNodeVoltages(branches, 0);

                foreach (var element in powersysgrid_collection)
                {
                    Dispatcher.Invoke(() =>
                    {
                        element.UpdateResults();
                    });
                }

                foreach (var prot in protections)
                {
                    prot.powersystem = powersysgrid_collection;
                    prot.sim_time = sim_time;
                    prot.sim_time_step = sim_step;
                    prot.UpdateInternalSignals(powersysgrid_collection);
                    prot.ProcessProtectionFunctions();
                }

                Dispatcher.Invoke(() =>
                {
                    DisplayResults();
                    log_time.Text = sim_time.ToString();
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
            if (startEventSubscribed)
            {
                foreach (var prot in protections)
                {
                    prot.Start -= Prot_Start;
                }
                startEventSubscribed = false;
            }
            foreach (var protectionDevice in protections)
            {
                protectionDevice.ResetDevices();
            }
        }

        public void Prot_Trip(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string property = e.PropertyName;
            ProtectionDevice protection = sender as ProtectionDevice;

            LogEvent(protection.label, property, "");
        }

        public void Prot_Start(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string property = e.PropertyName;
            ProtectionDevice protection = sender as ProtectionDevice;

            LogEvent(protection.label, property, "");
        }

        public void LogEvent(string obj, string property, string value)
        {
            if (property == "trip")
            {
                property = "Срабатывание";
            }
            if (property == "init")
            {
                property = "Пуск";
            }
            if (property == "property1")
            {
                property = "Фаза A";
            }
            if (property == "property2")
            {
                property = "Фаза B";
            }
            if (property == "property3")
            {
                property = "Фаза C";
            }
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
            savepwsbtn_txt.Text = "Saved!";
            loadpwsbtn_txt.Text = "Load Powersystem   (Загрузить схему)";
        }

        private void loadpwsbtn_Click(object sender, RoutedEventArgs e)
        {
            fileInteractions.ReconstructPowersystem(fileInteractions.LoadData(@"C:\Users\Default\Documents\AnfangPowersystem.txt"), powersysgrid_collection);
            loadpwsbtn_txt.Text = "Loaded!";
            savepwsbtn_txt.Text = "Save Powersystem (Сохранить схему)";
        }

        private void savepwsbtn_MouseLeave(object sender, MouseEventArgs e)
        {
            savepwsbtn_txt.Text = "Save Powersystem (Сохранить схему)";
        }

        private void loadpwsbtn_MouseLeave(object sender, MouseEventArgs e)
        {
            loadpwsbtn_txt.Text = "Load Powersystem   (Загрузить схему)";
        }

        private void protsavebtn_Click(object sender, RoutedEventArgs e)
        {
            fileInteractions.SaveData(fileInteractions.ProtectionDevicesToString(protectiongrid_collection), @"C:\Users\Default\Documents\AnfangProtections.txt");
        }

        private void protloadbtn_Click(object sender, RoutedEventArgs e)
        {
            protectiongrid_collection.Clear();
            foreach (var item in fileInteractions.LoadProtections(fileInteractions.LoadData(@"C:\Users\Default\Documents\AnfangProtections.txt")))
            {
                protectiongrid_collection.Add(item);
            }
        }

        private void protconfigbtn_Click(object sender, RoutedEventArgs e)
        {
            ProtectionEditor protectionEditor = new ProtectionEditor();
            protectionEditor.Owner = this;
            if (protectiongrid.SelectedItem != null)
            {
                ProtectionDevice original = protectiongrid.SelectedItem as ProtectionDevice;
                protectionEditor.protectionDevice = original.CreateCopy(original);
                protectionEditor.LinkProtectionDevice();
                protectionEditor.ShowDialog();
            }
            if (protectionEditor.DialogResult == true)
            {
                int index = protectiongrid_collection.IndexOf(protectiongrid.SelectedItem as ProtectionDevice);
                protectiongrid_collection.RemoveAt(index);
                protectiongrid_collection.Insert(index, protectionEditor.protectionDevice.CreateCopy(protectionEditor.protectionDevice));
            }
        }
    }
}
