using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Collections;

namespace WindowsFormsApplication1
{
    public partial class TMEditor : Form
    {
        const string TableName = "ResourceTable";
        const string col1Name = "col1";//needs meaningful name once
        const string col2Name = "col2";//I know what goes into them
        const int GridLineWidth = 1;
        const int NumColumns = 2;
        const string Separator = ";";

        //private DataTable resxTable;
        class Entry
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        BindingList<Entry> resxEntries = new BindingList<Entry>();

        public TMEditor()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TMEditor_Load(object sender, EventArgs e)
        {
            //Initialize everything on startup
            //Create a data table with two columns
            //this.resxTable = new DataTable(TableName);
            //this.resxTable.Columns.Add(new DataColumn(col1Name, Type.GetType("System.String")));
            //this.resxTable.Columns.Add(new DataColumn(col2Name, Type.GetType("System.DateTime")));
            //dataGrid.DataSource = resxTable;

            //Set up a better looking table 
            dataGrid.Columns[col1Name].Width = (dataGrid.Width - dataGrid.RowHeadersWidth - 2 * GridLineWidth) / NumColumns
                  - GridLineWidth;
            dataGrid.Columns[col2Name].Width = dataGrid.Columns[col1Name].Width; 
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Reset();
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.RestoreDirectory = false;
            openFileDialog1.Filter = "Resource files (*.resx)|*.resx|All files (*.*)|*.*";
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string target = openFileDialog1.FileName;
                StreamReader MyStream = new StreamReader(openFileDialog1.FileName);
                dataGrid.DataSource = null;
                //resxTable.Clear(); //Clear the existing table
                //dataGrid.DataSource = resxTable;

                Dictionary<string, string> resourceMap = new Dictionary<string, string>();
                ResXResourceReader resourceReader = new ResXResourceReader(target);
                IDictionaryEnumerator dict = resourceReader.GetEnumerator();
                foreach (DictionaryEntry d in resourceReader)
                {
                    resourceMap.Add(d.Key.ToString(), d.Value.ToString());
                    resxEntries.Add(new Entry() { Key = d.Key.ToString(), Value = d.Value.ToString() });
                }
                //dataGrid.DataSource = resourceMap.ToArray();
                dataGrid.DataSource = resxEntries;
                dataGrid.Update();
                dataGrid.Refresh();
                
                    

                //Dictionary<string, string> resourceMap = new Dictionary<string, string>();
                //string filename = "Your resource filepath";
                //ResXResourceReader rsxr = new ResXResourceReader(filename);
                //foreach (DictionaryEntry d in rsxr)
                //{
                //    resourceMap.Add(d.Key.ToString(), d.Value.ToString());
                //}
                //dataGridView1.DataSource = resourceMap.ToArray();

                ///try
                //{
                //    while (true)
                //    {
                //        String MyLine = MyStream.ReadLine();
                //        if (MyLine == null)
                //        {
                //            break;
                //        }
                //        else if (MyLine.Length != 0)
                //        {
                //            String[] fields = MyLine.Split(Separator.ToCharArray());
                //            if (fields.GetLength(0) == 2)
                //            {
                //                resxTable.Rows.Add(resxTable.NewRow());
                //                resxTable.Rows[resxTable.Rows.Count - 1][col1Name] =
                //                 fields[0].Trim();
                //                resxTable.Rows[resxTable.Rows.Count - 1][col2Name] =
                //                fields[1].Trim();
                //            }
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("Fatal Error" + ex.ToString());
                //    Application.Exit();
                //}
            }
        }

        
    }
}
