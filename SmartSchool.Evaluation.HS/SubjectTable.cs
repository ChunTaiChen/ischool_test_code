using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.Evaluation
{
    class SubjectTable
    {
        static private SubjectTable _Items = null;
        static public SubjectTable Items
        {
            get 
            {
                if ( _Items == null ) _Items = new SubjectTable();
                return _Items;
            }
        }
        public SubjectTable() { }

        private Dictionary<string, SubjectTableCollection> _SubjectTableCatalog = new Dictionary<string, SubjectTableCollection>();
        public SubjectTableCollection this[string catalog]
        {
            get
            {
                lock ( _SubjectTableCatalog )
                {
                    if ( !_SubjectTableCatalog.ContainsKey(catalog) )
                        _SubjectTableCatalog.Add(catalog, new SubjectTableCollection(catalog));
                }
                return _SubjectTableCatalog[catalog];
            }
        }
    }
    class SubjectTableCollection:Collection<SubjectTableItem>
    {
        private string _Catalog = "";

        public SubjectTableCollection(string catalog)
        {
            _Catalog = catalog;
            Reflash();
        }

        public void Reflash()
        { 
            this.Clear();
            foreach ( XmlElement var in SmartSchool.Feature.SubjectTable.QuerySubejctTable.GetSubejctTableList(_Catalog).GetContent().GetElements("SubjectTable") )
            {
                this.Add(new SubjectTableItem(var));
            }
        }

        public SubjectTableItem this[string name]
        {
            get
            {
                foreach ( SubjectTableItem table in this.Items )
                {
                    if ( table.Name == name )
                        return table;
                }
                return null;
            }
        }

        public bool Contains(string name)
        {
            foreach ( SubjectTableItem table in this.Items )
            {
                if ( table.Name == name )
                    return true;
            }
            return false;
        }
    }
    class SubjectTableItem
    {
        private string _ID;
        private string _Name;
        private string _Catalog;
        private XmlElement _Content;

        private readonly string _SchoolYear;
        private readonly string _TrimName;

        public SubjectTableItem(XmlElement subjectTableElement)
        {
            DSXmlHelper helper=new DSXmlHelper(subjectTableElement);
            _ID = subjectTableElement.GetAttribute("ID");
            _Name = helper.GetText("Name");
            _Catalog = helper.GetText("Catalog");
            _Content = helper.GetElement("Content");

            _SchoolYear = string.Empty;
            foreach (XmlElement node in _Content.ChildNodes)
            {
                if (node.Name == "SubjectTableContent")
                {
                    _SchoolYear = node.HasAttribute("SchoolYear") ? node.GetAttribute("SchoolYear") : string.Empty;
                    break;
                }
            }

            _TrimName = _Name;
            if (!string.IsNullOrEmpty(_SchoolYear))
            {
                //_TrimName = _Name.TrimStart(_SchoolYear.ToCharArray());
                _TrimName = _Name.Substring(_SchoolYear.Length);
            }

        }

        public string ID { get { return _ID; } }
        public string Name { get { return _Name; } }
        public string SchoolYear { get { return _SchoolYear; } }
        public string TrimName { get { return _TrimName; } }
        public string Catalog { get { return _Catalog; } }
        public XmlElement Content { get { return _Content; } }

        public override string ToString()
        {
            return _Name;
        }
    }
}
