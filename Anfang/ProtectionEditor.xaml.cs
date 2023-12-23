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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Anfang.LogicDevices;

namespace Anfang
{
    /// <summary>
    /// Логика взаимодействия для LogicCode_editor.xaml
    /// </summary>
    public partial class ProtectionEditor : Window
    {
        public ProtectionDevice protectionDevice = new ProtectionDevice();
        private static readonly ObservableCollection<AnalogInputLink> AnalogInputsCollection = new ObservableCollection<AnalogInputLink>();
        private static readonly ObservableCollection<DiscreteInputLink> DiscreteInputsCollection = new ObservableCollection<DiscreteInputLink>();
        private static readonly ObservableCollection<BaseLogicV2> InternalSignalsCollection = new ObservableCollection<BaseLogicV2>();
        private static readonly ObservableCollection<ProtectionFunctionV2> FunctionsCollection = new ObservableCollection<ProtectionFunctionV2>();
        private static readonly ObservableCollection<BaseLogicV2> LogicCollection = new ObservableCollection<BaseLogicV2>();
        private static readonly ObservableCollection<BreakerLink> BreakersCollection = new ObservableCollection<BreakerLink>();

        private ProtectionFunctionV2 selectedFunction = new ProtectionFunctionV2();

        public ProtectionEditor()
        {
            InitializeComponent();
            AnalogInputsGrid.ItemsSource = AnalogInputsCollection;
            DiscreteInputsGrid.ItemsSource = DiscreteInputsCollection;
            InternalSignalsGrid.ItemsSource = InternalSignalsCollection;
            FunctionsGrid.ItemsSource = FunctionsCollection;
            LogicGrid.ItemsSource = LogicCollection;
            BreakersGrid.ItemsSource = BreakersCollection;

            InternalSignalsCollection.Clear();
            AnalogInputsCollection.Clear();
            DiscreteInputsCollection.Clear();
            FunctionsCollection.Clear();
            LogicCollection.Clear();
            BreakersCollection.Clear();
        }

        public void LinkProtectionDevice()
        {
            protectionDevice.UpdateInternalSignals(((MainWindow)Application.Current.MainWindow).powersysgrid_collection);
            foreach (var item in protectionDevice.analogInputLinks)
            {
                AnalogInputsCollection.Add(item);
            }
            foreach (var item in protectionDevice.discreteInputLinks)
            {
                DiscreteInputsCollection.Add(item);
            }
            foreach (var item in protectionDevice.protectionFunctions)
            {
                FunctionsCollection.Add(item);
            }
            foreach (var item in protectionDevice.breakerLinks)
            {
                BreakersCollection.Add(item);
            }
            foreach (var item in protectionDevice.internalAnalogs)
            {
                InternalSignalsCollection.Add(item);
            }
            foreach (var item in protectionDevice.internalDiscretes)
            {
                InternalSignalsCollection.Add(item);
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            protectionDevice.analogInputLinks = AnalogInputsCollection.ToList();
            protectionDevice.discreteInputLinks = DiscreteInputsCollection.ToList();
            protectionDevice.protectionFunctions = FunctionsCollection.ToList();
            protectionDevice.breakerLinks = BreakersCollection.ToList();
            this.DialogResult = true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void ProcessLinks_Click(object sender, RoutedEventArgs e)
        {
            protectionDevice.analogInputLinks = AnalogInputsCollection.ToList();
            protectionDevice.discreteInputLinks = DiscreteInputsCollection.ToList();
            protectionDevice.UpdateInternalSignals(((MainWindow)Application.Current.MainWindow).powersysgrid_collection);
            InternalSignalsCollection.Clear();
            foreach (var item in protectionDevice.internalAnalogs)
            {
                InternalSignalsCollection.Add(item);
            }
            foreach (var item in protectionDevice.internalDiscretes)
            {
                InternalSignalsCollection.Add(item);
            }
        }

        private void SaveLogic_Click(object sender, RoutedEventArgs e)
        {
            if (FunctionsGrid.SelectedItem != null)
            {
                selectedFunction.LogicDevices.Clear();
                foreach (var item in LogicCollection)
                {
                    selectedFunction.LogicDevices.Add(item);
                }
            }
            protectionDevice.ProcessProtectionFunctions();
        }

        private void FunctionsGrid_CurrentCellChanged(object sender, EventArgs e)
        {

        }

        private void FunctionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogicCollection.Clear();
            if (FunctionsGrid.SelectedItem != null)
            {
                if (FunctionsGrid.SelectedItem.GetType().ToString().Contains("ProtectionFunction"))
                {
                    selectedFunction = FunctionsCollection[FunctionsCollection.IndexOf((ProtectionFunctionV2)FunctionsGrid.SelectedItem)];
                }
            }
            foreach (var item in selectedFunction.LogicDevices)
            {
                LogicCollection.Add(item);
            }
        }

        private void AddElementBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FunctionsGrid.SelectedItem != null)
            {
                if (FunctionsGrid.SelectedItem.GetType().ToString().Contains("ProtectionFunction"))
                {
                    NewLogicElementDialog newLogicElementDialog = new NewLogicElementDialog();
                    newLogicElementDialog.Owner = this;
                    newLogicElementDialog.LoadLinkOptions(LogicCollection);
                    newLogicElementDialog.ShowDialog();
                    if (newLogicElementDialog.DialogResult == true)
                    {
                        LogicCollection.Add(newLogicElementDialog.LogicDevice);
                    }
                }
            }
        }
    }
}
