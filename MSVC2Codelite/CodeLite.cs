using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MSVC2Codelite
{
    public class CodeLite
    {
        public static void OutputCodeLite(string codeliteProject,List<VirtualDirection> target)
        {
            XmlDocument dst_doc = new XmlDocument();
            dst_doc.Load(codeliteProject);
            XmlElement dst_root = dst_doc.DocumentElement;
            //////////////////////////////////////////////////////////////////////////
            //修改codelite 的工程
            XmlNodeList virtualNodeList = dst_root.GetElementsByTagName("VirtualDirectory");
            while (virtualNodeList.Count > 0)
            {
                XmlNode node = virtualNodeList.Item(0);
                node.ParentNode.RemoveChild(node);
            }
            foreach (VirtualDirection sub in target)
            {
                CreateCodeLiteXmlNode(sub, dst_root, dst_doc);
            }

            dst_doc.Save(codeliteProject);
        }
        public static void CreateCodeLiteXmlNode(VirtualDirection vdir, XmlNode node, XmlDocument doc)
        {
            XmlElement elem = doc.CreateElement("VirtualDirectory");
            XmlAttribute attr_elem = doc.CreateAttribute("Name");
            attr_elem.InnerText = vdir.m_name;
            elem.Attributes.Append(attr_elem);

            foreach (string file in vdir.m_files)
            {
                XmlElement elem_file = doc.CreateElement("File");
                XmlAttribute attr = doc.CreateAttribute("Name");
                attr.InnerText = file;
                elem_file.Attributes.Append(attr);
                elem.AppendChild(elem_file);
            }
            node.AppendChild(elem);
            foreach (VirtualDirection sub in vdir.m_subs)
            {
                CreateCodeLiteXmlNode(sub, elem, doc);
            }
        }
        
    }
}
