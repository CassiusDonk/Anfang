using MathNet.Numerics;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Anfang.LogicDevices
{
    public class BaseLogic : INotifyPropertyChanged
    {
        public string inputType = "";
        public string outputType = "";
        public Complex32 input_complex_old = new Complex32();

        public Complex32 input_complex
        {
            get
            {
                return input_complex_old;
            }
            set
            {
                if (value != this.input_complex_old)
                {
                    this.input_complex_old = value;
                    NotifyInputComplexChanged();
                }
            }
        }

        public Complex32 input_complex2_old = new Complex32();
        public Complex32 input_complex2
        {
            get
            {
                return input_complex2_old;
            }
            set
            {
                if (value != this.input_complex2_old)
                {
                    this.input_complex2_old = value;
                    NotifyInputComplexChanged();
                }
            }
        }

        public ObservableCollection<Complex32> input_complex_list_old = new ObservableCollection<Complex32>();
        public ObservableCollection<Complex32> input_complex_list
        {
            get
            {
                return input_complex_list_old;
            }
            set
            {
                if (value != this.input_complex_list_old)
                {
                    this.input_complex_list_old = value;
                    NotifyInputComplexChanged();
                }
            }
        }

        public bool input_bool_old = false;
        public bool input_bool
        {
            get
            {
                return input_bool_old;
            }
            set
            {
                if (value != this.input_bool_old)
                {
                    this.input_bool_old = value;
                    NotifyInputBoolChanged();
                }
            }
        }

        public ObservableCollection<bool> input_bool_list = new ObservableCollection<bool>();

        public bool init = false;
        public int delay { get; set; }
        public int sim_time_old = 0;
        public int sim_time
        {
            get
            {
                return sim_time_old;
            }
            set
            {
                if (value != this.sim_time_old)
                {
                    this.sim_time_old = value;
                    NotifySimTimeChanged();
                }
            }
        }

        public float triplevel { get; set; }

        public int internal_time = 0;
        public int sim_time_step = 0;

        public bool output = false;
        public Complex32 output_complex = new Complex32();

        public string label { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseLogic()
        {
            input_bool_list.CollectionChanged += NotifyInputBoolListChanged;
        }

        public virtual void NotifyInputBoolListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        public virtual void NotifyInputComplexChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.

        }

        public virtual void NotifyInputBoolChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.

        }

        public virtual void NotifySimTimeChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on sim time change.

        }

        public virtual void UpdateOutput()
        {

        }
    }
}
