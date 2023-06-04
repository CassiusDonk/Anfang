using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
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
        public float property2  // x'' gen / Uk / x0_line
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
        public float property3  // Egen / Pk / r0_line
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
        public float property4 { get; set; } // x2 gen / Px / b0_line
        public float property5 { get; set; } // x0 gen / ixx
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
        public List<Complex32> currents_side1 = new List<Complex32>();

        public List<Complex32> currents_side2 = new List<Complex32>();

        public List<Complex32> voltages_side1 = new List<Complex32>();
        public List<Complex32> voltages_side2 = new List<Complex32>();

        public void BuildModel()
        {
            Complex32 currentA = new Complex32();
            Complex32 currentB = new Complex32();
            Complex32 currentC = new Complex32();
            if (currents_side1 != null)
            {
                if (currents_side1.Count > 0)
                {
                    currentA = currents_side1[0];
                    currentB = currents_side1[1];
                    currentC = currents_side1[2];
                }
            }
            currents_side1 = new List<Complex32>();
            model.Clear();
            if (type == "GEN")
            {
                float ohms_react = property3 * voltage_side1 * voltage_side1 / (3 * property1) * property2;
                float emf = property3 * voltage_side1 / MathF.Sqrt(3);

                float ohms0_react = voltage_side1 * voltage_side1 / (3 * property1) * property5;
                if (property5 == 0)
                {
                    ohms0_react = ohms_react;
                }

                float ohms2_react = voltage_side1 * voltage_side1 / (3 * property1) * property4;
                if (property4 == 0)
                {
                    ohms2_react = ohms_react;
                }

                float ohms_phase = (ohms_react + ohms0_react + ohms2_react) / 3;
                Complex32 ohms_m = (ohms_react * Complex32.FromPolarCoordinates(1, 180 * 120) + ohms2_react * Complex32.FromPolarCoordinates(1, 180 * 240) + ohms0_react) / 3;
                Complex32 ohms_phase_ = new Complex32(0, ohms_phase) - ohms_m;

                float delta_ohms = MathF.Sqrt(3) * (ohms_react - ohms2_react);

                Complex32 depvoltA = -new Complex32(0, 1) * delta_ohms / 3 * currentA;
                Complex32 depvoltB = -new Complex32(0, 1) * delta_ohms / 3 * currentB;
                Complex32 depvoltC = -new Complex32(0, 1) * delta_ohms / 3 * currentC;


                ResetCounters();
                model.Add(new Branch()
                {
                    Node1 = GetNewHiddenNode(),
                    Node2 = GetNewModelNode(elnode2),
                    E_Act = Complex32.FromPolarCoordinates((float)emf, 0).Real - depvoltA.Real,
                    E_React = Complex32.FromPolarCoordinates((float)emf, 0).Imaginary - depvoltA.Imaginary,
                    Ohms_React = ohms_phase_.Imaginary,
                    Ohms_Act = ohms_phase_.Real,
                    id = id
                });
                model.Add(new Branch()
                {
                    Node2 = GetNewModelNode(elnode2),
                    Node1 = model[0].Node1,
                    E_Act = Complex32.FromPolarCoordinates((float)emf, (float)Math.PI / 180 * 240).Real - depvoltB.Real,
                    E_React = Complex32.FromPolarCoordinates((float)emf, (float)Math.PI / 180 * 240).Imaginary - depvoltB.Imaginary,
                    Ohms_React = ohms_phase_.Imaginary,
                    Ohms_Act = ohms_phase_.Real,
                    id = id
                });
                model.Add(new Branch()
                {
                    Node2 = GetNewModelNode(elnode2),
                    Node1 = model[0].Node1,
                    E_Act = Complex32.FromPolarCoordinates((float)emf, (float)Math.PI / 180 * 120).Real - depvoltC.Real,
                    E_React = Complex32.FromPolarCoordinates((float)emf, (float)Math.PI / 180 * 120).Imaginary - depvoltC.Imaginary,
                    Ohms_React = ohms_phase_.Imaginary,
                    Ohms_Act = ohms_phase_.Real,
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

                float DegsToRads(float degs)
                {
                    float rads = MathF.PI * degs / 180;
                    return rads;
                }
            }
            if (type == "TRAN")
            {
                currents_side1 = new List<Complex32>();
                currents_side2 = new List<Complex32>();
                ResetCounters();
                float ohms_react = property2 / 100 * voltage_side2 * voltage_side2 / (property1 / 3) / 2;
                float ohms_act = property3 / (property1 / 3) / (property1 / 3) * voltage_side2 / 2;
                float ohms_magnet_act = voltage_side2 * voltage_side2 / property4;
                float ohms_magnet_react = 100 / property5 * voltage_side2 / (property1 / 3);
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
                float ohms_react = property2 / 100 * voltage_side2 * voltage_side2 / (property1 / 3) / 2;
                float ohms_act = property3 / (property1 / 3) / (property1 / 3) * voltage_side2 / 2;
                float ohms_magnet_act = voltage_side2 * voltage_side2 / property4;
                float ohms_magnet_react = 100 / property5 * voltage_side2 / (property1 / 3);
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
            if (type == "ABC")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = b, Ohms_Act = property1, Ohms_React = property2, id = id });
                model.Add(new Branch() { Node1 = c, Node2 = b, Ohms_Act = property5, Ohms_React = property6, id = id });
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
            if (type == "ABN0")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = 0;
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2, id = id });
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property3, Ohms_React = property4, id = id });
            }
            if (type == "AB0")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = 0, Ohms_Act = property1, Ohms_React = property2, id = id });
                model.Add(new Branch() { Node1 = b, Node2 = 0, Ohms_Act = property3, Ohms_React = property4, id = id });
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
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = 0, Ohms_React = 0, id = id });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = 0, Ohms_React = 0, id = id });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = 0, Ohms_React = 0, id = id });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = 0, Ohms_React = 0, id = id });
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

            if (type == "LINE")
            {
                ResetCounters();

                float x_line = property2 * property1;
                float r_line = property3 * property1;
                Complex32 z_line = new Complex32(r_line, x_line);

                float r0_line = ground_act * property1;
                float x0_line = ground_react * property1;

                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line, Ohms_React = x_line });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line, Ohms_React = x_line });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line, Ohms_React = x_line });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewModelNode(elnode2), Ohms_Act = r0_line, Ohms_React = x0_line });

                if (property4 != 0 | property5 != 0)
                {
                    float b_line = property4 * property1 / 2 / 1000000;
                    float g_line = property5 * property1 / 2 / 1000000;
                    Complex32 y_line = 1 / new Complex32(g_line, b_line);

                    int ground_elnode1 = model[3].Node1;
                    int ground_elnode2 = model[3].Node2;

                    model.Add(new Branch() { Node1 = model[0].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[1].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[2].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });

                    model.Add(new Branch() { Node1 = model[0].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[1].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[2].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                }
            }
            if (type == "LINEAN")
            {
                ResetCounters();

                float x_line1 = property2 * property6;
                float r_line1= property3 * property6;
                Complex32 z_line1 = new Complex32(r_line1, x_line1);

                float x_line2 = property2 * (property1 - property6);
                float r_line2 = property3 * (property1 - property6);
                Complex32 z_line2 = new Complex32(r_line2, x_line2);

                float r0_line1 = ground_act * property6;
                float x0_line1 = ground_react * property6;

                float r0_line2 = ground_act * (property1 - property6);
                float x0_line2 = ground_react * (property1 - property6);

                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r_line1, Ohms_React = x_line1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r_line1, Ohms_React = x_line1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r_line1, Ohms_React = x_line1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r0_line1, Ohms_React = x0_line1 });

                model.Add(new Branch() { Node1 = model[0].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line2, Ohms_React = x_line2 });
                model.Add(new Branch() { Node1 = model[1].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line2, Ohms_React = x_line2 });
                model.Add(new Branch() { Node1 = model[2].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line2, Ohms_React = x_line2 });
                model.Add(new Branch() { Node1 = model[3].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r0_line2, Ohms_React = x0_line2 });

                model.Add(new Branch() { Node1 = model[0].Node2, Node2 = model[3].Node2, Ohms_Act = 0, Ohms_React = 0, id = id });

                if (property4 != 0 | property5 != 0)
                {
                    float b_line = property4 * property1 / 2 / 1000000;
                    float g_line = property5 * property1 / 2 / 1000000;
                    Complex32 y_line = 1 / new Complex32(g_line, b_line);

                    int ground_elnode1 = model[3].Node1;
                    int ground_elnode2 = model[3].Node2;

                    model.Add(new Branch() { Node1 = model[0].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[1].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[2].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });

                    model.Add(new Branch() { Node1 = model[0].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[1].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[2].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                }
            }
            if (type == "LINEABN")
            {
                ResetCounters();

                float x_line1 = property2 * property6;
                float r_line1 = property3 * property6;
                Complex32 z_line1 = new Complex32(r_line1, x_line1);

                float x_line2 = property2 * (property1 - property6);
                float r_line2 = property3 * (property1 - property6);
                Complex32 z_line2 = new Complex32(r_line2, x_line2);

                float r0_line1 = ground_act * property6;
                float x0_line1 = ground_react * property6;

                float r0_line2 = ground_act * (property1 - property6);
                float x0_line2 = ground_react * (property1 - property6);

                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r_line1, Ohms_React = x_line1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r_line1, Ohms_React = x_line1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r_line1, Ohms_React = x_line1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = r0_line1, Ohms_React = x0_line1 });

                model.Add(new Branch() { Node1 = model[0].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line2, Ohms_React = x_line2 });
                model.Add(new Branch() { Node1 = model[1].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line2, Ohms_React = x_line2 });
                model.Add(new Branch() { Node1 = model[2].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r_line2, Ohms_React = x_line2 });
                model.Add(new Branch() { Node1 = model[3].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = r0_line2, Ohms_React = x0_line2 });

                model.Add(new Branch() { Node1 = model[0].Node2, Node2 = model[3].Node2, Ohms_Act = 0, Ohms_React = 0, id = id });
                model.Add(new Branch() { Node1 = model[1].Node2, Node2 = model[3].Node2, Ohms_Act = 0, Ohms_React = 0, id = id });

                if (property4 != 0 | property5 != 0)
                {
                    float b_line = property4 * property1 / 2 / 1000000;
                    float g_line = property5 * property1 / 2 / 1000000;
                    Complex32 y_line = 1 / new Complex32(g_line, b_line);

                    int ground_elnode1 = model[3].Node1;
                    int ground_elnode2 = model[3].Node2;

                    model.Add(new Branch() { Node1 = model[0].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[1].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[2].Node1, Node2 = ground_elnode1, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });

                    model.Add(new Branch() { Node1 = model[0].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[1].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                    model.Add(new Branch() { Node1 = model[2].Node2, Node2 = ground_elnode2, Ohms_Act = y_line.Real, Ohms_React = y_line.Imaginary });
                }
            }

        }

        public void UpdateResults()
        {
            currents_side1.Clear();
            currents_side1.TrimExcess();
            voltages_side1.Clear();
            voltages_side1.TrimExcess();
            if (voltages_side2 != null)
            {
                voltages_side2.Clear();
                voltages_side2.TrimExcess();
            }
            if (currents_side2 != null)
            {
                currents_side2.Clear();
                currents_side2.TrimExcess();
            }
            if (model.Count() > 0)
            {
                if (type == "GEN")
                {
                    currents_side1.Add(model[0].Current);
                    currents_side1.Add(model[1].Current);
                    currents_side1.Add(model[2].Current);
                    currents_side1.Add(currents_side1[0] + currents_side1[1] + currents_side1[2]);

                    voltages_side1.Add(model[0].Voltage_Node2);
                    voltages_side1.Add(model[1].Voltage_Node2);
                    voltages_side1.Add(model[2].Voltage_Node2);
                    voltages_side1.Add(voltages_side1[0] + voltages_side1[1] + voltages_side1[2]);
                }
                if (type == "TRAN" | type == "TRANM")
                {
                    // side 1 - Y-side
                    currents_side1.Add(model[0].Current + model[10].Current + model[9].Current);
                    currents_side1.Add(model[1].Current + model[4].Current + model[3].Current);
                    currents_side1.Add(model[2].Current + model[6].Current + model[7].Current);
                    currents_side1.Add(currents_side1[0] + currents_side1[1] + currents_side1[2]);

                    voltages_side1.Add(model[0].Voltage_Node1);
                    voltages_side1.Add(model[1].Voltage_Node1);
                    voltages_side1.Add(model[2].Voltage_Node1);
                    voltages_side1.Add(voltages_side1[0] + voltages_side1[1] + voltages_side1[2]);

                    // side 2 - D-side
                    currents_side2.Add(model[3].Current + model[10].Current - model[5].Current + model[11].Current);
                    currents_side2.Add(model[4].Current + model[6].Current + model[5].Current - model[8].Current);
                    currents_side2.Add(model[7].Current + model[9].Current + model[8].Current - model[11].Current);
                    currents_side2.Add(currents_side2[0] + currents_side2[1] + currents_side2[2]);

                    voltages_side2.Add(model[3].Voltage_Node2);
                    voltages_side2.Add(model[6].Voltage_Node2);
                    voltages_side2.Add(model[9].Voltage_Node2);
                    voltages_side2.Add(voltages_side2[0] + voltages_side2[1] + voltages_side2[2]);
                }
                if (type == "LINE")
                {
                    currents_side1.Add(model[0].Current);
                    currents_side1.Add(model[1].Current);
                    currents_side1.Add(model[2].Current);
                    currents_side1.Add(currents_side1[0] + currents_side1[1] + currents_side1[2]);

                    currents_side2.Add(model[0].Current);
                    currents_side2.Add(model[1].Current);
                    currents_side2.Add(model[2].Current);
                    currents_side2.Add(currents_side1[0] + currents_side1[1] + currents_side1[2]);

                    voltages_side1.Add(model[0].Voltage_Node1);
                    voltages_side1.Add(model[1].Voltage_Node1);
                    voltages_side1.Add(model[2].Voltage_Node1);
                    voltages_side1.Add(voltages_side1[0] + voltages_side1[1] + voltages_side1[2]);

                    voltages_side2.Add(model[0].Voltage_Node2);
                    voltages_side2.Add(model[1].Voltage_Node2);
                    voltages_side2.Add(model[2].Voltage_Node2);
                    voltages_side2.Add(voltages_side2[0] + voltages_side2[1] + voltages_side2[2]);
                }
                if (type == "LINEAN" | type == "LINEABN")
                {
                    currents_side1.Add(model[0].Current);
                    currents_side1.Add(model[1].Current);
                    currents_side1.Add(model[2].Current);
                    currents_side1.Add(currents_side1[0] + currents_side1[1] + currents_side1[2]);

                    voltages_side1.Add(model[0].Voltage_Node1);
                    voltages_side1.Add(model[1].Voltage_Node1);
                    voltages_side1.Add(model[2].Voltage_Node1);
                    voltages_side1.Add(voltages_side1[0] + voltages_side1[1] + voltages_side1[2]);

                    currents_side2.Add(model[4].Current);
                    currents_side2.Add(model[5].Current);
                    currents_side2.Add(model[6].Current);
                    currents_side2.Add(currents_side2[0] + currents_side2[1] + currents_side2[2]);

                    voltages_side2.Add(model[4].Voltage_Node2);
                    voltages_side2.Add(model[5].Voltage_Node2);
                    voltages_side2.Add(model[6].Voltage_Node2);
                    voltages_side2.Add(voltages_side2[0] + voltages_side2[1] + voltages_side2[2]);
                }
                for (int i = 0; i < currents_side1.Count() - 1; i++)
                { // set values below a certain threshold to zero
                    if (currents_side1[i].Magnitude < 0.001)
                    {
                        currents_side1[i] = new Complex32(0, 0);
                    }
                }
                if (currents_side2 != null)
                { // see above
                    for (int i = 0; i < currents_side2.Count() - 1; i++)
                    {
                        if (currents_side2[i].Magnitude < 0.001)
                        {
                            currents_side2[i] = new Complex32(0, 0);
                        }
                    }
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
            return Uid;
        }

        public string GetLongUid()
        { // "LongUid" stores additional information used by GraphOps to determine how to draw the element, but this "LongUid" is not used to determine whether the element is drawn.
            string Uid = type + id;
            if (grounded)
            {
                Uid += "g";
            }
            if (type == "BRKR")
            {
                if (property1 == 1 & property2 == 1 & property3 == 1)
                {
                    Uid += "e";
                }
            }
            return Uid;
        }

    }
}
