using System.Windows.Forms;
namespace SmartSchool.Evaluation.Process.Rating
{
    partial class ResultViewForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該公開 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.lvScopes = new SmartSchool.Common.ListViewEX();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.cboScopeType = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.lvTargets = new SmartSchool.Common.ListViewEX();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.lvPlace = new SmartSchool.Common.ListViewEX();
            this.chClass = new System.Windows.Forms.ColumnHeader();
            this.chSeatNo = new System.Windows.Forms.ColumnHeader();
            this.chName = new System.Windows.Forms.ColumnHeader();
            this.chScore = new System.Windows.Forms.ColumnHeader();
            this.chPlace = new System.Windows.Forms.ColumnHeader();
            this.lvStudentPlace = new SmartSchool.Common.ListViewEX();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.column = new System.Windows.Forms.ColumnHeader();
            this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
            this.lblStudent = new DevComponents.DotNetBar.LabelX();
            this.SuspendLayout();
            // 
            // lvScopes
            // 
            this.lvScopes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lvScopes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvScopes.FullRowSelect = true;
            this.lvScopes.Location = new System.Drawing.Point(12, 44);
            this.lvScopes.Name = "lvScopes";
            this.lvScopes.Size = new System.Drawing.Size(201, 253);
            this.lvScopes.TabIndex = 0;
            this.lvScopes.UseCompatibleStateImageBehavior = false;
            this.lvScopes.View = System.Windows.Forms.View.Details;
            this.lvScopes.Click += new System.EventHandler(this.lvScopes_Click);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "名稱";
            this.columnHeader1.Width = 169;
            // 
            // cboScopeType
            // 
            this.cboScopeType.DisplayMember = "Text";
            this.cboScopeType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboScopeType.FormattingEnabled = true;
            this.cboScopeType.ItemHeight = 16;
            this.cboScopeType.Location = new System.Drawing.Point(12, 12);
            this.cboScopeType.Name = "cboScopeType";
            this.cboScopeType.Size = new System.Drawing.Size(201, 22);
            this.cboScopeType.TabIndex = 1;
            this.cboScopeType.SelectedIndexChanged += new System.EventHandler(this.cboScopeType_SelectedIndexChanged);
            // 
            // lvTargets
            // 
            this.lvTargets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lvTargets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.lvTargets.FullRowSelect = true;
            this.lvTargets.Location = new System.Drawing.Point(12, 303);
            this.lvTargets.Name = "lvTargets";
            this.lvTargets.Size = new System.Drawing.Size(201, 225);
            this.lvTargets.TabIndex = 0;
            this.lvTargets.UseCompatibleStateImageBehavior = false;
            this.lvTargets.View = System.Windows.Forms.View.Details;
            this.lvTargets.Click += new System.EventHandler(this.lvTargets_Click);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "名稱";
            this.columnHeader2.Width = 169;
            // 
            // lvPlace
            // 
            this.lvPlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lvPlace.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chClass,
            this.chSeatNo,
            this.chName,
            this.chScore,
            this.chPlace});
            this.lvPlace.FullRowSelect = true;
            this.lvPlace.Location = new System.Drawing.Point(219, 12);
            this.lvPlace.Name = "lvPlace";
            this.lvPlace.ShowItemToolTips = true;
            this.lvPlace.Size = new System.Drawing.Size(322, 518);
            this.lvPlace.TabIndex = 0;
            this.lvPlace.UseCompatibleStateImageBehavior = false;
            this.lvPlace.View = System.Windows.Forms.View.Details;
            this.lvPlace.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvPlace_ColumnClick);
            this.lvPlace.Click += new System.EventHandler(this.lvPlace_Click);
            // 
            // chClass
            // 
            this.chClass.Text = "班級";
            // 
            // chSeatNo
            // 
            this.chSeatNo.Text = "座號";
            // 
            // chName
            // 
            this.chName.Text = "姓名";
            // 
            // chScore
            // 
            this.chScore.Text = "成績";
            // 
            // chPlace
            // 
            this.chPlace.Text = "名次";
            // 
            // lvStudentPlace
            // 
            this.lvStudentPlace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvStudentPlace.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.column,
            this.columnHeader10,
            this.columnHeader11});
            this.lvStudentPlace.FullRowSelect = true;
            this.lvStudentPlace.Location = new System.Drawing.Point(547, 44);
            this.lvStudentPlace.Name = "lvStudentPlace";
            this.lvStudentPlace.ShowItemToolTips = true;
            this.lvStudentPlace.Size = new System.Drawing.Size(416, 486);
            this.lvStudentPlace.TabIndex = 0;
            this.lvStudentPlace.UseCompatibleStateImageBehavior = false;
            this.lvStudentPlace.View = System.Windows.Forms.View.Details;
            this.lvStudentPlace.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvStudentPlace_ColumnClick);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "範圍名稱(人數)";
            this.columnHeader3.Width = 175;
            // 
            // column
            // 
            this.column.Text = "成績名稱(人數)";
            this.column.Width = 105;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "成績";
            this.columnHeader10.Width = 44;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "名次";
            // 
            // lblStudent
            // 
            this.lblStudent.AutoSize = true;
            this.lblStudent.Location = new System.Drawing.Point(547, 15);
            this.lblStudent.Name = "lblStudent";
            this.lblStudent.Size = new System.Drawing.Size(134, 19);
            this.lblStudent.TabIndex = 2;
            this.lblStudent.Text = "Student Information";
            // 
            // ResultViewForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(975, 540);
            this.Controls.Add(this.lblStudent);
            this.Controls.Add(this.cboScopeType);
            this.Controls.Add(this.lvStudentPlace);
            this.Controls.Add(this.lvPlace);
            this.Controls.Add(this.lvTargets);
            this.Controls.Add(this.lvScopes);
            this.Name = "ResultViewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ResultViewForm";
            this.Load += new System.EventHandler(this.ResultViewForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ListView lvScopes;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cboScopeType;
        private ListView lvTargets;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private ListView lvPlace;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chClass;
        private System.Windows.Forms.ColumnHeader chSeatNo;
        private System.Windows.Forms.ColumnHeader chScore;
        private System.Windows.Forms.ColumnHeader chPlace;
        private ListView lvStudentPlace;
        private System.Windows.Forms.ColumnHeader column;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private DevComponents.DotNetBar.LabelX lblStudent;
    }
}