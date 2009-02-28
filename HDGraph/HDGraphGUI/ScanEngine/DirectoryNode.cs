using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;

namespace HDGraph
{
    public enum SpecialDirTypes : short
    {
        /// <summary>
        /// Un r�pertoire ordinaire.
        /// </summary>
        NotSpecial,
        /// <summary>
        /// Indiquant que le r�pertoire courant est en fait un r�pertoire fictif repr�sentant 
        /// l'espace libre, et qu'il a �t� comptabilis� dans la taille du root.
        /// </summary>
        FreeSpaceAndShow,
        /// <summary>
        /// Indiquant que le r�pertoire courant est en fait un r�pertoire fictif repr�sentant 
        /// l'espace libre, et qu'il n'a PAS �t� comptabilis� dans la taille du root.
        /// </summary>
        FreeSpaceAndHide,
        /// <summary>
        /// Indique que le r�pertoire courant est en fait un r�pertoire fictif repr�sentant 
        /// les fichiers et dossiers qui n'ont pas �t� comptabilis�s suite � des erreurs d'acc�s.
        /// </summary>
        UnknownPart,
        /// <summary>
        /// Indique qu'il s'agit d'un r�pertoire dont le scan a �chou�.
        /// </summary>
        ScanError
    }

    public class DirectoryNode : IXmlSerializable
    {
        #region Variables et propri�t�s

        private long totalSize;
        /// <summary>
        /// Taille total en octet du r�pertoire
        /// </summary>
        public long TotalSize
        {
            get { return totalSize; }
            set { totalSize = value; }
        }
        /// <summary>
        /// Taille du r�pertoire sous format lisible.
        /// (par exemple ###.## Mo)
        /// </summary>
        public string HumanReadableTotalSize
        {
            get { return HDGTools.FormatSize(totalSize); }
        }


        private long filesSize;
        /// <summary>
        /// Taille en octet de l'ensemble des fichiers du r�pertoire
        /// </summary>
        public long FilesSize
        {
            get { return filesSize; }
            set { filesSize = value; }
        }

        #region Number of files

        /// <summary>
        /// Number of files that are in the current directory, without the sub directories.
        /// </summary>
        public long DirectoryFilesNumber { get; set; }

        /// <summary>
        /// Total number of files that are in the current directory, AND all sub directories.
        /// </summary>
        public long TotalRecursiveFilesNumber
        {
            get
            {
                long num = DirectoryFilesNumber;
                foreach (DirectoryNode node in this.Children)
                {
                    num += node.TotalRecursiveFilesNumber;
                }
                return num;
            }
        }

        #endregion


        /// <summary>
        /// Taille de l'ensemble des fichiers du r�pertoire sous format lisible.
        /// (par exemple ###.## Mo)
        /// </summary>
        public string HumanReadableFilesSize
        {
            get { return HDGTools.FormatSize(filesSize); }
        }
        /// <summary>
        /// Pourcentage de fichiers dans le r�pertoire.
        /// </summary>
        public string FilesSizePercent
        {
            get { return filesSize * 100 / totalSize + "%"; }
        }


        private string name;
        /// <summary>
        /// Nom du r�pertoire
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string path;
        /// <summary>
        /// Chemin du r�pertoire
        /// </summary>
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private DirectoryNode parent;
        /// <summary>
        /// R�pertoire parent
        /// </summary>
        public DirectoryNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Obtient le r�pertoire racine de l'arborescence dans laquelle se trouve ce r�pertoire.
        /// </summary>
        public DirectoryNode Root
        {
            get
            {
                DirectoryNode root = this;
                while (root.Parent != null)
                    root = root.Parent;
                return root;
            }
        }

        private List<DirectoryNode> children = new List<DirectoryNode>();
        /// <summary>
        /// Liste des r�pertoires contenus dans ce r�pertoire.
        /// </summary>
        public List<DirectoryNode> Children
        {
            get { return children; }
            set { children = value; }
        }

        private int depthMaxLevel = 1;
        /// <summary>
        /// Plus grande profondeur calcul�e sur le r�pertoire courant.
        /// </summary>
        public int DepthMaxLevel // Profondeur max
        {
            get { return depthMaxLevel; }
            set { depthMaxLevel = value; }
        }

        private bool existsUncalcSubdir;
        /// <summary>
        /// Booleen indiquant s'il existe des r�pertoires enfants qui n'ont pas �t� calcul�s.
        /// </summary>
        public bool ExistsUncalcSubDir
        {
            get { return existsUncalcSubdir; }
            set { existsUncalcSubdir = value; }
        }

        private SpecialDirTypes directoryType;
        /// <summary>
        /// Type de r�pertoire
        /// </summary>
        public SpecialDirTypes DirectoryType
        {
            get { return directoryType; }
            set { directoryType = value; }
        }

        #endregion

        #region Constructeur(s)

        public DirectoryNode(string path)
        {
            this.path = path;
            this.name = GetNameFromPath(path);
        }

        internal DirectoryNode()
        {

        }

        #endregion

        #region M�thodes

        public override string ToString()
        {
            return base.ToString() + ": " + name;
        }

        private string GetNameFromPath(string path)
        {
            string theName = System.IO.Path.GetFileName(path);
            if (theName == null || theName.Length == 0)
                theName = path;
            return theName;
        }

        /// <summary>
        /// Se base sur le nom du node courant et sur le path du p�re pour mettre � jour le path courant.
        /// </summary>
        private void UpdatePathFromNameAndParent()
        {
            if (parent != null)
                this.path = parent.path + System.IO.Path.DirectorySeparatorChar + name;
            foreach (DirectoryNode node in children)
            {
                node.UpdatePathFromNameAndParent();
            }
        }

        /// <summary>
        /// Return true if the current directory contains recursively more than X directories.
        /// </summary>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public bool HasMoreDirectoriesThan(long threshold)
        {
            return CountDirectoriesToThreshold(threshold) > threshold;
        }

        /// <summary>
        /// Count recusively the number of directories contained in the current directory.
        /// Stop counting if the threshold is reached.
        /// </summary>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private long CountDirectoriesToThreshold(long threshold)
        {
            long num = this.children.Count;
            foreach (DirectoryNode node in this.Children)
            {
                if (num > threshold)
                    return num;
                num += node.CountDirectoriesToThreshold(threshold - num);
            }
            return num;
        }

        #endregion

        #region IXmlSerializable Membres

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            // D�but �l�ment DirectoryNode
            reader.ReadStartElement();
            if (reader.NodeType == System.Xml.XmlNodeType.EndElement)
                return;
            name = reader.ReadElementContentAsString();
            totalSize = reader.ReadElementContentAsLong();
            filesSize = reader.ReadElementContentAsLong();
            if (reader.Name == "DirectoryFilesCount") // because v1.2 doesn't contains the "DirectoryFilesCount" value
                DirectoryFilesNumber = reader.ReadContentAsLong();
            // here : reader.Name is "ProfondeurMax" (v1.2) or "DepthMaxLevel" (v1.3)
            depthMaxLevel = reader.ReadElementContentAsInt();
            existsUncalcSubdir = Boolean.Parse(reader.ReadElementContentAsString());
            directoryType = (SpecialDirTypes)Convert.ToInt16(reader.ReadElementContentAsInt());
            // D�but �l�ment Children
            reader.ReadStartElement("Children");
            XmlSerializer serializer = new XmlSerializer(typeof(List<DirectoryNode>));
            children = (List<DirectoryNode>)serializer.Deserialize(reader);
            // Fin �l�ment Children
            reader.ReadEndElement();

            // Mise � jour du parent
            foreach (DirectoryNode node in children)
            {
                node.parent = this;
            }

            // Mise � jour du path
            if (name.Contains(":"))
            {
                path = name;
                name = GetNameFromPath(path);
                UpdatePathFromNameAndParent();
            }

            // Fin �l�ment DirectoryNode
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            if (parent == null)
                writer.WriteElementString("Name", path);
            else
                writer.WriteElementString("Name", name);
            writer.WriteElementString("TotalSize", totalSize.ToString());
            writer.WriteElementString("FilesSize", filesSize.ToString());
            writer.WriteElementString("DirectoryFilesCount", DirectoryFilesNumber.ToString());
            writer.WriteElementString("DepthMaxLevel", depthMaxLevel.ToString());
            writer.WriteElementString("ExistsUncalcSubdir", existsUncalcSubdir.ToString());
            writer.WriteElementString("DirectoryType", ((short)directoryType).ToString());
            writer.WriteStartElement("Children");
            XmlSerializer serializer = new XmlSerializer(typeof(List<DirectoryNode>));
            serializer.Serialize(writer, children);
            writer.WriteEndElement();

            // Pour s�rialiser g�n�riquement les propri�t�s d'un  objet :
            //foreach (PropertyInfo prop in this.GetType().GetProperties())
            //{
            //    if (prop.GetAccessors().Length > 1  // il faut un get et un set
            //        && prop.GetAccessors()[0].IsPublic   // chacun des 2 doivent �tre publiques
            //        && prop.GetAccessors()[1].IsPublic   // chacun des 2 doivent �tre publiques
            //        && prop.Name != "Parent"
            //        && prop.Name != "Path"
            //        && prop.Name != "Name"
            //        && prop.Name != "ProfondeurMax")
            //    {
            //        writer.WriteStartElement(prop.Name);
            //        XmlSerializer serializer = new XmlSerializer(prop.PropertyType);
            //        serializer.Serialize(writer, prop.GetValue(this, null));
            //        writer.WriteEndElement();
            //    }
            //}
        }

        #endregion
    }
}
