using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore
{
    public enum ScoreType
    {
        原始成績,
        原始補考擇優,
        擇優成績
    }

    public enum RatingMethod
    {
        班級排名,
        年級排名,
        科別排名
    }

    public class ReportOptions
    {
        public ReportOptions()
        {
            #region 讀取 Preference

            XmlElement config = CurrentUser.Instance.Preference["多學期成績單"];

            if ( config != null )
            {
                if(config.HasAttribute("FixMoralScore"))
                {
                    bool.TryParse(config.GetAttribute("FixMoralScore"), out fix_moral_score);
                }

                XmlElement print = (XmlElement)config.SelectSingleNode("Print");
                XmlElement customize = (XmlElement)config.SelectSingleNode("CustomizeTemplate");
                XmlElement printsemester = (XmlElement)config.SelectSingleNode("PrintSemesters");

                if ( config.SelectNodes("PrintEntry").Count > 0 )
                {
                    print_entries.Clear();
                    foreach ( XmlElement var in config.SelectNodes("PrintEntry"))
                    {
                        print_entries.Add(var.InnerText);
                    }
                }

                if ( print != null )
                {
                    int tryInt;
                    score = (ScoreType)( print.HasAttribute("ScoreType") ? ( int.TryParse(print.GetAttribute("ScoreType"), out tryInt) ? tryInt : 0 ) : 0 );
                    rating = (RatingMethod)( print.HasAttribute("RatingMethod") ? ( int.TryParse(print.GetAttribute("RatingMethod"), out tryInt) ? tryInt : 0 ) : 0 );

                    bool tryBool;
                    if ( print.HasAttribute("Default") && bool.TryParse(print.GetAttribute("Default"), out tryBool) )
                        is_default_template = tryBool;
                    else
                        is_default_template = true;
                }
                else
                {
                    print = config.OwnerDocument.CreateElement("Print");
                    config.AppendChild(print);
                }

                if ( customize != null )
                {
                    customize_template = ( !string.IsNullOrEmpty(customize.InnerText) ) ? Convert.FromBase64String(customize.InnerText) : null;
                }
                else
                {
                    customize = config.OwnerDocument.CreateElement("CustomizeTemplate");
                    config.AppendChild(customize);
                }

                if ( printsemester != null )
                {
                    int tryInt;
                    int.TryParse(printsemester.InnerText,out printSemesters);
                    
                }
                else
                {
                    printsemester = config.OwnerDocument.CreateElement("PrintSemesters");
                    config.AppendChild(printsemester);
                }

                CurrentUser.Instance.Preference["多學期成績單"] = config;
            }
            else
            {
                config = new XmlDocument().CreateElement("多學期成績單");
                XmlElement new_print = config.OwnerDocument.CreateElement("Print");
                XmlElement new_customize = config.OwnerDocument.CreateElement("CustomizeTemplate");
                XmlElement new_printsemesters = config.OwnerDocument.CreateElement("PrintSemesters");
                config.AppendChild(new_print);
                config.AppendChild(new_customize);
                config.AppendChild(new_printsemesters);
                CurrentUser.Instance.Preference["多學期成績單"] = config;
            }

            #endregion
        }

        public void Save()
        {
            #region 儲存 Preference

            XmlElement config = CurrentUser.Instance.Preference["多學期成績單"];

            if ( config == null )
                config = new XmlDocument().CreateElement("多學期成績單");

            XmlElement print = config.OwnerDocument.CreateElement("Print");
            XmlElement customize = config.OwnerDocument.CreateElement("CustomizeTemplate");
            XmlElement printsemester = config.OwnerDocument.CreateElement("PrintSemesters");

            customize.InnerText = customize_template != null ? Convert.ToBase64String(customize_template) : "";
            printsemester.InnerText = printSemesters.ToString();

            config.RemoveAll();
            config.AppendChild(print);
            config.AppendChild(customize);
            config.AppendChild(printsemester);

            foreach ( string entry in print_entries )
            {
                XmlElement printentry = config.OwnerDocument.CreateElement("PrintEntry");
                printentry.InnerText = entry;
                config.AppendChild(printentry);
            }

            print.SetAttribute("ScoreType", "" + ( score == ScoreType.原始成績 ? 0 : ( score == ScoreType.原始補考擇優 ? 1 : 2 ) ));
            print.SetAttribute("RatingMethod", rating.ToString());
            print.SetAttribute("Default", is_default_template.ToString());

            config.SetAttribute("FixMoralScore",""+fix_moral_score);

            CurrentUser.Instance.Preference["多學期成績單"] = config;

            #endregion
        }

        private ScoreType score;
        public ScoreType ScoreType
        {
            get { return score; }
            set { score = value; }
        }

        private RatingMethod rating;
        public RatingMethod RatingMethod
        {
            get { return rating; }
            set { rating = value; }
        }

        private int printSemesters = 0;
        public int PrintSemester
        {
            get { return printSemesters; }
            set { printSemesters = value; }
        }

        private bool is_default_template = true;
        public bool IsDefaultTemplate
        {
            get { return is_default_template; }
            set { is_default_template = value; }
        }

        public byte[] DefaultTemplate
        {
            get { return Properties.Resources.多學期成績單; }
        }

        private byte[] customize_template = null;
        public byte[] CustomizeTemplate
        {
            get
            {
                if ( customize_template == null )
                    return Properties.Resources.多學期成績單;
                return customize_template;
            }
            set { customize_template = value; }
        }

        private bool fix_moral_score = true;
        public bool FixMoralScore
        {
            get { return fix_moral_score; }
            set { fix_moral_score = value; }
        }

        private List<string> print_entries = new List<string>(new  string[] { "學業","體育","健康與護理","國防通識","實習科目","德行"});
        public List<string> PrintEntries
        {
            get { return print_entries; }
            set { print_entries = value; }
        }
    }
}
