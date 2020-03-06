using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Common.log;

namespace Base.kit
{
    public class XmlKit
    {
        private static readonly NLOG Logger = new NLOG("Base.kit");
        public bool isxml(string XmlPath)
        {
            if (System.IO.File.Exists(XmlPath) == false) {
                return false;
            }
            string xmlpath = XmlPath;
            StreamReader sr = new StreamReader(xmlpath);
            string strXml = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(strXml);//判断是否加载成功
                return true;//是xml文件，返回
            }
            catch
            {
                Logger.Debug("wrong xml file!!");
                return false;//不是xml文件，返回
            }
        }
        public static string GetByXml(string name, XmlNode node, string def = null)
        {
            try
            {
                XmlAttribute typeAttr = node.Attributes[name];
                if (typeAttr == null)
                {
                    return def;
                }
                return typeAttr.Value;
            }
            catch (Exception)
            {
                return def;
            }
        }
        private static M ParseXmlFileRecursion(XmlNode node)
        {
            M result = new M
            {
                Name = node.Name.ToUpper(),
                Attribute = new R1<string, string>(),
                Children = new R1<string, List<M>>()
            };

            foreach (XmlAttribute attr in node.Attributes)
            {
                result.Attribute.Add(attr.Name, attr.Value);
            }

            foreach (XmlNode n in node.ChildNodes)
            {
                List<M> c = result.Children.Get(n.Name.ToUpper());
                if (c == null)
                {
                    c = new List<M>();
                    result.Children.Add(n.Name.ToUpper(), c);
                }

                c.Add(ParseXmlFileRecursion(n));
            }

            return result;
        }

        public static M ParseXmlFile(string xmlFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(xmlFile, settings);
            xmlDoc.Load(reader);
            XmlNode root = xmlDoc.DocumentElement as XmlNode;
            M result = ParseXmlFileRecursion(root);
            reader.Close();
            return result;
        }


        public static string ConvertXmlToString(XmlDocument xmlDoc)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            xmlDoc.Save(writer);
            StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();
            return xmlString;
        }

        private static bool SerializeToXml1<T>(T myObject, Stream stream)
        {
            if (myObject == null || stream == null)
            {
                return false;
            }

            XmlSerializer serializer = new XmlSerializer(myObject.GetType());
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            settings.NewLineChars = "\r\n";
            settings.Encoding = Encoding.UTF8;
            settings.OmitXmlDeclaration = true;  // 不生成声明头
            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    serializer.Serialize(xmlWriter, myObject, namespaces);
                    xmlWriter.Close();
                };
                return true;
            }
            catch (Exception e)
            {
                Logger.Info(" error----------------[{}]",e.ToString());
                return false;
            }
        }

        public static string SerializeToXml(object myObject)
        {
            var stream = new MemoryStream();
            string returnStr = null;
            if (SerializeToXml1(myObject, stream))
            {
                byte[] dataBytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(dataBytes, 0, (int)stream.Length);
                returnStr = Encoding.UTF8.GetString(dataBytes);
            }

            stream.Close();
            return returnStr;
        }

        public static void SerializeToXml<T>(T myObject, string file)
        {
            var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
            if (SerializeToXml1(myObject, stream))
            {

            }

            stream.Close();
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="xml">XML字符</param>
        /// <returns></returns>
        public static T DeserializeToObject1<T>(string xml)
        {
            T myObject;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(xml);
            myObject = (T)serializer.Deserialize(reader);
            reader.Close();
            return myObject;
        }

        public static T DeserializeToObject<T>(string path)
        {
            T myObject = default(T);
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(path);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                myObject = (T)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                Logger.Info(" error----------------[{}]", e.ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return myObject;
        }
    }
}
