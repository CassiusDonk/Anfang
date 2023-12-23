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
        private static readonly ObservableCollection<string> LogicOptionsCollection = new ObservableCollection<string>();
        private static readonly ObservableCollection<string> LinkOptionsCollection = new ObservableCollection<string>();
        private static readonly ObservableCollection<string> SelectedLinksCollection = new ObservableCollection<string>();

        private class ExtendedLabel
        {
           public string Label = "";
           public string LinkLabel = "";
        }

        private List<ExtendedLabel> ExtendedLabels = new List<ExtendedLabel>();

        public BaseLogicV2 LogicDevice = new BaseLogicV2();
        public NewLogicElementDialog()
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
        }

        public void LoadLinkOptions(ObservableCollection<BaseLogicV2> LogicDevices)
        {
            LinkOptionsCollection.Clear();
            ExtendedLabels.Clear();
            foreach (var logicDevice in LogicDevices)
            {
                string type = "Undefined";
                if (logicDevice.OutputBool == true) { type = "Дискрет"; }
                else { type = "Числовой"; }
                string LinkLabel = $"{logicDevice.Label}, ({type})";
                LinkOptionsCollection.Add(LinkLabel);
                ExtendedLabels.Add(new ExtendedLabel() { Label = logicDevice.Label, LinkLabel = LinkLabel });
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
                        LogicDevice = new ANDV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Лог. ИЛИ")
                    {
                        LogicDevice = new ORV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Лог. Инверсия")
                    {
                        LogicDevice = new InvertV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Компаратор")
                    {
                        LogicDevice = new ComparatorV2() { tripLevel = triplevel };
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Сравнение фаз")
                    {
                        LogicDevice = new PhasorV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Выдержка на сраб.")
                    {
                        LogicDevice = new TimerV2() { TimerRiseDelay = delay1 };
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Аналог. выход")
                    {
                        LogicDevice = new AnalogSignalV2();
                        LogicDevice.IsOutputOfFunction = true;
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Дискрет. выход")
                    {
                        LogicDevice = new DiscrSignalV2();
                        LogicDevice.IsOutputOfFunction = true;
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Аналог. вход")
                    {
                        LogicDevice = new AnalogSignalV2();
                    }
                    if (LogicOptionsList.SelectedItem.ToString() == "Дискрет. вход")
                    {
                        LogicDevice = new DiscrSignalV2();
                    }
                    LogicDevice.Label = LabelBox.Text;
                    foreach (var ExtendedLabel in ExtendedLabels)
                    {
                        if (SelectedLinksCollection.Contains(ExtendedLabel.LinkLabel))
                        {
                            LogicDevice.InputLinks.Add(ExtendedLabel.Label);
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
    }
}
