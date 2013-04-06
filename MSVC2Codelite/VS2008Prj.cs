using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace MSVC2Codelite
{
    public class VS2008Prj
    {
        public static void OutputVS2008(string vs2008Prj, List<VirtualDirection> target)
        {
            XmlDocument dst_doc = new XmlDocument();
            dst_doc.Load(vs2008Prj);
            XmlElement dst_root = dst_doc.DocumentElement;
            Dictionary<string, List<XmlElement> > fileConfigs = new Dictionary<string, List<XmlElement>>();
            XmlNodeList fileList = dst_root.GetElementsByTagName("File");
            foreach (XmlNode node in fileList)
            {
                XmlAttribute attr=node.Attributes["RelativePath"];
                string n = attr.ToString();
                foreach (XmlNode cn in node.ChildNodes)
                {
                    if (cn.Name == "FileConfiguration")
                    {
                        if (!fileConfigs.ContainsKey(n))
                        {
                            fileConfigs[n] = new List<XmlElement>();
                        }
                        fileConfigs[n].Add(cn as XmlElement);
                    }
                }
            }
            XmlNodeList filesRootList = dst_root.GetElementsByTagName("Files");
            XmlNode root = filesRootList[0];
            
            //////////////////////////////////////////////////////////////////////////
            //修改2008 的工程
            XmlNodeList virtualNodeList = dst_root.GetElementsByTagName("Filter");
            while (virtualNodeList.Count > 0)
            {
                XmlNode node = virtualNodeList.Item(0);
                node.ParentNode.RemoveChild(node);
            }
            foreach (VirtualDirection sub in target)
            {
                CreateVS2008XmlNode(sub, root, dst_doc, fileConfigs);
            }

            dst_doc.Save(vs2008Prj);
        }
        public static void CreateVS2008XmlNode(VirtualDirection vdir, XmlNode node, XmlDocument doc, Dictionary<string, List<XmlElement>> fileConfigs)
        {
            XmlElement elem = doc.CreateElement("Filter");
            XmlAttribute attr_elem = doc.CreateAttribute("Name");
            attr_elem.InnerText = vdir.m_name;
            elem.Attributes.Append(attr_elem);

            foreach (string file in vdir.m_files)
            {
                XmlElement elem_file = doc.CreateElement("File");
                XmlAttribute attr = doc.CreateAttribute("RelativePath");
                attr.InnerText = file;
                elem_file.Attributes.Append(attr);
                elem.AppendChild(elem_file);
                
                if (fileConfigs.ContainsKey(file))
                {
                    List<XmlElement> cfg = fileConfigs[file];
                    foreach (XmlElement cfgNode in cfg)
                    {
                        elem.AppendChild(cfgNode);
                    }
                }

            }
            node.AppendChild(elem);
            foreach (VirtualDirection sub in vdir.m_subs)
            {
                CreateVS2008XmlNode(sub, elem, doc, fileConfigs);
            }
        }
    }

}
