using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MSVC2Codelite
{
    class Program
    {
        public class VirtualDirection
        {
            public string m_name;
            public List<string> m_files = new List<string>();
            public List<VirtualDirection> m_subs = new List<VirtualDirection>();
        }
        static List<VirtualDirection> ms_target=new List<VirtualDirection>();
        static void MSVCToCodeLite(string msvc_filters, string codeliteProject)
        {
            XmlDocument src_doc = new XmlDocument();
            src_doc.Load(msvc_filters);
            XmlDocument dst_doc = new XmlDocument();
            dst_doc.Load(codeliteProject);
            dst_doc.Save(codeliteProject + ".bak");
            XmlElement dst_root = dst_doc.DocumentElement;
            XmlElement src_root = src_doc.DocumentElement;
            XmlNodeList itemGroup = src_root.GetElementsByTagName("ItemGroup");
            foreach(XmlNode groupNode in itemGroup)
            {
                XmlElement group_elem = (XmlElement)groupNode;
                foreach (XmlNode cnode in group_elem.ChildNodes)
                {
                    if (cnode is XmlElement)
                    {
                        XmlElement cpp_elem = (XmlElement)cnode;
                        ReadItem(cpp_elem);
                    }
                }
                
                //XmlNodeList cppNodes = group_elem.GetElementsByTagName("ClCompile");
                //foreach (XmlNode cpp_node in cppNodes)
                //{
                //    XmlElement cpp_elem = (XmlElement)cpp_node;
                //    ReadItem(cpp_elem);
                //}
                //XmlNodeList headerNodes = group_elem.GetElementsByTagName("ClInclude");
                //foreach (XmlNode h_node in headerNodes)
                //{
                //    XmlElement h_elem = (XmlElement)h_node;
                //    ReadItem(h_elem);
                //}
            }
            //////////////////////////////////////////////////////////////////////////
            //修改codelite 的工程
            XmlNodeList virtualNodeList = dst_root.GetElementsByTagName("VirtualDirectory");
            while (virtualNodeList.Count > 0)
            {
                XmlNode node = virtualNodeList.Item(0);
                node.ParentNode.RemoveChild(node);
            }
            foreach (VirtualDirection sub in ms_target)
            {
                //.AppendChild(xmlelem2);
                CreateXmlNode(sub, dst_root, dst_doc);
            }
            
            dst_doc.Save(codeliteProject);
        }
        static void CreateXmlNode(VirtualDirection vdir, XmlNode node,XmlDocument doc)
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
                CreateXmlNode(sub, elem, doc);
            }
        }
        static void ReadItem(XmlElement node)
        {
            if (!node.HasAttribute("Include"))
            {
                return;
            }
            string filter;

            string file = node.GetAttribute("Include");
            XmlNodeList filter_nodeList = node.GetElementsByTagName("Filter");
            if (filter_nodeList.Count > 0)
            {
                XmlElement filter_elem = (XmlElement)filter_nodeList.Item(0);
                filter = filter_elem.FirstChild.InnerText;
                AddFile(file, filter);
            }
        }
        static void AddFile(string file, string filter)
        {
            string filter_name;
            VirtualDirection vdir = CheckVDir(filter);
            vdir.m_files.Add(file);
        }
        static VirtualDirection CheckVDir(string filter)
        {
            string[] filterPath = filter.Split('\\');
            List<VirtualDirection>  beginList = ms_target;
            VirtualDirection vdir = null;
            foreach (string name in filterPath)
            {
                vdir = _CheckVDir(name, beginList);
                beginList = vdir.m_subs;
            }
            return vdir;
        }
        static VirtualDirection _CheckVDir(string name, List<VirtualDirection> vlist)
        {
            VirtualDirection vdir = vlist.Find(delegate(VirtualDirection bk)
            {
                return bk.m_name == name;
            });
            if (vdir == null)
            {
                vdir = new VirtualDirection();
                vdir.m_name=name;
                vlist.Add(vdir);
            }
            return vdir;
        }
        static void Main(string[] args)
        {
            MSVCToCodeLite(args[0], args[1]);
            //foreach (string s in args)
            //{
            //    Console.WriteLine(s);
            //}
            Console.WriteLine("Succ");
        }
    }
}
