using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppointmentNotificationManager
{
    public static class XmlHelper
    {
        private static Regex regexHTML = new Regex("<.*?>", RegexOptions.Compiled);
        private static Regex regexXML = new Regex("&.*?;", RegexOptions.Compiled);


        public static string StripXml(string xmlString)
        {
            return new string(regexXML.Replace(regexHTML.Replace(xmlString, string.Empty), string.Empty).Where(c => c != 8203).ToArray());
        }

        public static string GetHtmlDocument(string content)
        {
            return string.Format("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 3.2//EN\">\n<HTML>\n<HEAD>\n</HEAD>\n<BODY>\n{0}</BODY>\n</HTML>", content);
        }
    }
}