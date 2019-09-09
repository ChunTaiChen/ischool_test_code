using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports
{
    public partial class SelectTypeForm : BaseForm
    {
        private string _preferenceElementName;
        private BackgroundWorker _BGWAbsenceAndPeriodList;

        private List<string> typeList = new List<string>();
        private List<string> absenceList = new List<string>();

        public SelectTypeForm(string name)
        {
            InitializeComponent();

            _preferenceElementName = name;

            _BGWAbsenceAndPeriodList = new BackgroundWorker();
            _BGWAbsenceAndPeriodList.DoWork += new DoWorkEventHandler(_BGWAbsenceAndPeriodList_DoWork);
            _BGWAbsenceAndPeriodList.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWAbsenceAndPeriodList_RunWorkerCompleted);
            _BGWAbsenceAndPeriodList.RunWorkerAsync();
        }

        void _BGWAbsenceAndPeriodList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {            
            System.Windows.Forms.DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.HeaderText = "節次分類";
            colName.MinimumWidth = 70;
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colName.Width = 70;
            this.dataGridViewX1.Columns.Add(colName);

            foreach (string absence in absenceList)
            {
                System.Windows.Forms.DataGridViewCheckBoxColumn newCol=new DataGridViewCheckBoxColumn();
                newCol.HeaderText = absence;
                newCol.Width = 55;
                newCol.ReadOnly = false;
                newCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                newCol.Tag = absence ;
                newCol.ValueType=typeof(bool);
                this.dataGridViewX1.Columns.Add(newCol);
            }
            foreach (string type in typeList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewX1, type);
                row.Tag = type;
                dataGridViewX1.Rows.Add(row);
            }

            #region 讀取列印設定 Preference

            XmlElement config = CurrentUser.Instance.Preference[_preferenceElementName];
            if (config != null)
            {
                #region 已有設定檔則將設定檔內容填回畫面上
                foreach (XmlElement type in config.SelectNodes("Type"))
                {
                    string typeName = type.GetAttribute("Text");
                    foreach (DataGridViewRow row in dataGridViewX1.Rows)
                    {
                        if (typeName == ("" + row.Tag))
                        {
                            foreach (XmlElement absence in type.SelectNodes("Absence"))
                            {
                                string absenceName = absence.GetAttribute("Text");
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    if (cell.OwningColumn is DataGridViewCheckBoxColumn && ("" + cell.OwningColumn.Tag) == absenceName)
                                    {
                                        cell.Value = true;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region 產生空白設定檔
                config = new XmlDocument().CreateElement(_preferenceElementName);
                CurrentUser.Instance.Preference[_preferenceElementName] = config;
                #endregion
            }

            #endregion
        }

        void _BGWAbsenceAndPeriodList_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (XmlElement var in SmartSchool.Feature.Basic.Config.GetPeriodList().GetContent().GetElements("Period"))
            {
                if (!typeList.Contains(var.GetAttribute("Type")))
                    typeList.Add(var.GetAttribute("Type"));
            }

            foreach (XmlElement var in SmartSchool.Feature.Basic.Config.GetAbsenceList().GetContent().GetElements("Absence"))
            {
                if (!absenceList.Contains(var.GetAttribute("Name")))
                    absenceList.Add(var.GetAttribute("Name"));
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (!CheckColumnNumber())
                return;

            #region 更新列印設定 Preference

            XmlElement config = CurrentUser.Instance.Preference[_preferenceElementName];

            if (config == null)
            {
                config = new XmlDocument().CreateElement(_preferenceElementName);
            }

            foreach (XmlElement var in config.SelectNodes("Type"))
                config.RemoveChild(var);

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                bool needToAppend = false;
                XmlElement type = config.OwnerDocument.CreateElement("Type");
                type.SetAttribute("Text", "" + row.Tag);
                foreach (DataGridViewCell cell in row.Cells)
                {
                    XmlElement absence = config.OwnerDocument.CreateElement("Absence");
                    absence.SetAttribute("Text", ""+cell.OwningColumn.Tag);
                    if (cell.Value is bool && ((bool)cell.Value))
                    {
                        needToAppend = true;
                        type.AppendChild(absence);
                    }
                }
                if(needToAppend)
                    config.AppendChild(type);
            }

            CurrentUser.Instance.Preference[_preferenceElementName] = config;

            #endregion

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        internal bool CheckColumnNumber()
        {
            int limit = 253;
            int columnNumber = 0;
            int block = 9;

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value is bool &&((bool)cell.Value ))
                        columnNumber++;
                }
            }

            if (columnNumber * block > limit)
            {
                MsgBox.Show("您所選擇的假別超出 Excel 的最大欄位，請減少部分假別");
                return false;
            }
            else
                return true;
        }

        private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
    }
}