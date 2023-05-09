using MathNet.Numerics;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Anfang.LogicDevices
{
    public class BaseLogic : INotifyPropertyChanged
    {
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
        public int delay = 0;
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

        public float triplevel = new float();

        public int internal_time = 0;
        public int sim_time_step = 0;

        public bool output = false;
        public Complex32 output_complex = new Complex32();

        public string label = "";

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
    }
}
