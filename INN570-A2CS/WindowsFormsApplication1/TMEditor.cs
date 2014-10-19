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
using System.Xml;

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

            openFileDialog1.Reset();
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.RestoreDirectory = false;
            openFileDialog1.Filter = "Resource files (*.resx)|*.resx|TM files (*.tmx)|*.tmx|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileName.Contains(".resx"))
                {
                    BindingList<Entry> resxEntries = new BindingList<Entry>();
                    string target = openFileDialog1.FileName;
                    string[] resxCulture = target.Split('.');
                    if (resxCulture.Length > 1)
                    {
                        lblResxCulture.Text = resxCulture[resxCulture.Length - 2];
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
                //Check a resx has been loaded
                else if (dataGrid.Rows[0].Cells[0].Value.ToString() != null)
                {
                    //csv loaded
                    XmlDocument doc = new XmlDocument();
                    doc.Load(openFileDialog1.FileName);

                    // }
                    XmlNodeList nodelist = doc.SelectNodes("/tmx/body/tu/tuv");
                    //for each entry in column 1
                    foreach (DataGridViewRow row in dataGrid.Rows)
                    {
                        for (int i = 0; i < nodelist.Count; i++)
                        {
                            //find equivalent
                            if (nodelist[i].SelectSingleNode("seg").InnerText == row.Cells[0].Value.ToString())
                            {
                                //Match found, set third column value
                                row.Cells[2].Value = nodelist[i + 1].SelectSingleNode("seg").InnerText;
                            }
                        }
                        //Check if no match was found
                        if (row.Cells[2].Value == null)
                            row.Cells[2].Value = "Undefined";
                    }
                    dataGrid.Refresh();
                }
            }

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Reset();
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog1.RestoreDirectory = false;
            var savetype = cboCulture.Text.Split(' ');
            saveFileDialog1.Filter = string.Format("Resource files (*.{0}.resx)|*.{0}.resx| All files (*.*)|*.*", savetype[0]);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ResXResourceWriter writer = new ResXResourceWriter(saveFileDialog1.FileName);
                dataGrid.EndEdit(); //Finalises latest entry
                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    if ((row.Cells[0].Value != null && row.Cells[1].Value != null))
                    {
                        writer.AddResource(new ResXDataNode(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString()));
                    }
                    //TODO else error
                }
                writer.Generate();
                writer.Close();
            }
        }

        private void saveTMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Reset();
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog1.RestoreDirectory = false;
            var savetype = cboCulture.Text.Split(' ');
            saveFileDialog1.Filter = string.Format("TM files (*.tmx)|*.tmx| All files (*.*)|*.*", savetype[0]);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement tmx = doc.CreateElement("tmx");
                XmlAttribute version = doc.CreateAttribute("version");
                tmx.Attributes.Append(version);
                tmx.Attributes[0].Value = "1.4";
                //Code for header
                XmlElement body = doc.CreateElement("body");
                tmx.AppendChild(body);
                int i = 0;
                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    if (row.Cells[2].Value != null)
                    {
                        i++;
                        XmlElement tu = doc.CreateElement("tu");
                        XmlElement tuv = doc.CreateElement("tuv");
                        XmlElement tuvTM = doc.CreateElement("tuv");
                        XmlElement seg = doc.CreateElement("seg");
                        XmlElement segTM = doc.CreateElement("seg");

                        XmlAttribute tuid = doc.CreateAttribute("tuid");
                        tu.Attributes.Append(tuid);
                        tu.Attributes[0].Value = i.ToString();
                        XmlAttribute lang = doc.CreateAttribute("xml:lang");
                        tuv.Attributes.Append(lang);
                        tuv.Attributes[0].Value = lblResxCulture.Text;
                        tuvTM.Attributes.Append(lang);
                        tuvTM.Attributes[0].Value = savetype[0];
                        segTM.InnerText = row.Cells[2].Value.ToString();
                        seg.InnerText = row.Cells[1].Value.ToString();

                        body.AppendChild(tu);
                        tu.AppendChild(tuv);
                        tuv.AppendChild(seg);
                        tu.AppendChild(tuvTM);
                        tuvTM.AppendChild(segTM);
                    }
                }
                doc.AppendChild(tmx);
                doc.Save(saveFileDialog1.FileName);
            }
        }
    }
}
