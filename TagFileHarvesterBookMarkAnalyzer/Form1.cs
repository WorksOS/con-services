using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagFileHarvester.Implementation;
using TagFileHarvester.Models;


namespace TagFileHarvesterBookMarkAnalyzer
{
  public partial class Form1 : Form
  {

    private IEnumerable<Bookmark> datasource;
    private BindingSource bs = new BindingSource();
    private DataGridView grid;

    public Form1()
    {
      InitializeComponent();
      timer1.Interval = 7000;
      timer1.Enabled = true;
      timer1.Tick += timer1_Tick;

      grid = new DataGridView();
      grid.Dock = DockStyle.Fill;
      this.Controls.Add(grid);

      // Begin watching.

      bs.DataSource = datasource;
      grid.DataSource = bs;
      grid.AutoGenerateColumns = true;
      grid.AutoSize = true;
      grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

    }

    void timer1_Tick(object sender, EventArgs e)
    {
      try
      {
        datasource = XMLBookMarkManager.Instance.GetBookmarksList();
        try
        {
          datasource.FirstOrDefault();
        }
        catch (Exception ex)
        {
          this.Invoke((MethodInvoker) delegate { this.Text = ex.Message; });
        }


        this.Invoke((MethodInvoker) delegate
                                    {
                                      bs.DataSource = datasource;
                                      grid.Invalidate();
                                      if (grid.Columns.Count > 2)
                                      {
                                        grid.Columns[0].DefaultCellStyle = new DataGridViewCellStyle
                                                                           {
                                                                               Format =
                                                                                   "dd'/'MM'/'yyyy hh:mm:ss"
                                                                           };
                                        grid.Columns[1].DefaultCellStyle = new DataGridViewCellStyle
                                                                           {
                                                                               Format =
                                                                                   "dd'/'MM'/'yyyy hh:mm:ss"
                                                                           };
                                        grid.Columns[2].DefaultCellStyle = new DataGridViewCellStyle
                                                                           {
                                                                               Format =
                                                                                   "dd'/'MM'/'yyyy hh:mm:ss"
                                                                           };
                                        grid.Columns[3].DefaultCellStyle = new DataGridViewCellStyle
                                                                           {
                                                                               Format =
                                                                                   "dd'/'MM'/'yyyy hh:mm:ss"
                                                                           };
                                      }

                                    });
        this.Invoke((MethodInvoker) delegate { this.Text = "Updated at " + DateTime.Now.ToLongTimeString(); });
      }
      catch
      {
      }
    }

  }

}


