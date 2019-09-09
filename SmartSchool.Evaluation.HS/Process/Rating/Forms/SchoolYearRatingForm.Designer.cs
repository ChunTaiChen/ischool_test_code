namespace SmartSchool.Evaluation.Process.Rating
{
    partial class SchoolYearRatingForm 
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
            this.groupPanel1 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.chkYearMoral = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkYearScore = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkYearSubjectScore = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.groupPanel2 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.chkSequence = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkUnSequence = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.groupPanel3 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.chkGrade3 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkGrade2 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkGrade1 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.btnExit = new DevComponents.DotNetBar.ButtonX();
            this.btnRank = new DevComponents.DotNetBar.ButtonX();
            this.intSchoolYear = new DevComponents.Editors.IntegerInput();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.groupPanel1.SuspendLayout();
            this.groupPanel2.SuspendLayout();
            this.groupPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.intSchoolYear)).BeginInit();
            this.SuspendLayout();
            // 
            // groupPanel1
            // 
            this.groupPanel1.BackColor = System.Drawing.Color.Transparent;
            this.groupPanel1.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel1.Controls.Add(this.chkYearMoral);
            this.groupPanel1.Controls.Add(this.chkYearScore);
            this.groupPanel1.Controls.Add(this.chkYearSubjectScore);
            this.groupPanel1.Controls.Add(this.labelX2);
            this.groupPanel1.Controls.Add(this.labelX1);
            this.groupPanel1.Location = new System.Drawing.Point(12, 52);
            this.groupPanel1.Name = "groupPanel1";
            this.groupPanel1.Size = new System.Drawing.Size(166, 251);
            // 
            // 
            // 
            this.groupPanel1.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanel1.Style.BackColorGradientAngle = 90;
            this.groupPanel1.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanel1.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderBottomWidth = 1;
            this.groupPanel1.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanel1.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderLeftWidth = 1;
            this.groupPanel1.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderRightWidth = 1;
            this.groupPanel1.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderTopWidth = 1;
            this.groupPanel1.Style.CornerDiameter = 4;
            this.groupPanel1.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel1.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel1.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanel1.TabIndex = 0;
            this.groupPanel1.Text = "排名項目";
            // 
            // chkYearMoral
            // 
            this.chkYearMoral.AutoSize = true;
            this.chkYearMoral.BackColor = System.Drawing.Color.Transparent;
            this.chkYearMoral.Location = new System.Drawing.Point(14, 164);
            this.chkYearMoral.Name = "chkYearMoral";
            this.chkYearMoral.Size = new System.Drawing.Size(117, 19);
            this.chkYearMoral.TabIndex = 1;
            this.chkYearMoral.Text = "學年德行成績";
            // 
            // chkYearScore
            // 
            this.chkYearScore.AutoSize = true;
            this.chkYearScore.BackColor = System.Drawing.Color.Transparent;
            this.chkYearScore.Location = new System.Drawing.Point(14, 80);
            this.chkYearScore.Name = "chkYearScore";
            this.chkYearScore.Size = new System.Drawing.Size(117, 19);
            this.chkYearScore.TabIndex = 1;
            this.chkYearScore.Text = "學年學業成績";
            // 
            // chkYearSubjectScore
            // 
            this.chkYearSubjectScore.AutoSize = true;
            this.chkYearSubjectScore.BackColor = System.Drawing.Color.Transparent;
            this.chkYearSubjectScore.Location = new System.Drawing.Point(14, 48);
            this.chkYearSubjectScore.Name = "chkYearSubjectScore";
            this.chkYearSubjectScore.Size = new System.Drawing.Size(117, 19);
            this.chkYearSubjectScore.TabIndex = 1;
            this.chkYearSubjectScore.Text = "學年科目成績";
            // 
            // labelX2
            // 
            this.labelX2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(217)))), ((int)(((byte)(247)))));
            this.labelX2.Location = new System.Drawing.Point(3, 122);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(258, 23);
            this.labelX2.TabIndex = 0;
            this.labelX2.Text = "德行成績";
            // 
            // labelX1
            // 
            this.labelX1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(217)))), ((int)(((byte)(247)))));
            this.labelX1.Location = new System.Drawing.Point(3, 12);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(258, 23);
            this.labelX1.TabIndex = 0;
            this.labelX1.Text = "學業成績";
            // 
            // groupPanel2
            // 
            this.groupPanel2.BackColor = System.Drawing.Color.Transparent;
            this.groupPanel2.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel2.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel2.Controls.Add(this.chkSequence);
            this.groupPanel2.Controls.Add(this.chkUnSequence);
            this.groupPanel2.Location = new System.Drawing.Point(199, 52);
            this.groupPanel2.Name = "groupPanel2";
            this.groupPanel2.Size = new System.Drawing.Size(212, 102);
            // 
            // 
            // 
            this.groupPanel2.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanel2.Style.BackColorGradientAngle = 90;
            this.groupPanel2.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanel2.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderBottomWidth = 1;
            this.groupPanel2.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanel2.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderLeftWidth = 1;
            this.groupPanel2.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderRightWidth = 1;
            this.groupPanel2.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderTopWidth = 1;
            this.groupPanel2.Style.CornerDiameter = 4;
            this.groupPanel2.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel2.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel2.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanel2.TabIndex = 0;
            this.groupPanel2.Text = "排名選項";
            // 
            // chkSequence
            // 
            this.chkSequence.AutoSize = true;
            this.chkSequence.BackColor = System.Drawing.Color.Transparent;
            this.chkSequence.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.chkSequence.Location = new System.Drawing.Point(12, 44);
            this.chkSequence.Name = "chkSequence";
            this.chkSequence.Size = new System.Drawing.Size(179, 19);
            this.chkSequence.TabIndex = 1;
            this.chkSequence.Text = "接序排名(例：1.2.3.3.4)";
            // 
            // chkUnSequence
            // 
            this.chkUnSequence.AutoSize = true;
            this.chkUnSequence.BackColor = System.Drawing.Color.Transparent;
            this.chkUnSequence.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.chkUnSequence.Checked = true;
            this.chkUnSequence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUnSequence.CheckValue = "Y";
            this.chkUnSequence.Location = new System.Drawing.Point(12, 14);
            this.chkUnSequence.Name = "chkUnSequence";
            this.chkUnSequence.Size = new System.Drawing.Size(194, 19);
            this.chkUnSequence.TabIndex = 1;
            this.chkUnSequence.Text = "不接序排名(例：1.2.3.3.5)";
            // 
            // groupPanel3
            // 
            this.groupPanel3.BackColor = System.Drawing.Color.Transparent;
            this.groupPanel3.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel3.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel3.Controls.Add(this.chkGrade3);
            this.groupPanel3.Controls.Add(this.chkGrade2);
            this.groupPanel3.Controls.Add(this.chkGrade1);
            this.groupPanel3.Location = new System.Drawing.Point(199, 173);
            this.groupPanel3.Name = "groupPanel3";
            this.groupPanel3.Size = new System.Drawing.Size(212, 130);
            // 
            // 
            // 
            this.groupPanel3.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanel3.Style.BackColorGradientAngle = 90;
            this.groupPanel3.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanel3.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel3.Style.BorderBottomWidth = 1;
            this.groupPanel3.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanel3.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel3.Style.BorderLeftWidth = 1;
            this.groupPanel3.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel3.Style.BorderRightWidth = 1;
            this.groupPanel3.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel3.Style.BorderTopWidth = 1;
            this.groupPanel3.Style.CornerDiameter = 4;
            this.groupPanel3.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel3.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel3.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanel3.TabIndex = 0;
            this.groupPanel3.Text = "排名對象";
            // 
            // chkGrade3
            // 
            this.chkGrade3.AutoSize = true;
            this.chkGrade3.BackColor = System.Drawing.Color.Transparent;
            this.chkGrade3.Location = new System.Drawing.Point(12, 70);
            this.chkGrade3.Name = "chkGrade3";
            this.chkGrade3.Size = new System.Drawing.Size(71, 19);
            this.chkGrade3.TabIndex = 1;
            this.chkGrade3.Text = "三年級";
            // 
            // chkGrade2
            // 
            this.chkGrade2.AutoSize = true;
            this.chkGrade2.BackColor = System.Drawing.Color.Transparent;
            this.chkGrade2.Location = new System.Drawing.Point(12, 42);
            this.chkGrade2.Name = "chkGrade2";
            this.chkGrade2.Size = new System.Drawing.Size(71, 19);
            this.chkGrade2.TabIndex = 1;
            this.chkGrade2.Text = "二年級";
            // 
            // chkGrade1
            // 
            this.chkGrade1.AutoSize = true;
            this.chkGrade1.BackColor = System.Drawing.Color.Transparent;
            this.chkGrade1.Location = new System.Drawing.Point(12, 14);
            this.chkGrade1.Name = "chkGrade1";
            this.chkGrade1.Size = new System.Drawing.Size(71, 19);
            this.chkGrade1.TabIndex = 1;
            this.chkGrade1.Text = "一年級";
            // 
            // btnExit
            // 
            this.btnExit.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnExit.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Location = new System.Drawing.Point(333, 320);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "離開";
            // 
            // btnRank
            // 
            this.btnRank.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnRank.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnRank.Location = new System.Drawing.Point(252, 320);
            this.btnRank.Name = "btnRank";
            this.btnRank.Size = new System.Drawing.Size(75, 23);
            this.btnRank.TabIndex = 1;
            this.btnRank.Text = "排名";
            this.btnRank.Click += new System.EventHandler(this.btnRank_Click);
            // 
            // intSchoolYear
            // 
            // 
            // 
            // 
            this.intSchoolYear.BackgroundStyle.Class = "DateTimeInputBackground";
            this.intSchoolYear.Location = new System.Drawing.Point(82, 12);
            this.intSchoolYear.Name = "intSchoolYear";
            this.intSchoolYear.ShowUpDown = true;
            this.intSchoolYear.Size = new System.Drawing.Size(80, 23);
            this.intSchoolYear.TabIndex = 2;
            // 
            // labelX3
            // 
            this.labelX3.Location = new System.Drawing.Point(17, 14);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(59, 23);
            this.labelX3.TabIndex = 5;
            this.labelX3.Text = "學年度";
            // 
            // SchoolYearRatingForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(430, 355);
            this.Controls.Add(this.labelX3);
            this.Controls.Add(this.intSchoolYear);
            this.Controls.Add(this.btnRank);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.groupPanel3);
            this.Controls.Add(this.groupPanel2);
            this.Controls.Add(this.groupPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SchoolYearRatingForm";
            this.Text = "學年成績固定排名";
            this.DoubleClick += new System.EventHandler(this.RatingForm_DoubleClick);
            this.groupPanel1.ResumeLayout(false);
            this.groupPanel1.PerformLayout();
            this.groupPanel2.ResumeLayout(false);
            this.groupPanel2.PerformLayout();
            this.groupPanel3.ResumeLayout(false);
            this.groupPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.intSchoolYear)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel1;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkYearSubjectScore;
        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkYearScore;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkYearMoral;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel2;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkSequence;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkUnSequence;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel3;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkGrade2;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkGrade1;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkGrade3;
        private DevComponents.DotNetBar.ButtonX btnExit;
        private DevComponents.DotNetBar.ButtonX btnRank;
        private DevComponents.Editors.IntegerInput intSchoolYear;
        private DevComponents.DotNetBar.LabelX labelX3;
    }
}