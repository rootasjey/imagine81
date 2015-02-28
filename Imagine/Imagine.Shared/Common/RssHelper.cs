using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Imagine.Common
{
    public static class RssHelper
    {

        /// <summary>
        /// RSS has a specific DateTime format - this method pulls out the DateTime from that provided
        /// </summary>
        /// <param name="Date">Incoming RSS DateTime</param>
        /// <returns>Compatible .NET DateTime</returns>
        public static DateTime ParseRssDate(string Date)
        {
            // clean up in the input date
            var newDate = Date;
            newDate = newDate.Replace("GMT", "+0:00");
            newDate = newDate.Replace("PST", "-8:00");
            newDate = newDate.Replace("PDT", "-7:00");
            newDate = newDate.Replace("MST", "-7:00");
            newDate = newDate.Replace("CST", "-6:00");
            newDate = newDate.Replace("CDT", "-5:00");
            newDate = newDate.Replace("EST", "-5:00");
            newDate = newDate.Replace("EDT", "-4:00");
            newDate = newDate.Replace("AST", "-4:00");
            newDate = newDate.Replace("ADT", "-3:00");
            // add your own here if I missed one you need

            // run the conversion
            var provider = CultureInfo.InvariantCulture;
            DateTime result;
            const string format = "ddd, dd MMM yyyy HH:mm:ss zzz";

            try
            {
                result = DateTime.ParseExact(newDate, format, provider);
            }
            catch (Exception)
            {
                //return the min value for that item. no need to have the entire feed fail
                //result = DateTime.MinValue;
                result = DateTime.Now;
            }

            // all done!
            return result;
        }



        /// <summary>
        /// Pull out the first img tag we find in the html description.
        /// If not, then use a default image.
        /// </summary>
        /// <param name="description">the description from the podcast rss feed</param>
        /// <returns>description without the HTML markup</returns>
        public static string ParseImageFromDescription(string description)
        {
            // if there is no image, then kick out the default
            var imgStart = description.IndexOf("img ");
            if (imgStart <= 0)
                return null; // Ajouter l'image par défaut

            // find where the src parameter starts
            var srcStart = description.IndexOf("src=", imgStart);

            // this is either a " or a '
            var wrapper = description.Substring(srcStart + 4, 1);

            // find where the other " or ' is located
            var srcEnd = description.IndexOf(wrapper, srcStart + 5);

            // pull out the url from between the "'s
            var src = description.Substring(srcStart + 5, srcEnd - srcStart - 5);
            return src;
        }



        /// <summary>
        /// Take the first non-null, non-empty value from the list
        /// </summary>
        /// <param name="default">supply a default value if no matches are made</param>
        /// <param name="parms">ParamArray for the items to coalesce</param>
        /// <returns>the first non-null, non-empty value, or the default</returns>
        public static object Coalesce(params object[] parms)
        {
            foreach (var p in parms)
            {
                if (p != null && p.ToString().Length > 0)
                    return p;
            }
            return null;
        }
        /// <summary>
        /// Remove HTML tags from a given string
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static string StripHtmlTags(string value)
        {
            const string HTML_TAG_PATTERN = "<.*?>";
            var result = Regex.Replace(value, HTML_TAG_PATTERN, string.Empty);
            return result;
        }

        /// <summary>
        /// Look for an element and find it's value.
        /// </summary>
        /// <param name="element">the element in which to search</param>
        /// <param name="elementName">the child element name</param>
        /// <param name="attributeName">if supplied, find the attribute value instead of the element value</param>
        /// <returns>value from the element or attribute</returns>
        public static string getElementValue(XElement element, XName elementName, string attributeName = "")
        {
            if (element == null) return null;

            var e = element.Element(elementName);
            if (e == null)
                return "";

            if (attributeName.Length > 0)
            {
                var a = e.Attribute(attributeName);
                return a == null ? "" : a.Value;
            }
            else
            {
                return e == null ? "" : e.Value;
            }
        }

        public static string ReplaceHTMLCodeSpecialChars(string text)
        {
            text = text.Replace("&#x27;", "").Replace("&amp;", "&").Replace("&lt;", ">").Replace("&gt;", ">").Replace("&quot;", "\"");

            return text;
        }

    }
}
