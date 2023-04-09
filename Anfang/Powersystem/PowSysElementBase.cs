using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace Anfang.Powersystem
{
    public class PowSysElementBase
    {
        public int id { get; set; } // needed to get unique numbers for "hidden" nodes, has to be unique for each item
        public int elnode1 { get; set; }
        public int elnode2 { get; set; }
        public string type { get; set; } // GEN - generator, TRAN - transformer, LINE - line, LOAD - load, SHORT - short-circuit, BRK - breaker
        public float property1 { get; set; } // S_gen / S_trf / L_line
        public float property2 { get; set; } // x'' gen / Uk / x1
        public float property3 { get; set; } // Egen / Pk
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

        public void BuildModel()
        {
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
                    Ohms_React = ohms_react
                });
                model.Add(new Branch()
                {
                    Node2 = GetNewModelNode(elnode2),
                    Node1 = model[0].Node1,
                    E_Act = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 240).Real,
                    E_React = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 240).Imaginary,
                    Ohms_React = ohms_react
                });
                model.Add(new Branch()
                {
                    Node2 = GetNewModelNode(elnode2),
                    Node1 = model[0].Node1,
                    E_Act = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 120).Real,
                    E_React = Complex32.FromPolarCoordinates((float)property3, (float)Math.PI / 180 * 120).Imaginary,
                    Ohms_React = ohms_react
                });
                if (grounded)
                {
                    model.Add(new Branch()
                    {
                        Node1 = model[0].Node1,
                        Node2 = 0,
                        Ohms_Act = ground_act,
                        Ohms_React = ground_react
                    });
                    model.Add(new Branch()
                    {
                        Node1 = GetNewModelNode(elnode2),
                        Node2 = 0,
                        Ohms_Act = 0,
                        Ohms_React = 0
                    });
                }
                else
                {
                    model.Add(new Branch()
                    {
                        Node1 = model[0].Node1,
                        Node2 = 0,
                        Ohms_Act = 1000000000,
                        Ohms_React = 1000000000
                    });
                    model.Add(new Branch()
                    {
                        Node1 = GetNewModelNode(elnode2),
                        Node2 = 0,
                        Ohms_Act = 1000000000,
                        Ohms_React = 1000000000
                    });
                }
            }
            if (type == "TRAN")
            {
                ResetCounters();
                float ohms_react = property2 / 100 * voltage_side2 * voltage_side2 / property1 / 2;
                float ohms_act = property3 / property1 / property1 * voltage_side2 / 2;
                float ohms_magnet_act = voltage_side2 * voltage_side2 / property4;
                float ohms_magnet_react = 100 / property5 * voltage_side2 / property1;
                float sqrt3 = (float)Math.Sqrt(3);
                Complex32 ohms = new Complex32(ohms_act, ohms_react);
                Complex32 ohms_magnet = new Complex32(ohms_magnet_act, ohms_magnet_react);
                Complex32 ohms_6_9_12 = 3 * ohms * ohms_magnet / ((3 * ohms) + ohms_magnet);

                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = ohms_act, Ohms_React = ohms_react }); //1 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react }); //2 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react }); //3 (Y-side)
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react }); //4
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react }); //5
                model.Add(new Branch() { Node1 = model[3].Node2, Node2 = model[4].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary }); //6
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = model[4].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react }); //7
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react }); //8
                model.Add(new Branch() { Node1 = model[6].Node2, Node2 = model[7].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary }); //9
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[7].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react }); //10
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[3].Node2, Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react }); //11
                model.Add(new Branch() { Node1 = model[9].Node2, Node2 = model[10].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary }); //12
                if (grounded)
                {
                    model.Add(new Branch() { Node1 = model[0].Node2, Node2 = 0, Ohms_Act = ground_act, Ohms_React = ground_react }); //ground
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 0, Ohms_React = 0 }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000 }); //neutral wire - D-side
                }
                else
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000 }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000 }); //neutral wire - D-side
                }
            }
            if (type == "TRANM")
            {
                ResetCounters();
                float ohms_react = property2 / 100 * voltage_side2 * voltage_side2 / property1 / 2;
                float ohms_act = property3 / property1 / property1 * voltage_side2 / 2;
                float ohms_magnet_act = voltage_side2 * voltage_side2 / property4;
                float ohms_magnet_react = 100 / property5 * voltage_side2 / property1;
                float sqrt3 = (float)Math.Sqrt(3);
                Complex32 ohms = new Complex32(ohms_act, ohms_react);
                Complex32 ohms_magnet = new Complex32(ohms_magnet_act, ohms_magnet_react);
                Complex32 ohms_6_9_12 = 3 * ohms;

                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = ohms_act, Ohms_React = ohms_react }); //1 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react }); //2 (Y-side)
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = model[0].Node2, Ohms_Act = ohms_act, Ohms_React = ohms_react }); //3 (Y-side)
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react }); //4
                model.Add(new Branch() { Node1 = model[1].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react }); //5
                model.Add(new Branch() { Node1 = model[3].Node2, Node2 = model[4].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary }); //6
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = model[4].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react }); //7
                model.Add(new Branch() { Node1 = model[2].Node1, Node2 = GetNewModelNode(elnode2), Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react }); //8
                model.Add(new Branch() { Node1 = model[6].Node2, Node2 = model[7].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary }); //9
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[7].Node2, Ohms_Act = -sqrt3 * ohms_act, Ohms_React = -sqrt3 * ohms_react }); //10
                model.Add(new Branch() { Node1 = model[0].Node1, Node2 = model[3].Node2, Ohms_Act = sqrt3 * ohms_act, Ohms_React = sqrt3 * ohms_react }); //11
                model.Add(new Branch() { Node1 = model[9].Node2, Node2 = model[10].Node2, Ohms_Act = ohms_6_9_12.Real, Ohms_React = ohms_6_9_12.Imaginary }); //12
                if (grounded)
                {
                    model.Add(new Branch() { Node1 = model[0].Node2, Node2 = 0, Ohms_Act = ground_act, Ohms_React = ground_react }); //ground
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 0, Ohms_React = 0 }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000 }); //neutral wire - D-side
                }
                else
                {
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000 }); //neutral wire - Y-side
                    model.Add(new Branch() { Node1 = GetNewModelNode(elnode2), Node2 = 0, Ohms_Act = 1000000000, Ohms_React = 1000000000 }); //neutral wire - D-side
                }
            }
            if (type == "ABCN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2 });
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property3, Ohms_React = property4 });
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property5, Ohms_React = property6 });
            }
            if (type == "ABN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2 });
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property3, Ohms_React = property4 });
            }
            if (type == "BCN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property1, Ohms_React = property2 });
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property3, Ohms_React = property4 });
            }
            if (type == "ACN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2 });
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property3, Ohms_React = property4 });
            }
            if (type == "AB")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = b, Ohms_Act = property1, Ohms_React = property2 });
            }
            if (type == "BC")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = b, Node2 = c, Ohms_Act = property1, Ohms_React = property2 });
            }
            if (type == "CA")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = c, Ohms_Act = property1, Ohms_React = property2 });
            }
            if (type == "AN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = a, Node2 = n, Ohms_Act = property1, Ohms_React = property2 });
            }
            if (type == "BN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = b, Node2 = n, Ohms_Act = property1, Ohms_React = property2 });
            }
            if (type == "CN")
            {
                ResetCounters();
                int a = GetNewModelNode(elnode1);
                int b = GetNewModelNode(elnode1);
                int c = GetNewModelNode(elnode1);
                int n = GetNewModelNode(elnode1);
                model.Add(new Branch() { Node1 = c, Node2 = n, Ohms_Act = property1, Ohms_React = property2 });
            }
            if (type == "CMEAS")
            {
                ResetCounters();
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = 1, Ohms_React = 1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = 1, Ohms_React = 1 });
                model.Add(new Branch() { Node1 = GetNewModelNode(elnode1), Node2 = GetNewHiddenNode(), Ohms_Act = 1, Ohms_React = 1 });
                model.Add(new Branch() { Node1 = model[0].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = -1, Ohms_React = -1 });
                model.Add(new Branch() { Node1 = model[1].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = -1, Ohms_React = -1 });
                model.Add(new Branch() { Node1 = model[2].Node2, Node2 = GetNewModelNode(elnode2), Ohms_Act = -1, Ohms_React = -1 });
            }
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
