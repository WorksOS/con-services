using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BookMark
{
    public partial class BookmarkFrm : Form
    {
        private readonly OpenFileDialog ofd = new OpenFileDialog();
        private string saveFileName = string.Empty;
        private List<XmlBookMark> xmlBookMarkList = new List<XmlBookMark>();
        private DateTime calendarSelected;

        public BookmarkFrm()
        {
            InitializeComponent();
            monthCalendar.Hide();                     
            btnUpdate.Hide();
            grpSave.Hide();
            lbSuccess.Text = string.Empty;
        }

        #region Private methods
        /// <summary>
        /// Read the XML file
        /// </summary>
        /// <returns>File name</returns>
        private string OpenTheBookmarkInputXmlFile()
        {
            ofd.Filter = @"XML file | *.xml";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                return ofd.FileName;
            }
            return string.Empty;
        }

        /// <summary>
        /// Save file dialog to select a folder and file name to save as.
        /// </summary>
        /// <returns>Return a Xml file to save as </returns>
        private string GetTheFileNameForTheOutputXmlFile()
        {
            var saveFileDialog = new SaveFileDialog { Filter = @"XML file | *.xml" };
            saveFileDialog.ShowDialog();
            return saveFileDialog.FileName != string.Empty ? saveFileDialog.FileName : string.Empty;
        }

        /// <summary>
        /// Read XML file then load into datagrid
        /// </summary>
        /// <param name="fileName">File name that needs to be loaded</param>
        private void LoadDataGridWithXmlData(string fileName)
        {
            var xmlfile = new XmlFile();
            xmlBookMarkList = xmlfile.ReadXmlData(fileName);
            LoadDataGridWithBookMarks(xmlBookMarkList);
        }

        /// <summary>
        /// Load the datagrid with book marks
        /// </summary>
        private void LoadDataGridWithBookMarks(List<XmlBookMark> inListofBookMarks)
        {
            dgvBookmarks.Rows.Clear();
            dgvBookmarks.Refresh();
            foreach (var xmlitem in inListofBookMarks)
            {
                dgvBookmarks.Rows.Add(false, 
                                        xmlitem.Customer, 
                                        xmlitem.BookmarkUtc, 
                                        xmlitem.LastUpdateDateTime,
                                        xmlitem.LastFilesProcessed,
                                        xmlitem.LastFilesErrorneous,
                                        xmlitem.TotalFilesProcessed
                                        );
            }
        }

        /// <summary>
        /// Add all the selected bookmarks from the data grid to a list
        /// </summary>
        /// <returns>List of organisationsa and bookmarks</returns>
        private List<XmlBookMark> BuildSelectedBookmarksList()
        {
            List<XmlBookMark> bookMarkList = new List<XmlBookMark>();
            for (var counter = 0; counter < (dgvBookmarks.Rows.Count); counter++)
            {
                var cbxCell = dgvBookmarks.Rows[counter].Cells["ColSelect"] as DataGridViewCheckBoxCell;
                if (cbxCell == null) 
                    {continue;}

                if (!(bool) cbxCell.EditedFormattedValue) 
                    {continue;}

                var xmlBookMark = new XmlBookMark
                {
                    Customer = dgvBookmarks.Rows[counter].Cells["Organisation"].Value.ToString(),
                    BookmarkUtc = Convert.ToDateTime(dgvBookmarks.Rows[counter].Cells["BookMark"].Value),
                    LastUpdateDateTime = Convert.ToDateTime(dgvBookmarks.Rows[counter].Cells["LastUpdated"].Value)
                };
                bookMarkList.Add(xmlBookMark);
            }
            return bookMarkList;
        }

        /// <summary>
        /// Update the book marks in the XML file that have been selected in the data grid 
        /// </summary>
        /// <param name="selectedBookmarks">List of book marks</param>
        /// <returns>The number of XML bookmarks updated</returns>
        private int UpdateTheBookMarksInFileWithTheSelectedOnes(List<XmlBookMark> selectedBookmarks)
        {
            XmlFile xmlfile = new XmlFile();
            var xmlDocument = xmlfile.UpdateSelectedBookMarksXmlDataWithNewDateTheSaveFullFile(calendarSelected, ofd.FileName, selectedBookmarks);
            saveFileName = GetTheFileNameForTheOutputXmlFile();
            if (string.IsNullOrEmpty(saveFileName))
            {
                return 0;
            }
            xmlDocument.Save(saveFileName);
            return selectedBookmarks.Count;
        }

        /// <summary>
        /// Update and save XML file
        /// </summary>
        /// <returns>True or false</returns>
        private bool UpdateBookmarksAndSaveXmlFile()
        {
            XmlFile xmlfile = new XmlFile();
            var xmlDocument = xmlfile.UpdateAllBookMarksXmlDataWithNewDate(calendarSelected, ofd.FileName);
            saveFileName = GetTheFileNameForTheOutputXmlFile();
            if (string.IsNullOrEmpty(saveFileName))
            {
                return false;
            }
            xmlDocument.Save(saveFileName);
            return true;
        }
        #endregion 

        #region Events
        /// <summary>
        /// Browse for a XML input file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var fileName = OpenTheBookmarkInputXmlFile();
            if (fileName == string.Empty)
                { return; }
            LoadDataGridWithXmlData(fileName);
            monthCalendar.Show();
        }

        /// <summary>
        /// Update the a new xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            int updatedCount = 0;
            if (rbtAll.Checked)
            {
                if (!UpdateBookmarksAndSaveXmlFile()) 
                   {return;}
            }
            if (rbtSelected.Checked)
            {
                List<XmlBookMark> selectedBookmarks = BuildSelectedBookmarksList();
                if (selectedBookmarks.Count == 0)
                {
                    MessageBox.Show(@"Are you trying to update selected bookmarks? You need to select some", @"Nothing selected", MessageBoxButtons.OK, MessageBoxIcon.Error);                       
                    return;
                }
                updatedCount = UpdateTheBookMarksInFileWithTheSelectedOnes(selectedBookmarks);
                if (updatedCount == 0)
                   { return; }
            }

            // Updated Data grid view
            dgvBookmarks.Rows.Clear();
            dgvBookmarks.Refresh();
            LoadDataGridWithXmlData(saveFileName);
            lbSuccess.Text = updatedCount + @" Bookmarks updated to " + calendarSelected.ToShortDateString();
        }

        /// <summary>
        /// Select a date from the calendar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monthCalendar_MouseDown(object sender, MouseEventArgs e)
        {
            calendarSelected = monthCalendar.SelectionStart;
            lbSuccess.Text = @"Date selected: " + calendarSelected.ToShortDateString();
            btnUpdate.Show();
            grpSave.Show();
        }
 
        private void dgvBookmarks_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                foreach (DataGridViewRow row in dgvBookmarks.SelectedRows)
                {
                    try
                    {
                        if (!Convert.ToBoolean(row.Cells[0].Value))
                        {
                            row.Cells[0].Value = true;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
        #endregion
    }
}
