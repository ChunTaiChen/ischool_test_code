using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Web;
using System.Security;
using System.IO;

namespace SmartSchool.Evaluation.Reports.Retake
{
    public class XMLEncoding
    {
        public static string Encoding(string str)
        {
            return SecurityElement.Escape(str).Replace("\r", "&#xD;").Replace("\n", "&#xA;");
        }
    }
}
