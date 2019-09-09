namespace SmartSchool.Evaluation.Process.Wizards
{
    partial class CreateClassCourse
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
            if ( disposing && ( components != null ) )
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateClassCourse));
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.txtSubject = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtLevel = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.labelX13 = new DevComponents.DotNetBar.LabelX();
            this.labelX8 = new DevComponents.DotNetBar.LabelX();
            this.labelX11 = new DevComponents.DotNetBar.LabelX();
            this.cboEntry = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.comboItem15 = new DevComponents.Editors.ComboItem();
            this.comboItem16 = new DevComponents.Editors.ComboItem();
            this.cboRequiredBy = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.comboItem24 = new DevComponents.Editors.ComboItem();
            this.comboItem25 = new DevComponents.Editors.ComboItem();
            this.cboRequired = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.comboItem21 = new DevComponents.Editors.ComboItem();
            this.comboItem20 = new DevComponents.Editors.ComboItem();
            this.labelX7 = new DevComponents.DotNetBar.LabelX();
            this.txtCredit = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.buttonX1 = new DevComponents.DotNetBar.ButtonX();
            this.buttonX2 = new DevComponents.DotNetBar.ButtonX();
            this.cboSemester = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.comboItem1 = new DevComponents.Editors.ComboItem();
            this.comboItem2 = new DevComponents.Editors.ComboItem();
            this.cboSchoolYear = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.labelX4 = new DevComponents.DotNetBar.LabelX();
            this.SuspendLayout();
            // 
            // labelX1
            // 
            this.labelX1.AutoSize = true;
            this.labelX1.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.Class = "";
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Location = new System.Drawing.Point(21, 41);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(47, 21);
            this.labelX1.TabIndex = 0;
            this.labelX1.Text = "科目：";
            // 
            // labelX2
            // 
            this.labelX2.AutoSize = true;
            this.labelX2.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX2.BackgroundStyle.Class = "";
            this.labelX2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX2.Location = new System.Drawing.Point(235, 41);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(47, 21);
            this.labelX2.TabIndex = 0;
            this.labelX2.Text = "級別：";
            // 
            // txtSubject
            // 
            // 
            // 
            // 
            this.txtSubject.Border.Class = "TextBoxBorder";
            this.txtSubject.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtSubject.Location = new System.Drawing.Point(74, 39);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(125, 25);
            this.txtSubject.TabIndex = 2;
            this.txtSubject.TextChanged += new System.EventHandler(this.txtSubject_TextChanged);
            // 
            // txtLevel
            // 
            // 
            // 
            // 
            this.txtLevel.Border.Class = "TextBoxBorder";
            this.txtLevel.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtLevel.Location = new System.Drawing.Point(288, 39);
            this.txtLevel.Name = "txtLevel";
            this.txtLevel.Size = new System.Drawing.Size(125, 25);
            this.txtLevel.TabIndex = 3;
            this.txtLevel.TextChanged += new System.EventHandler(this.checkIsInteger);
            // 
            // labelX13
            // 
            this.labelX13.AutoSize = true;
            this.labelX13.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX13.BackgroundStyle.Class = "";
            this.labelX13.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX13.Location = new System.Drawing.Point(8, 103);
            this.labelX13.Name = "labelX13";
            this.labelX13.Size = new System.Drawing.Size(60, 21);
            this.labelX13.TabIndex = 25;
            this.labelX13.Text = "校部訂：";
            // 
            // labelX8
            // 
            this.labelX8.AutoSize = true;
            this.labelX8.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX8.BackgroundStyle.Class = "";
            this.labelX8.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX8.Location = new System.Drawing.Point(222, 103);
            this.labelX8.Name = "labelX8";
            this.labelX8.Size = new System.Drawing.Size(60, 21);
            this.labelX8.TabIndex = 30;
            this.labelX8.Text = "必選修：";
            // 
            // labelX11
            // 
            this.labelX11.AutoSize = true;
            this.labelX11.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX11.BackgroundStyle.Class = "";
            this.labelX11.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX11.Location = new System.Drawing.Point(208, 72);
            this.labelX11.Name = "labelX11";
            this.labelX11.Size = new System.Drawing.Size(74, 21);
            this.labelX11.TabIndex = 22;
            this.labelX11.Text = "分項類別：";
            // 
            // cboEntry
            // 
            this.cboEntry.DisplayMember = "Text";
            this.cboEntry.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboEntry.FormattingEnabled = true;
            this.cboEntry.ItemHeight = 19;
            this.cboEntry.Items.AddRange(new object[] {
            this.comboItem15,
            this.comboItem16});
            this.cboEntry.Location = new System.Drawing.Point(288, 70);
            this.cboEntry.MaxDropDownItems = 6;
            this.cboEntry.Name = "cboEntry";
            this.cboEntry.Size = new System.Drawing.Size(125, 25);
            this.cboEntry.TabIndex = 5;
            this.cboEntry.Tag = "ForceValidate";
            this.cboEntry.TextChanged += new System.EventHandler(this.checkInItem);
            // 
            // comboItem15
            // 
            this.comboItem15.Text = "是";
            // 
            // comboItem16
            // 
            this.comboItem16.Text = "否";
            // 
            // cboRequiredBy
            // 
            this.cboRequiredBy.DisplayMember = "Text";
            this.cboRequiredBy.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboRequiredBy.FormattingEnabled = true;
            this.cboRequiredBy.ItemHeight = 19;
            this.cboRequiredBy.Items.AddRange(new object[] {
            this.comboItem24,
            this.comboItem25});
            this.cboRequiredBy.Location = new System.Drawing.Point(74, 101);
            this.cboRequiredBy.MaxDropDownItems = 6;
            this.cboRequiredBy.Name = "cboRequiredBy";
            this.cboRequiredBy.Size = new System.Drawing.Size(125, 25);
            this.cboRequiredBy.TabIndex = 6;
            this.cboRequiredBy.Tag = "ForceValidate";
            this.cboRequiredBy.TextChanged += new System.EventHandler(this.checkInItem);
            // 
            // comboItem24
            // 
            this.comboItem24.Text = "校訂";
            // 
            // comboItem25
            // 
            this.comboItem25.Text = "部訂";
            // 
            // cboRequired
            // 
            this.cboRequired.DisplayMember = "Text";
            this.cboRequired.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboRequired.FormattingEnabled = true;
            this.cboRequired.ItemHeight = 19;
            this.cboRequired.Items.AddRange(new object[] {
            this.comboItem21,
            this.comboItem20});
            this.cboRequired.Location = new System.Drawing.Point(288, 101);
            this.cboRequired.MaxDropDownItems = 6;
            this.cboRequired.Name = "cboRequired";
            this.cboRequired.Size = new System.Drawing.Size(125, 25);
            this.cboRequired.TabIndex = 7;
            this.cboRequired.Tag = "ForceValidate";
            this.cboRequired.TextChanged += new System.EventHandler(this.checkInItem);
            // 
            // comboItem21
            // 
            this.comboItem21.Text = "選修";
            // 
            // comboItem20
            // 
            this.comboItem20.Text = "必修";
            // 
            // labelX7
            // 
            this.labelX7.AutoSize = true;
            this.labelX7.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX7.BackgroundStyle.Class = "";
            this.labelX7.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX7.Location = new System.Drawing.Point(8, 72);
            this.labelX7.Name = "labelX7";
            this.labelX7.Size = new System.Drawing.Size(60, 21);
            this.labelX7.TabIndex = 28;
            this.labelX7.Text = "學分數：";
            // 
            // txtCredit
            // 
            // 
            // 
            // 
            this.txtCredit.Border.Class = "TextBoxBorder";
            this.txtCredit.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtCredit.Location = new System.Drawing.Point(74, 70);
            this.txtCredit.Name = "txtCredit";
            this.txtCredit.Size = new System.Drawing.Size(125, 25);
            this.txtCredit.TabIndex = 4;
            this.txtCredit.TextChanged += new System.EventHandler(this.checkIsdecimal);
            // 
            // buttonX1
            // 
            this.buttonX1.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX1.BackColor = System.Drawing.Color.Transparent;
            this.buttonX1.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX1.Enabled = false;
            this.buttonX1.Location = new System.Drawing.Point(267, 144);
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.Size = new System.Drawing.Size(75, 25);
            this.buttonX1.TabIndex = 8;
            this.buttonX1.Text = "確定";
            this.buttonX1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // buttonX2
            // 
            this.buttonX2.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX2.BackColor = System.Drawing.Color.Transparent;
            this.buttonX2.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonX2.Location = new System.Drawing.Point(348, 144);
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.Size = new System.Drawing.Size(75, 25);
            this.buttonX2.TabIndex = 9;
            this.buttonX2.Text = "離開";
            this.buttonX2.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // cboSemester
            // 
            this.cboSemester.DisplayMember = "Text";
            this.cboSemester.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboSemester.FormattingEnabled = true;
            this.cboSemester.ItemHeight = 19;
            this.cboSemester.Items.AddRange(new object[] {
            this.comboItem1,
            this.comboItem2});
            this.cboSemester.Location = new System.Drawing.Point(288, 8);
            this.cboSemester.Name = "cboSemester";
            this.cboSemester.Size = new System.Drawing.Size(125, 25);
            this.cboSemester.TabIndex = 1;
            this.cboSemester.TextChanged += new System.EventHandler(this.checkSemester);
            // 
            // comboItem1
            // 
            this.comboItem1.Text = "1";
            // 
            // comboItem2
            // 
            this.comboItem2.Text = "2";
            // 
            // cboSchoolYear
            // 
            this.cboSchoolYear.DisplayMember = "Text";
            this.cboSchoolYear.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboSchoolYear.FormattingEnabled = true;
            this.cboSchoolYear.ItemHeight = 19;
            this.cboSchoolYear.Location = new System.Drawing.Point(74, 8);
            this.cboSchoolYear.Name = "cboSchoolYear";
            this.cboSchoolYear.Size = new System.Drawing.Size(125, 25);
            this.cboSchoolYear.TabIndex = 0;
            this.cboSchoolYear.TextChanged += new System.EventHandler(this.checkSchoolYear);
            // 
            // labelX3
            // 
            this.labelX3.AutoSize = true;
            this.labelX3.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX3.BackgroundStyle.Class = "";
            this.labelX3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX3.Location = new System.Drawing.Point(235, 10);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(47, 21);
            this.labelX3.TabIndex = 31;
            this.labelX3.Text = "學期：";
            // 
            // labelX4
            // 
            this.labelX4.AutoSize = true;
            this.labelX4.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX4.BackgroundStyle.Class = "";
            this.labelX4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX4.Location = new System.Drawing.Point(8, 10);
            this.labelX4.Name = "labelX4";
            this.labelX4.Size = new System.Drawing.Size(60, 21);
            this.labelX4.TabIndex = 32;
            this.labelX4.Text = "學年度：";
            // 
            // CreateClassCourse
            // 
            this.AcceptButton = this.buttonX1;
            this.CancelButton = this.buttonX2;
            this.ClientSize = new System.Drawing.Size(433, 180);
            this.Controls.Add(this.cboSemester);
            this.Controls.Add(this.cboSchoolYear);
            this.Controls.Add(this.labelX3);
            this.Controls.Add(this.labelX4);
            this.Controls.Add(this.buttonX2);
            this.Controls.Add(this.buttonX1);
            this.Controls.Add(this.labelX13);
            this.Controls.Add(this.labelX8);
            this.Controls.Add(this.labelX11);
            this.Controls.Add(this.cboEntry);
            this.Controls.Add(this.cboRequiredBy);
            this.Controls.Add(this.cboRequired);
            this.Controls.Add(this.labelX7);
            this.Controls.Add(this.txtCredit);
            this.Controls.Add(this.txtLevel);
            this.Controls.Add(this.txtSubject);
            this.Controls.Add(this.labelX2);
            this.Controls.Add(this.labelX1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CreateClassCourse";
            this.Text = "班級開課";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.Controls.TextBoxX txtSubject;
        private DevComponents.DotNetBar.Controls.TextBoxX txtLevel;
        private DevComponents.DotNetBar.LabelX labelX13;
        private DevComponents.DotNetBar.LabelX labelX8;
        private DevComponents.DotNetBar.LabelX labelX11;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cboEntry;
        private DevComponents.Editors.ComboItem comboItem15;
        private DevComponents.Editors.ComboItem comboItem16;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cboRequiredBy;
        private DevComponents.Editors.ComboItem comboItem24;
        private DevComponents.Editors.ComboItem comboItem25;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cboRequired;
        private DevComponents.Editors.ComboItem comboItem20;
        private DevComponents.Editors.ComboItem comboItem21;
        private DevComponents.DotNetBar.LabelX labelX7;
        protected DevComponents.DotNetBar.Controls.TextBoxX txtCredit;
        private DevComponents.DotNetBar.ButtonX buttonX1;
        private DevComponents.DotNetBar.ButtonX buttonX2;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cboSemester;
        private DevComponents.Editors.ComboItem comboItem1;
        private DevComponents.Editors.ComboItem comboItem2;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cboSchoolYear;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.LabelX labelX4;
    }
}