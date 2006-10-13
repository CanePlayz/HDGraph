using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

namespace HDGraph
{
    public class MoteurGraphiqueur : IXmlSerializable
    {
        private static string scanningMessage = null;

        private DirectoryNode root = null;

        public DirectoryNode Root
        {
            get { return root; }
        }

        private DateTime analyzeDate;

        public DateTime AnalyzeDate
        {
            get { return analyzeDate; }
            set { analyzeDate = value; }
        }


        public delegate void PrintInfoDelegate(string message);

        private PrintInfoDelegate printInfoDeleg = null;

        [XmlIgnore()]
        public PrintInfoDelegate PrintInfoDeleg
        {
            get { return printInfoDeleg; }
            set { printInfoDeleg = value; }
        }

        private bool pleaseCancelCurrentWork = false;

        public bool PleaseCancelCurrentWork
        {
            get { return pleaseCancelCurrentWork; }
            set { pleaseCancelCurrentWork = value; }
        }

        private bool workCanceled = false;

        public bool WorkCanceled
        {
            get { return workCanceled; }
            set { workCanceled = value; }
        }

        private bool autoRefreshAllowed;
        /// <summary>
        /// TODO: pr savoir s'il faire un refresh auto ou non dans le cas d'un changement de r�pertoire cible.
        /// </summary>
        public bool AutoRefreshAllowed
        {
            get { return autoRefreshAllowed; }
            set { autoRefreshAllowed = value; }
        }



        public MoteurGraphiqueur()
        {
            if (scanningMessage == null)
                scanningMessage = HDGTools.resManager.GetString("Scanning");
        }

        public void ConstruireArborescence(string path, int maxLevel)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("Invalid path.", "path");
            if (path.EndsWith(":"))
                path += @"\";
            pleaseCancelCurrentWork = false;
            workCanceled = false;
            if (maxLevel < 1)
                throw new ArgumentOutOfRangeException("maxLevel", "Il faut afficher au moins 1 niveau !");
            root = new DirectoryNode(path);

            ConstruireArborescence(root, maxLevel - 1);
            analyzeDate = DateTime.Now;
            if (workCanceled)
                root = null;
        }

        private void ConstruireArborescence(DirectoryNode dir, int maxLevel)
        {
            if (pleaseCancelCurrentWork)
            {
                workCanceled = true;
                return;
            }
            try
            {
                if (printInfoDeleg != null)
                    printInfoDeleg(scanningMessage + dir.Path + "...");
                DirectoryInfo dirInfo = new DirectoryInfo(dir.Path);
                if (maxLevel <= 0)
                {
                    FileInfo[] fis = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                    foreach (FileInfo fi in fis)
                    {
                        if (pleaseCancelCurrentWork)
                        {
                            workCanceled = true;
                            return;
                        }
                        dir.TotalSize += fi.Length;
                    }
                }
                else
                {
                    // Add file sizes.
                    FileInfo[] fis = dirInfo.GetFiles();
                    foreach (FileInfo fi in fis)
                    {
                        if (pleaseCancelCurrentWork)
                        {
                            workCanceled = true;
                            return;
                        }
                        dir.FilesSize += fi.Length;
                    }
                    dir.TotalSize += dir.FilesSize;

                    // Add subdirectory sizes.
                    DirectoryInfo[] dis = dirInfo.GetDirectories();
                    foreach (DirectoryInfo di in dis)
                    {
                        if (pleaseCancelCurrentWork)
                        {
                            workCanceled = true;
                            return;
                        }
                        DirectoryNode dirNode = new DirectoryNode(di.FullName);
                        ConstruireArborescence(dirNode, maxLevel - 1);
                        dirNode.Parent = dir;
                        dir.Children.Add(dirNode);
                        dir.TotalSize += dirNode.TotalSize;
                        if (dir.ProfondeurMax < dirNode.ProfondeurMax + 1)
                            dir.ProfondeurMax = dirNode.ProfondeurMax + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error during folder analysis ("+dir.Path+"). Folder skiped. Details: " + HDGTools.PrintError(ex));
                dir.Name += String.Format(HDGTools.resManager.GetString("ErrorLoading"), dir.Name, ex.Message);
            }
        }

        #region IXmlSerializable Membres

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            //string ns = "http://HDGraph.tools.laugel.fr/MoteurGraphiqueur.xsd";

            // D�but �l�ment MoteurGraphiqueur
            reader.ReadStartElement();

            XmlSerializer serializer = new XmlSerializer(typeof(DirectoryNode));
            root = (DirectoryNode)serializer.Deserialize(reader);

            analyzeDate = reader.ReadElementContentAsDateTime();

            // Fin �l�ment MoteurGraphiqueur
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            //string ns = "http://HDGraph.tools.laugel.fr/MoteurGraphiqueur.xsd";
            XmlSerializer serializer = new XmlSerializer(typeof(DirectoryNode));//, ns);
            serializer.Serialize(writer, root);

            writer.WriteElementString("AnalyzeDate", analyzeDate.ToString("s"));
        }

        #endregion
    }
}
