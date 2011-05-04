using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace com.johnruiz.minecraft
{
    public class MinecraftConfig : IConfigurationSectionHandler
    {
        public string JavaExecutable { get; set; }

        public string MinecraftJarDirectory { get; set; }

        public int MaxHeapInMegabytes { get; set; }

        public int InitialHeapInMegabytes { get; set; }


        public object Create(object parent, object configContext, XmlNode section)
        {
            XmlSerializer ser = new XmlSerializer(typeof(MinecraftConfig));
            MinecraftConfig config = null;

            using (XmlNodeReader reader = new XmlNodeReader(section))
            {
                config = ser.Deserialize(reader) as MinecraftConfig;
                reader.Close();
            }

            return config;
        }
    }
}
