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
using System.Globalization;

namespace Anfang
{
    /// <summary>
    /// Логика взаимодействия для NewLogicElement.xaml
    /// </summary>
    public partial class NewLogicElementDialog : Window
    {
        bool editing = false;
        private static readonly ObservableCollection<string> LogicOptionsCollection = new ObservableCollection<string>();
        private static readonly ObservableCollection<string> LinkOptionsCollection = new ObservableCollection<string>();
        private static readonly ObservableCollection<string> SelectedLinksCollection = new ObservableCollection<string>();

        private class ExtendedLabel
        {
           public string Label = "";
           public string LinkLabel = "";
        }

        private List<ExtendedLabel> ExtendedLabels = new List<ExtendedLabel>();

        public BaseLogicV2 LogicElement = new BaseLogicV2();
        public NewLogicElementDialog(ObservableCollection<BaseLogicV2> LogicDevices)
        {
            InitializeComponent();
            LogicOptionsList.ItemsSource = LogicOptionsCollection;
            LogicOptionsCollection.Clear();
            LogicOptionsCollection.Add("Лог. И");
            LogicOptionsCollection.Add("Лог. ИЛИ");
            LogicOptionsCollection.Add("Лог. Инверсия");
            LogicOptionsCollection.Add("Компаратор");
            LogicOptionsCollection.Add("Сравнение фаз");
            LogicOptionsCollection.Add("Выдержка на сраб.");
            LogicOptionsCollection.Add("Аналог. выход");
            LogicOptionsCollection.Add("Дискрет. выход");
            LogicOptionsCollection.Add("Аналог. вход");
            LogicOptionsCollection.Add("Дискрет. вход");

            Delay1Box.IsEnabled = false;
            Delay2Box.IsEnabled = false;
            TripLevelBox.IsEnabled = false;

            LinkOptionsList.ItemsSource = LinkOptionsCollection;
            SelectedLinksList.ItemsSource = SelectedLinksCollection;
            SelectedLinksCollection.Clear();

            LoadLinkOptions(LogicDevices);
        }

        public NewLogicElementDialog(ObservableCollection<BaseLogicV2> LogicDevices, BaseLogicV2 logicElement)
        {
            InitializeComponent();
            this.LogicElement = logicElement;
            editing = true;
            LogicOptionsList.ItemsSource = LogicOptionsCollection;
            LogicOptionsCollection.Clear();
            LogicOptionsCollection.Add("Лог. И");
            LogicOptionsCollection.Add("Лог. ИЛИ");
            LogicOptionsCollection.Add("Лог. Инверсия");
            LogicOptionsCollection.Add("Компаратор");
            LogicOptionsCollection.Add("Сравнение фаз");
            LogicOptionsCollection.Add("Выдержка на сраб.");
            LogicOptionsCollection.Add("Аналог. выход");
            LogicOptionsCollection.Add("Дискрет. выход");
            LogicOptionsCollection.Add("Аналог. вход");
            LogicOptionsCollection.Add("Дискрет. вход");

            Delay1Box.IsEnabled = false;
            Delay2Box.IsEnabled = false;
            TripLevelBox.IsEnabled = false;

            LoadLinkOptions(LogicDevices);

            LinkOptionsList.ItemsSource = LinkOptionsCollection;
            SelectedLinksList.ItemsSource = SelectedLinksCollection;
            SelectedLinksCollection.Clear();
            foreach (var inputLink in logicElement.InputLinks)
            {
                ExtendedLabel link = ExtendedLabels.Find(x => x.Label == inputLink);
                if (link != null)
                {
                    SelectedLinksCollection.Add(link.LinkLabel);
                }
            }
            LabelBox.Text = logicElement.Label;

            string logicElementType = logicElement.GetType().ToString();

            if (logicElementType == "Anfang.LogicDevices.ANDV2") { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Лог. И")]; }
            if (logicElementType == "Anfang.LogicDevices.ORV2") { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Лог. ИЛИ")]; }
            if (logicElementType == "Anfang.LogicDevices.InvertV2") { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Лог. Инверсия")]; }
            if (logicElementType == "Anfang.LogicDevices.ComparatorV2") { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Компаратор")]; }
            if (logicElementType == "Anfang.LogicDevices.PhasorV2") { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Сравнение фаз")]; }
            if (logicElementType == "Anfang.LogicDevices.TimerV2") { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Выдержка на сраб.")]; }
            if (logicElementType == "Anfang.LogicDevices.AnalogSignalV2" & logicElement.IsOutputOfFunction == true) { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Аналог. выход")]; }
            if (logicElementType == "Anfang.LogicDevices.DiscrSignalV2" & logicElement.IsOutputOfFunction == true) { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Дискрет. выход")]; }
            if (logicElementType == "Anfang.LogicDevices.AnalogSignalV2" & logicElement.IsOutputOfFunction == false) { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Аналог. вход")]; }
            if (logicElementType == "Anfang.LogicDevices.DiscrSignalV2" & logicElement.IsOutputOfFunction == false) { LogicOptionsList.SelectedItem = LogicOptionsCollection[LogicOptionsCollection.IndexOf("Дискрет. вход")]; }
        }

        private void LoadLinkOptions(ObservableCollection<BaseLogicV2> LogicDevices)
        {
            LinkOptionsCollection.Clear();
            ExtendedLabels.Clear();
            foreach (var logicDevice in LogicDevices)
            {
                logicDevice.BuildExtendedLabels();
                LinkOptionsCollection.Add(logicDevice.ExtendedLabel);
                ExtendedLabels.Add(new ExtendedLabel() { Label = logicDevice.Label, LinkLabel = logicDevice.ExtendedLabel });
            }
        }
        private void LinkOptionsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LinkOptionsList.SelectedItem != null)
            {
                SelectedLinksCollection.Add(LinkOptionsList.SelectedItem as string);
            }
        }

        private void SelectedLinksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedLinksList.SelectedItem != null)
            {
                SelectedLinksCollection.Remove((string)SelectedLinksList.SelectedItem);
            }
        }

        private void LogicOptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LogicOptionsList.SelectedItem != null)
            {
                if (LogicOptionsList.SelectedItem.ToString() == "Компаратор")
                {
                    TripLevelBox.IsEnabled = true;
                }
                else
                {
                    TripLevelBox.IsEnabled = false;
                }
                if (LogicOptionsList.SelectedItem.ToString() == "Выдержка на сраб.")
                {
                    Delay1Box.IsEnabled = true;
                }
                else
                {
                    Delay1Box.IsEnabled = false;
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            CultureInfo culture = new CultureInfo("en-US");

            float triplevel = new float();
            float.TryParse((string)TripLevelBox.Text, NumberStyles.Float, culture, out triplevel);

            int delay1 = new int();
            int.TryParse((string)Delay1Box.Text, NumberStyles.Integer, culture, out delay1);

            int delay2 = new int();
            int.TryParse((string)Delay1Box.Text, NumberStyles.Integer, culture, out delay2);

            if (!IsValid(this)) return;
            else
            {
                if (LogicOptionsList.SelectedItem != null)
                {
                    if (LogicOptionsList.SelectedItem.ToString() == "Лог. И")
                    {
                        LogicElement = new ANDV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Лог. ИЛИ")
                    {
                        LogicElement = new ORV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Лог. Инверсия")
                    {
                        LogicElement = new InvertV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Компаратор")
                    {
                        LogicElement = new ComparatorV2() { tripLevel = triplevel };
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Сравнение фаз")
                    {
                        LogicElement = new PhasorV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Выдержка на сраб.")
                    {
                        LogicElement = new TimerV2() { TimerRiseDelay = delay1 };
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Аналог. выход")
                    {
                        LogicElement = new AnalogSignalV2();
                        LogicElement.IsOutputOfFunction = true;
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Дискрет. выход")
                    {
                        LogicElement = new DiscrSignalV2();
                        LogicElement.IsOutputOfFunction = true;
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Аналог. вход")
                    {
                        LogicElement = new AnalogSignalV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Дискрет. вход")
                    {
                        LogicElement = new DiscrSignalV2();
                    }
                    LogicElement.Label = LabelBox.Text;
                    foreach (var selectedLink in SelectedLinksCollection)
                    {
                        ExtendedLabel link = ExtendedLabels.Find(x => x.LinkLabel == selectedLink);
                        if (link != null)
                        {
                            LogicElement.InputLinks.Add(ExtendedLabels.Find(x => x.LinkLabel == selectedLink).Label);
                        }
                    }
                    DialogResult = true;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        // The following is stolen verbatim from a microsoft tutorial page on dialog windows hehehe
        // Validate all dependency objects in a window 
        bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached
                bool isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    // If a child dependency object is invalid, return false immediately,
                    // otherwise keep checking
                    if (IsValid((DependencyObject)subnode) == false) return false;
                }
            }

            // All dependency objects are valid
            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string logicElementType = LogicElement.GetType().ToString();
            if (logicElementType == "Anfang.LogicDevices.TimerV2")
            {
                TimerV2 timerV2 = LogicElement as TimerV2;
                Delay1Box.Text = timerV2.TimerRiseDelay.ToString();
            }
            if (logicElementType == "Anfang.LogicDevices.ComparatorV2")
            {
                ComparatorV2 comparatorV2 = LogicElement as ComparatorV2;
                TripLevelBox.Text = comparatorV2.tripLevel.ToString();
            }
        }
    }
}
