using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace CashvaultCore.Utilities
{
    public static class XMLHelper
    {
        public static string ConvertObjectToXmlString(object obj)
        {
            string serializedString = String.Empty;
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, obj, namespaces);
                ms.Position = 0; //Position is currently at end, we need to set it to 0
                using (TextReader reader = new StreamReader(ms))
                {
                    serializedString = reader.ReadToEnd();
                }
            }
            return serializedString;
        }

        public static T ConvertXmlStringToType<T>(string xmlString) where T: class 
        {
            var deserializer = new XmlSerializer(typeof(T));         
            var sr = new StringReader(xmlString);
            var obj = deserializer.Deserialize(sr) as T;
            sr.Close();

            return obj;
        }

        public static string SerializeToString(object o)
        {
            string serialized = "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            //Serialize to memory stream
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(o.GetType());
            System.IO.TextWriter w = new System.IO.StringWriter(sb);
            ser.Serialize(w, o);
            w.Close();

            //Read to string
            serialized = sb.ToString();
            return serialized;
        }

        public static string XmlSerialize(object o)
        {
            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.GetEncoding(1252),
                    OmitXmlDeclaration = true
                };
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    var xmlSerializer = new XmlSerializer(o.GetType());
                    xmlSerializer.Serialize(writer, o);
                }
                return stringWriter.ToString();
            }
        }
    }
}
