using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Anfang.Powersystem
{
    public class PowSysElementBase : INotifyPropertyChanged
    {
        public int id { get; set; } // needed to get unique numbers for "hidden" nodes, has to be unique for each item
        public int elnode1 { get; set; }
        public int elnode2 { get; set; }
        public string type { get; set; } // GEN - generator, TRAN - transformer, LINE - line, LOAD - load, AN(etc) - short-circuit, BRKR - breaker
        public float property1 // S_gen / S_trf / L_line
        {
            get
            {
                return property1_;
            }
            set
            {
                if (type == "BRKR" & value != 0 & value != 1)
                {
                    property1_ = 0;
                }
                if (value != property1_ & type == "BRKR")
                {
                    property1_ = value;
                    UpdateModelOnTrip();
                }
                if (type != "BRKR")
                {
                    property1_ = value;
                }
            }
        }
        private float property1_;
        public float property2  // x'' gen / Uk / x1
        {
            get
            {
                return property2_;
            }
            set
            {
                if (type == "BRKR" & value != 0 & value != 1)
                {
                    property2_ = 0;
                }
                if (value != property2_ & type == "BRKR")
                {
                    property2_ = value;
                    UpdateModelOnTrip();
                }
                if (type != "BRKR")
                {
                    property2_ = value;
                }
            }
        }
        private float property2_;
        public float property3  // Egen / Pk
        {
            get
            {
                return property3_;
            }
            set
            {
                if (type == "BRKR" & value != 0 & value != 1)
                {
                    property3_ = 0;
                }
                if (value != property3_ & type == "BRKR")
                {
                    property3_ = value;
                    UpdateModelOnTrip();
                }
                if (type != "BRKR")
                {
                    property3_ = value;
                }
            }
        }
        private float property3_;
        public float property4 { get; set; } //  / Px
        public float property5 { get; set; } //  / ixx
        public float property6 { get; set; }
        public bool grounded { get; set; }
        public float ground_act { get; set; }
        public float ground_react { get; set; }
        public float voltage_side1 { get; set; }
        public float voltage_side2 { get; set; }
        public List<float> properties;
        public int elnode3;
        public int elnode4;
        public int elnode1_counter = 0;
        public int elnode2_counter = 0;
        public int elnode3_counter = 0;
        public int elnode4_counter = 0;
        public int hidden_node_counter = 0;
        public List<int> elnodes;
        public CustomObservable model = new CustomObservable();
        public string currents_side1_string { get; set; }
        public List<Complex32> currents_side1
        { 
            get
            {
                return currents_side1_;
            }
            set
            {
                if (value != currents_side1_)
                {
                    currents_side1_ = value;
                    UpdateResults();
                }
            }
        }
        private List<Complex32> currents_side1_;

        public List<Complex32> currents_side2
        {
            get
            {
                return currents_side2_;
            }
            set
            {
                if (value != currents_side2_)
                {
                    currents_side2_ = value;
                    UpdateResults();
                }
            }
        }
        private List<Complex32> currents_side2_;

        public void BuildModel()
        {
            currents_side1 = new List<Complex32>();
            model.Clear();
            if (type == "GEN")
            {
                float ohms_react = voltage_side2 * voltage_side2 / property1 * property2;
                ResetCounters();
                model.Add(new Branch()
                {
                    Node1 = GetNewHiddenNode(),
                    Node2 = GetNewModelNode(elnode2),
                    E_Act = Complex32.FromPolarCoordinates((float)property3, 0).Real,
                    E_React = Complex32.FromPolarCoordinates((float)property3, 0).Imaginary,
                    Ohms_React = ohms_react,
                    id = id
                });
                model.Add(new Branch()
                {
                    Node2 = GetNewModelNode(elnode2),
                    Node1 = model[0].Node1,
                    E_Act = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 240).Real,
                    E_React = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 240).Imaginary,
                    Ohms_React = ohms_react,
                    id = id
                });
                model.Add(new Branch()
                {
                    Node2 = GetNewModelNode(elnode2),
                    Node1 = model[0].Node1,
                    E_Act = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 120).Real,
                    E_React = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 120).Imaginary,
                    Ohms_React = ohms_react,
                    id = id
                });
                if (grounded)
                {
                    model.Add(new Branch()
                    {
                        Node1 = model[0].Node1,
                        Node2 = 0,
                        Ohms_Act = ground_act,
                        Ohms_React = ground_react,
                        id = id
                    });
                    model.Add(new Branch()
                    {
                        Node1 = GetNewModelNode(elnode2),
                        Node2 = 0,
                        Ohms_Act = 0,
                        Ohms_React = 0,
                        id = id
                    });
                }
                else
                {
                    model.Add(new Branch()
                    {
                        Node1 = model[0].Node1,
                        Node2 = 0,
                        Ohms_Act = 1000000000,
                        Ohms_React = 1000000000,
                        id = id
                    });
                    model.Add(new Branch()
                    {
                        Node1 = GetNewModelNode(elnode2),
                        Node2 = 0,
                        Ohms_Act = 1000000000,
                        Ohms_React = 1000000000,
                        id = id
                    });
                }
            }
            if (type == "TRAN")
            {
                currents_side1 = new List<Complex32>();
                currents_side2 = new List<Complex32>();
                ResetCounters();
                float ohms_react = property2 / 100 * voltage_side2 * voltage_side2 / property1 / 2;
                float ohms_act = property3 / property1 / property1 * voltage_side2 / 2;
                float ohms_magnet_act = voltage_side2 * voltage_side2 / property4;
                float ohms_magnet_react = 100 / property5 * voltage_side2 / property1;
                float sqrt3 = (float)Math.Sqrt(3);
                Complex32 ohms = new Complex32(ohms_act, ohms_react);
                Complex32 ohms_magnet = new Complex32(ohms_magnet_act, ohms_magnet_react);
                Complex32 ohms_6_9_12 = 3 * ohms * ohms_magnet / ((3 * ohms) + ohms_magnet);

                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = ohms_act, Ohms_React = ohms_react, id = id }); //1 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react, id = id }); //2 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react, id = id }); //3 (Y-side)
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react, id = id }); //4
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react, id = id }); //5
                model.Add(new Branch() { Node1 = model[3].Node2, Node2 = model[4].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary, id = id }); //6
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = model[4].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react, id = id }); //7
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react, id = id }); //8
                model.Add(new Branch() { Node1 = model[6].Node2, Node2 = model[7].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary, id = id }); //9
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[7].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react, id = id }); //10
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[3].Node2, Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react, id = id }); //11
                model.Add(new Branch() { Node1 = model[9].Node2, Node2 = model[10].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary, id = id }); //12
                if (grounded)
                {
                    model.Add(new Branch() { Node1 = model[0].Node2, Node2 = 0, Ohms_Act = ground_act, Ohms_React = ground_react, id = id }); //ground
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 0, Ohms_React = 0, id = id }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000, id = id }); //neutral wire - D-side
                }
                else
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000, id = id }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000, id = id }); //neutral wire - D-side
                }
            }
            if (type == "TRANM")
            {
                currents_side1 = new List<Complex32>();
                currents_side2 = new List<Complex32>();
                ResetCounters();
                float ohms_react = property2 / 100 * voltage_side2 * voltage_side2 / property1 / 2;
                float ohms_act = property3 / property1 / property1 * voltage_side2 / 2;
                float ohms_magnet_act = voltage_side2 * voltage_side2 / property4;
                float ohms_magnet_react = 100 / property5 * voltage_side2 / property1;
                float sqrt3 = (float)Math.Sqrt(3);
                Complex32 ohms = new Complex32(ohms_act, ohms_react);
                Complex32 ohms_magnet = new Complex32(ohms_magnet_act, ohms_magnet_react);
                Complex32 ohms_6_9_12 = 3 * ohms;

                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = ohms_act, Ohms_React = ohms_react, id = id }); //1 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react, id = id }); //2 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react, id = id }); //3 (Y-side)
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react, id = id }); //4
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react, id = id }); //5
                model.Add(new Branch() { Node1 = model[3].Node2, Node2 = model[4].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary, id = id }); //6
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = model[4].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react, id = id }); //7
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react, id = id }); //8
                model.Add(new Branch() { Node1 = model[6].Node2, Node2 = model[7].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary, id = id }); //9
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[7].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react, id = id }); //10
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[3].Node2, Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react, id = id }); //11
                model.Add(new Branch() { Node1 = model[9].Node2, Node2 = model[10].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary, id = id }); //12
                if (grounded)
                {
                    model.Add(new Branch() { Node1 = model[0].Node2, Node2 = 0, Ohms_Act = ground_act, Ohms_React = ground_react, id = id }); //ground
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 0, Ohms_React = 0, id = id }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000, id = id }); //neutral wire - D-side
                }
                else
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000, id = id }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000, id = id }); //neutral wire - D-side
                }
            }
            if (type == "ABCN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property3, Ohms_React = property4, id = id });
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property5, Ohms_React = property6, id = id });
            }
            if (type == "ABN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property3, Ohms_React = property4, id = id });
            }
            if (type == "BCN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property3, Ohms_React = property4, id = id });
            }
            if (type == "ACN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property3, Ohms_React = property4, id = id });
            }
            if (type == "AB")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = b, Ohms_Act = property1, Ohms_React = property2, id = id });
            }
            if (type == "BC")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = b, Node2 = c, Ohms_Act = property1, Ohms_React = property2, id = id });
            }
            if (type == "CA")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = c, Ohms_Act = property1, Ohms_React = property2, id = id });
            }
            if (type == "AN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
            }
            if (type == "BN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
            }
            if (type == "CN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
            }
            if (type == "CMEAS")
            {
                ResetCounters();
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = 1, Ohms_React = 1, id = id });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = 1, Ohms_React = 1, id = id });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = 1, Ohms_React = 1, id = id });
                model.Add(new Branch() { Node1 = model[0].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = -1, Ohms_React = -1, id = id });
                model.Add(new Branch() { Node1 = model[1].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = -1, Ohms_React = -1, id = id });
                model.Add(new Branch() { Node1 = model[2].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = -1, Ohms_React = -1, id = id });
            }
            if (type == "BRKR")
            {
                ResetCounters();
                if (property1 == 0)
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), IsBreaker = true, Enabled = false, id = id });
                }
                if (property1 == 1)
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), IsBreaker = true, Enabled = true, id = id });
                }
                if (property2 == 0)
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), IsBreaker = true, Enabled = false, id = id });
                }
                if (property2 == 1)
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), IsBreaker = true, Enabled = true, id = id });
                }
                if (property3 == 0)
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), IsBreaker = true, Enabled = false, id = id });
                }
                if (property3 == 1)
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), IsBreaker = true, Enabled = true, id = id });
                }
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = 0, Ohms_React = 0, id = id });
            }
        }

        public void UpdateResults()
        {
            if (model.Count() > 0)
            {
                if (type == "GEN")
                {
                    currents_side1.Add(model[0].Current);
                    currents_side1.Add(model[1].Current);
                    currents_side1.Add(model[2].Current);
                }
                if (type == "TRAN")
                {
                    // side 1 - Y-side
                    currents_side1.Add(model[0].Current + model[10].Current + model[9].Current);
                    currents_side1.Add(model[1].Current + model[4].Current + model[3].Current);
                    currents_side1.Add(model[2].Current + model[6].Current + model[7].Current);
                    // side 2 - D-side
                    currents_side2.Add(model[3].Current + model[10].Current - model[5].Current + model[11].Current);
                    currents_side2.Add(model[4].Current + model[6].Current + model[5].Current - model[8].Current);
                    currents_side2.Add(model[7].Current + model[9].Current + model[8].Current - model[11].Current);
                }
                if (type == "TRANM")
                {
                    // side 1 - Y-side
                    currents_side1.Add(model[0].Current);
                    currents_side1.Add(model[1].Current);
                    currents_side1.Add(model[2].Current);
                    // side 2 - D-side
                    currents_side2.Add(model[3].Current + model[10].Current + model[5].Current + model[11].Current);
                    currents_side2.Add(model[4].Current + model[6].Current + model[5].Current + model[8].Current);
                    currents_side2.Add(model[7].Current + model[9].Current);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler ResultsChanged;

        private void UpdateModelOnTrip([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void UpdateResults([CallerMemberName] String propertyName = "")
        {
            ResultsChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ResetCounters()
        {
            elnode1_counter = 0;
            elnode2_counter = 0;
            elnode3_counter = 0;
            elnode4_counter = 0;
            hidden_node_counter = 0;
        }

        public int GetNewHiddenNode()
        {
            if (hidden_node_counter == 0)
            {
                hidden_node_counter = 999999999 - (id * 1000);
            }
            hidden_node_counter--;
            return hidden_node_counter;
        }

        public int GetNewModelNode(int elnode)
        {
            if (elnode1 == elnode)
            {
                if (elnode1_counter == 0)
                {
                    elnode1_counter = elnode * 10000;
                }
                elnode1_counter++;
                return elnode1_counter;
            }
            if (elnode2 == elnode)
            {
                if (elnode2_counter == 0)
                {
                    elnode2_counter = elnode * 10000;
                }
                elnode2_counter++;
                return elnode2_counter;
            }
            if (elnode3 == elnode)
            {
                if (elnode3_counter == 0)
                {
                    elnode3_counter = elnode * 10000;
                }
                elnode3_counter++;
                return elnode3_counter;
            }
            if (elnode4 == elnode)
            {
                if (elnode4_counter == 0)
                {
                    elnode4_counter = elnode * 10000;
                }
                elnode4_counter++;
                return elnode4_counter;
            }
            return 0;
        }

        public string GetUid()
        {
            string Uid = type + id;
            if (grounded)
            {
                Uid += "g";
            }
            return Uid;
        }

    }
}
