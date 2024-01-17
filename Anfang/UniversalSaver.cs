using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections;

namespace Anfang
{
    class UniversalSaver
    {
        public List<string> DumpToText(object objectToSave)
        {
            List<string> result = new List<string>();

            PropertyInfo[] properties = objectToSave.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance);
            FieldInfo[] fields = objectToSave.GetType().GetFields();

            foreach (var property in properties)
            {
                string line = "";
                string propertyName = property.Name;
                string propertyValue = property.GetValue(objectToSave).ToString();
                line = $"{propertyName}={propertyValue}";
                result.Add(line);
            }

            foreach (var field in fields)
            {
                string line = "";
                string fieldName = field.Name;
                line = $"{fieldName}=";
                if (field.GetValue(objectToSave) != null)
                {
                    if (field.FieldType.ToString().Contains("List"))
                    {
                        PropertyInfo[] listProperties = field.GetValue(objectToSave).GetType().GetProperties();
                        Type listObjectType = listProperties[2].GetType();
                        Type listType = field.GetValue(objectToSave).GetType();
                        IList list = (IList)field.GetValue(objectToSave);
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                line = line + item.ToString() + ";";
                            }
                        }
                    }
                    if (field.FieldType.ToString().Contains("ObservableCollection"))
                    {
                        if (field.FieldType.ToString().Contains("Complex32"))
                        {
                            ObservableCollection<Complex32> list = field.GetValue(objectToSave) as ObservableCollection<Complex32>;
                            foreach (var item in list)
                            {
                                line = line + item.ToString() + ";";
                            }
                        }
                        if (field.FieldType.ToString().Contains("bool"))
                        {
                            ObservableCollection<bool> list = field.GetValue(objectToSave) as ObservableCollection<bool>;
                            foreach (var item in list)
                            {
                                line = line + item.ToString() + ";";
                            }
                        }
                    }
                    if (!field.FieldType.ToString().Contains("ObservableCollection") & !field.FieldType.ToString().Contains("List"))
                    {
                        line += field.GetValue(objectToSave).ToString();
                    }
                }
                result.Add(line);
            }
            return result;
        }
    }
}
