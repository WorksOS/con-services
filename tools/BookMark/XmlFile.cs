using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BookMark
{
    public class XmlFile
    {
        public string FileName = string.Empty;

        /// <summary>
        /// Update the XML data with the new date time.
        /// </summary>
        /// <param name="newDateTime">New date time to set to</param>
        /// <param name="fileName">XML input file</param>
        public XDocument UpdateAllBookMarksXmlDataWithNewDate(DateTime newDateTime, string fileName)
        {
            XDocument xDoc = XDocument.Load(fileName);
            var elementsToUpdate = xDoc.Descendants().Where(o => o.Name == "BookmarkUTC" && !o.HasElements);
            foreach (XElement element in elementsToUpdate)
            {
                element.Value = newDateTime.ToString("yyyy-MM-dd") + "T00:00:00";
            }
            return xDoc;
        }

        /// <summary>
        /// Update the selected bookmarks from the data grid in the XML file
        /// </summary>
        /// <param name="newDateTime">Date and time to set the bookmark to</param>
        /// <param name="fileName">XML input file</param>
        /// <param name="selectedBookMarks">List of selected bookmarks</param>
        /// <returns>XML document</returns>
        public XDocument UpdateSelectedBookMarksXmlDataWithNewDateAndRemoveNotUpdated(DateTime newDateTime, string fileName, List<XmlBookMark> selectedBookMarks)
        {
            XDocument xDoc = XDocument.Load(fileName);
            var elementsToUpdate = xDoc.Descendants().Where(o => o.Name == "BookmarkUTC" || o.Name == "Key");
            bool containsItem = false;
            foreach (XElement element in elementsToUpdate)
            {
                if (element.Name == "Key")
                {
                    containsItem = selectedBookMarks.Any(item => item.Customer == element.Value); // found it in the list
                }
                if (element.Name == "BookmarkUTC" && containsItem)
                {
                    element.Value = newDateTime.ToString("yyyy-MM-dd") + "T00:00:00";
                }
            }

            var cutDownDocument = RemoveAllElementsThatDidNotGetUpdated(selectedBookMarks, xDoc);
            return cutDownDocument;
        }

      /// <summary>
      /// Update the selected bookmarks from the data grid in the XML file then save whole file
      /// </summary>
      /// <param name="newDateTime">Date and time to set the bookmark to</param>
      /// <param name="fileName">XML input file</param>
      /// <param name="selectedBookMarks">List of selected bookmarks</param>
      /// <returns>XML document</returns>
      public XDocument UpdateSelectedBookMarksXmlDataWithNewDateTheSaveFullFile(DateTime newDateTime, string fileName, List<XmlBookMark> selectedBookMarks)
      {
        XDocument xDoc = XDocument.Load(fileName);
        var elementsToUpdate = xDoc.Descendants().Where(o => o.Name == "BookmarkUTC" || o.Name == "Key");
        bool containsItem = false;
        foreach (XElement element in elementsToUpdate)
        {
          if (element.Name == "Key")
          {
            containsItem = selectedBookMarks.Any(item => item.Customer == element.Value); // found it in the list
          }
          if (element.Name == "BookmarkUTC" && containsItem)
          {
            element.Value = newDateTime.ToString("yyyy-MM-dd") + "T00:00:00";
          }
        }
        return xDoc;
    }

    /// <summary>
    /// Remove all the elements that weren't updated
    /// </summary>
    /// <param name="selectedBookMarks"></param>
    /// <param name="xDoc"></param>
    /// <returns></returns>
    private XDocument RemoveAllElementsThatDidNotGetUpdated(List<XmlBookMark> selectedBookMarks, XDocument xDoc)
        {
            IEnumerable<XElement> selectXml = xDoc.Descendants("KeysAndValues");
            bool isThere = false;
            while (true)
            {
                foreach (XElement xBm in selectXml)
                {
                    isThere = xBm.Descendants("Key").Select(node => selectedBookMarks.Any(p => p.Customer == node.Value)).FirstOrDefault();
                    if (!isThere)
                    {
                        xBm.Remove();
                        break;
                    }
                }
                if (isThere) // All done.
                {
                    break;
                }
            }
            return xDoc;
        }


        /// <summary>
        /// Read the XML data for the bookmark and the key's
        /// </summary>
        /// <param name="fileName">XML Input file name</param>
        /// <returns>A list of selected XML tags </returns>    
        public List<XmlBookMark> ReadXmlData(string fileName)
        {
            XDocument xDoc = XDocument.Load(fileName);
            var elementsToUpdate = xDoc.Descendants().Where(o => (o.Name == "BookmarkUTC" ||
                                                                    o.Name == "Key" || 
                                                                    o.Name == "LastUpdateDateTime" || 
                                                                    o.Name == "LastFilesProcessed" || 
                                                                    o.Name == "LastFilesErrorneous" || 
                                                                    o.Name == "TotalFilesProcessed") && !o.HasElements);
            List<XmlBookMark> xmlBookMarkList = LoadXmlBookMarksIntoAList(elementsToUpdate);
            return xmlBookMarkList;
        }

        /// <summary>
        /// Load the selected XML data into to a list
        /// </summary>
        /// <param name="elementsToUpdate"></param>
        /// <returns>The full list of all bookmarks and the customers</returns>
        private List<XmlBookMark> LoadXmlBookMarksIntoAList(IEnumerable<XElement> elementsToUpdate)
        {
            List<XmlBookMark> xmlBookMarkList = new List<XmlBookMark>();
            XmlBookMark xmlBookMark = new XmlBookMark();
            foreach (XElement element in elementsToUpdate)
            {
                switch (element.Name.ToString())
                {
                    case "BookmarkUTC":
                    {
                        xmlBookMark.BookmarkUtc = DateTime.Parse(element.Value);
                        break;
                    }
                    case "Key":
                    {
                        xmlBookMark.Customer = element.Value;
                        break;
                    }
                    case "LastUpdateDateTime":
                    {
                        xmlBookMark.LastUpdateDateTime = DateTime.Parse(element.Value);
                        break;
                    }
                    case "LastFilesProcessed":
                        xmlBookMark.LastFilesProcessed = element.Value;
                        break;
                    case "LastFilesErrorneous":
                        xmlBookMark.LastFilesErrorneous = element.Value;
                        break;
                    case "TotalFilesProcessed":
                        xmlBookMark.TotalFilesProcessed = element.Value;
                        xmlBookMarkList.Add(xmlBookMark);
                        xmlBookMark = new XmlBookMark();
                        break;
                }
            }
            return xmlBookMarkList;
        }
    }
}
