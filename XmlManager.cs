using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace TradeRobot
{
    public class XmlManager
    {
        private readonly XmlDocument doc;

        public XmlManager()
        {
            doc = new XmlDocument();
        }

        public bool IsLoaded
        {
            get
            {
                return doc.DocumentElement != null;
            }
        }

        public int Count
        {
            get
            {
                return doc.DocumentElement.ChildNodes.Count;
            }
        }

        public void Load(string url)
        {
            doc.Load(url);
        }

        public string GetName(int index)
        {
            return
                doc.DocumentElement.ChildNodes[index].SelectSingleNode("Name").InnerText;
        }

        public string GetId(int index)
        {
            return
                doc.DocumentElement.ChildNodes[index].Attributes["ID"].InnerText;
        }

        public string GetDate(int index)
        {
            return
                doc.DocumentElement.ChildNodes[index].Attributes["Date"].InnerText;
        }

        public double GetValue(int index)
        {
            return
                double.Parse(
                    doc.DocumentElement.ChildNodes[index].SelectSingleNode("Value")
                        .InnerText
                );
        }

        public int GetNominal(int index)
        {
            return
                int.Parse(
                    doc.DocumentElement.ChildNodes[index].SelectSingleNode("Nominal")
                        .InnerText
                );
        }

        public string GetCharCode(int index)
        {
            return
                doc.DocumentElement.ChildNodes[index].SelectSingleNode("CharCode").InnerText;
        }
    }
}