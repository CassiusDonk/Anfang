using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Collections.ObjectModel;
using Anfang.Powersystem;
using System.Windows.Controls;

namespace Anfang
{
    class FileInteractions
    {
        CultureInfo format = new CultureInfo("en-US");
        public void SaveData(List<string> Data, string path)
        {
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (string line in Data)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            else
            {
                File.Delete(path);
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (string line in Data)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
        }

        public List<string> LoadData(string path)
        {
            List<string> data = new List<string>();

            using (StreamReader sr = File.OpenText(path))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    data.Add(s);
                }
            }
            return data;
        }

        public List<string> PowersystemToString(ObservableCollection<PowSysElementBase> powersys_collection)
        {
            List<string> data = new List<string>();
            foreach (PowSysElementBase element in powersys_collection)
            {
                data.Add(BuildString(element));
            }
            return data;

            string BuildString(PowSysElementBase element)
            {
                CultureInfo format = new CultureInfo("en-US");
                string element_string =
                    element.id.ToString(format) + "," +
                    element.elnode1.ToString(format) + "," +
                    element.elnode2.ToString(format) + "," +
                    element.type.ToString(format) + "," +
                    element.property1.ToString(format) + "," +
                    element.property2.ToString(format) + "," +
                    element.property3.ToString(format) + "," +
                    element.property4.ToString(format) + "," +
                    element.property5.ToString(format) + "," +
                    element.property6.ToString(format) + "," +
                    element.grounded.ToString(format) + "," +
                    element.ground_act.ToString(format) + "," +
                    element.ground_react.ToString(format) + "," +
                    element.voltage_side1.ToString(format) + "," +
                    element.voltage_side2.ToString(format);
                return element_string;
            }
        }

        public void ReconstructPowersystem(List<string> data, ObservableCollection<PowSysElementBase> powersys_collection)
        {
            powersys_collection.Clear();

            foreach (string line in data)
            {
                List<string> split_params = SplitParams(line);
                bool grounded = false;
                if (split_params[10] == "True")
                {
                    grounded = true;
                }

                powersys_collection.Add(new PowSysElementBase()
                {
                    id = TryToParseInt(split_params[0]),
                    elnode1 = TryToParseInt(split_params[1]),
                    elnode2 = TryToParseInt(split_params[2]),
                    type = split_params[3],
                    property1 = TryToParseInt(split_params[4]),
                    property2 = TryToParseFloat(split_params[5]),
                    property3 = TryToParseFloat(split_params[6]),
                    property4 = TryToParseFloat(split_params[7]),
                    property5 = TryToParseFloat(split_params[8]),
                    property6 = TryToParseFloat(split_params[9]),
                    grounded = grounded,
                    ground_act = TryToParseFloat(split_params[11]),
                    ground_react = TryToParseFloat(split_params[12]),
                    voltage_side1 = TryToParseInt(split_params[13]),
                    voltage_side2 = TryToParseFloat(split_params[14])
                });
            }
        }

        public List<string> ProtectionsToString(ObservableCollection<Protection> protectiongrid_collection)
        {
            List<string> data = new List<string>();
            foreach (Protection prot in protectiongrid_collection)
            {
                string header = prot.label + "," + prot.init_label + "," + prot.trip_label;
                string logic = StringListToString(prot.logic_config);
                string triplevels = FloatListToString(prot.tripLevels);
                string delays = IntListToString(prot.timer_delays);
                string breakers = IntListToString(prot.breaker_numbers);
                string analogs_start = "ANALOGS";
                string analogs_end = "ANALOGS_END";
                string separator = "END";

                data.Add(header);
                data.Add(logic);
                data.Add(triplevels);
                data.Add(delays);
                data.Add(breakers);
                data.Add(analogs_start);
                foreach (AnalogInputLink analogInputLink in prot.analogInputLinks)
                {
                    data.Add($"{analogInputLink.id},{analogInputLink.isVoltage},{analogInputLink.phase},{analogInputLink.side}");
                }
                data.Add(analogs_end);
                data.Add(separator);
            }
            return data;
        }

        public void ReconstructProtections(List<string> data, ObservableCollection<Protection> protectiongrid_collection)
        {

            List<List<string>> split_by_separator()
            {
                List<List<string>> result = new List<List<string>>();
                int i = 0;
                int ammount = data.FindAll(x => x == "END").Count();
                for (int a = 1; a <= ammount; a++)
                {
                    result.Add(new List<string>());
                }
                foreach (string line in data)
                {
                    if (line != "END")
                    {
                        result[i].Add(line);
                    }
                    else
                    {
                        i++;
                    }
                }
                return result;
            }

            foreach (List<string> list in split_by_separator())
            {
                Protection prot = new Protection();

                List<string> header = SplitParams(list[0]);
                prot.label = header[0];
                prot.init_label = header[1];
                prot.trip_label = header[2];

                List<string> logic = SplitParams(list[1]);
                prot.logic_config.AddRange(logic);

                List<string> triplevels = SplitParams(list[2]);

                List<List<string>> analogs = new List<List<string>>();
                bool separate_analogs = false;
                foreach (string line in list)
                {
                    if (line == "ANALOGS")
                    {
                        separate_analogs = true;
                    }
                    if (line == "ANALOGS_END")
                    {
                        separate_analogs = false;
                    }
                    if (separate_analogs & line != "ANALOGS")
                    {
                        List<string> analog = SplitParams(line);
                        analogs.Add(analog);
                    }
                }

                foreach (string line in triplevels)
                {
                    prot.tripLevels.Add(TryToParseFloat(line));
                }

                List<string> delays = SplitParams(list[3]);
                foreach (string line in delays)
                {
                    prot.timer_delays.Add(TryToParseInt(line));
                }

                List<string> breakers = SplitParams(list[4]);
                foreach (string line in breakers)
                {
                    prot.breaker_numbers.Add(TryToParseInt(line));
                }

                foreach (List<string> analog in analogs)
                {
                    int id = TryToParseInt(analog[0]);
                    bool isVoltage = false;
                    if (analog[1] == "True")
                    {
                        isVoltage = true;
                    }
                    string phase = analog[2];
                    int side = TryToParseInt(analog[3]);
                    prot.analogInputLinks.Add(new AnalogInputLink { id = id, isVoltage = isVoltage, phase = phase, side = side });
                }

                protectiongrid_collection.Add(prot);
            }
        }

        public List<string> SplitParams(string line)
        {
            List<string> split_params = new List<string>();
            string split = "";
            foreach (char symbol in line)
            {
                if (symbol.ToString() != ",")
                {
                    split += symbol;
                }
                else
                {
                    split_params.Add(split);
                    split = "";
                }
            }
            split_params.Add(split);
            return split_params;
        }

        public string StringListToString(List<string> list)
        {
            string converted = "";
            foreach (string line in list)
            {
                converted += line + ",";
            }
            if (converted.Length != 0)
            {
                converted = converted.Remove(converted.Length - 1);
            }
            return converted;
        }

        public string IntListToString(List<int> list)
        {
            string converted = "";
            foreach (int number in list)
            {
                converted += number.ToString(format) + ",";
            }
            if (converted.Length != 0)
            {
                converted = converted.Remove(converted.Length - 1);
            }
            return converted;
        }

        public string FloatListToString(List<float> list)
        {
            string converted = "";
            foreach (float number in list)
            {
                converted += number.ToString(format) + ",";
            }
            if (converted.Length != 0)
            {
                converted = converted.Remove(converted.Length - 1);
            }
            return converted;
        }

        int TryToParseInt(string value)
        {
            Int64.TryParse(value, out long number);
            return (int)number;
        }

        float TryToParseFloat(string value)
        {
            CultureInfo format = new CultureInfo("en-US");
            double.TryParse(value, NumberStyles.Float, format, out double number);
            return (float)number;
        }
    }
}
