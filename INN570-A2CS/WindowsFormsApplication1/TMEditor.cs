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
using System.Globalization;

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
            //Set up a better looking table 
            //dataGrid.Columns[col1Name].Width = (dataGrid.Width - dataGrid.RowHeadersWidth - 2 * GridLineWidth) / NumColumns
            //      - GridLineWidth;
            //dataGrid.Columns[col2Name].Width = dataGrid.Columns[col1Name].Width;
            CultureInfo[] list;
            String name;
            //Get a list of cultures - either all or installed
            list = CultureInfo.GetCultures(CultureTypes.AllCultures);
            
                //list = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);

            //Fill the combo boxes
            cboCulture.Items.Clear();

            //There is a name and a display name
            foreach (CultureInfo ci in list)
            {
                //bind the names together and show for culture
                name = ci.Name + " " + ci.DisplayName;
                cboCulture.Items.Add(name);
            }

            cboCulture.SelectedIndex = 0;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BindingList<Entry> resxEntries = new BindingList<Entry>();
            openFileDialog1.Reset();
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.RestoreDirectory = false;
            openFileDialog1.Filter = "Resource files (*.resx)|*.resx|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string target = openFileDialog1.FileName;
                string[] resxCulture = target.Split('.');
                if(resxCulture.Length == 3){
                    lblResxCulture.Text = resxCulture[1];
                }
                else lblResxCulture.Text = "Undefined";//filters based on filename
                dataGrid.DataSource = null;
                dataGrid.RowCount = 0;
                dataGrid.ColumnCount = 0;
                dataGrid.AutoGenerateColumns = true;

                Dictionary<string, string> resourceMap = new Dictionary<string, string>();
                ResXResourceReader resourceReader = new ResXResourceReader(target);
                foreach (DictionaryEntry d in resourceReader)
                {
                    resxEntries.Add(new Entry() { Key = d.Key.ToString(), Value = d.Value.ToString() });
                }
                    dataGrid.DataSource = resxEntries;
                    //Add third colum for user to select translations
                    var col3 = new DataGridViewTextBoxColumn();
                    dataGrid.Columns.Add(new DataGridViewTextBoxColumn());
                    dataGrid.Columns[2].Name = "Target";
                    dataGrid.Update();
                    dataGrid.Refresh();
                    
            }


        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Reset();
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog1.RestoreDirectory = false;
            var savetype = cboCulture.Text.Split(' ');
            saveFileDialog1.Filter = string.Format("Resource files (*.{0}.resx)|*.{0}.resx", savetype[0]);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ResXResourceWriter writer = new ResXResourceWriter(saveFileDialog1.FileName);
                dataGrid.EndEdit(); //Finalises latest entry
                        foreach (DataGridViewRow row in dataGrid.Rows)
                    {
                            if((row.Cells[0].Value.ToString() != null || row.Cells[2].Value.ToString() != null))
                            {
                                writer.AddResource(new ResXDataNode(row.Cells[0].Value.ToString(), row.Cells[2].Value.ToString()));
                            }
                            //TODO else error
                      }
                writer.Generate();
                writer.Close();
            }
        }

    }
}
