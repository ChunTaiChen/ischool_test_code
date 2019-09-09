using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Customization.Data;


namespace SmartSchool.Evaluation.Process
{
    public partial class ErrorViewer : SmartSchool.Common.BaseForm
    {
        public ErrorViewer()
        {
            InitializeComponent();
        }
        public void SetMessage(StudentRecord stu, List<string> messages)
        {
            //bool first = true;
            foreach (string msg in messages)
            {
                //2018/5/30 穎驊註記 把舊UI 填資料方式註解掉
                //string s = "";
                //if (first)
                //{
                //    if (stu.RefClass != null)
                //    {
                //        s += stu.RefClass.ClassName;
                //        if (stu.SeatNo != "")
                //            s += "(" + stu.SeatNo + "號)";
                //        s += " ";
                //    }
                //    if (stu.StudentNumber != "")
                //        s += stu.StudentNumber + " ";
                //    if (s == "")
                //        s += "學生：";
                //    s += stu.StudentName;
                //    dataGridViewX1.Rows.Add(s, msg);
                //    first = false;
                //}
                //else
                //    dataGridViewX1.Rows.Add("", msg);

                string className = "";
                if (stu.RefClass != null)
                {
                    className = stu.RefClass.ClassName;
                }

                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewX1); // 為新的row 增加cell

                row.Tag = stu.StudentID;

                row.Cells[0].Value = className;
                row.Cells[1].Value = stu.SeatNo;
                row.Cells[2].Value = stu.StudentNumber;
                row.Cells[3].Value = stu.StudentName;
                row.Cells[4].Value = msg;

                dataGridViewX1.Rows.Add(row);

            }
            toolStripStatusLabel1.Text ="總計"+dataGridViewX1.Rows.Count+"個錯誤。";
        }

        public void Clear()
        {
            //this.Hide();
            this.dataGridViewX1.Rows.Clear();
            toolStripStatusLabel1.Text = "";
        }

        // 匯出
        private void buttonX1_Click(object sender, EventArgs e)
        {            
            #region 匯出
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            DataGridViewExport export = new DataGridViewExport(dataGridViewX1);
            export.Save(saveFileDialog1.FileName);
            
            if (new CompleteForm().ShowDialog() == DialogResult.Yes)
                System.Diagnostics.Process.Start(saveFileDialog1.FileName);
            #endregion
        }

        // 加入待處理
        private void buttonX3_Click(object sender, EventArgs e)
        {
            List<string> TemporalStud = new List<string>();
            foreach (DataGridViewRow var in dataGridViewX1.SelectedRows)
            {
                if (!TemporalStud.Contains("" + var.Tag))
                {
                    TemporalStud.Add("" + var.Tag);
                }
            }
            
            K12.Presentation.NLDPanels.Student.AddToTemp(TemporalStud);
            FISCA.Presentation.Controls.MsgBox.Show("新增 " + TemporalStud.Count + " 名學生於待處理");
            
        }
    }
}