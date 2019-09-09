namespace SmartSchool.Evaluation.Process.Wizards
{
    partial class CalcSchoolYearMoralScoreWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalcSchoolYearMoralScoreWizard));
            this.wizard1 = new DevComponents.DotNetBar.Wizard();
            this.wizardPage1 = new DevComponents.DotNetBar.WizardPage();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.wizardPage2 = new DevComponents.DotNetBar.WizardPage();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.progressBarX1 = new DevComponents.DotNetBar.Controls.ProgressBarX();
            this.labelX4 = new DevComponents.DotNetBar.LabelX();
            this.wizardPage4 = new DevComponents.DotNetBar.WizardPage();
            this.progressBarX2 = new DevComponents.DotNetBar.Controls.ProgressBarX();
            this.labelX5 = new DevComponents.DotNetBar.LabelX();
            this.wizard1.SuspendLayout();
            this.wizardPage1.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.numericUpDown1 ) ).BeginInit();
            this.wizardPage2.SuspendLayout();
            this.wizardPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard1
            // 
            this.wizard1.BackButtonText = "< 上一步";
            this.wizard1.BackButtonWidth = 65;
            this.wizard1.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 205 ) ) ) ), ( (int)( ( (byte)( 229 ) ) ) ), ( (int)( ( (byte)( 253 ) ) ) ));
            this.wizard1.BackgroundImage = ( (System.Drawing.Image)( resources.GetObject("wizard1.BackgroundImage") ) );
            this.wizard1.ButtonStyle = DevComponents.DotNetBar.eWizardStyle.Office2007;
            this.wizard1.CancelButtonText = "取消";
            this.wizard1.CancelButtonWidth = 65;
            this.wizard1.Cursor = System.Windows.Forms.Cursors.Default;
            this.wizard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard1.FinishButtonTabIndex = 3;
            this.wizard1.FinishButtonText = "關閉";
            this.wizard1.FinishButtonWidth = 65;
            this.wizard1.FooterHeight = 25;
            // 
            // 
            // 
            this.wizard1.FooterStyle.BackColor = System.Drawing.Color.Transparent;
            this.wizard1.ForeColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 15 ) ) ) ), ( (int)( ( (byte)( 57 ) ) ) ), ( (int)( ( (byte)( 129 ) ) ) ));
            this.wizard1.HeaderHeight = 30;
            this.wizard1.HeaderImageSize = new System.Drawing.Size(48, 48);
            this.wizard1.HeaderImageVisible = false;
            // 
            // 
            // 
            this.wizard1.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 191 ) ) ) ), ( (int)( ( (byte)( 215 ) ) ) ), ( (int)( ( (byte)( 243 ) ) ) ));
            this.wizard1.HeaderStyle.BackColor2 = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 219 ) ) ) ), ( (int)( ( (byte)( 241 ) ) ) ), ( (int)( ( (byte)( 254 ) ) ) ));
            this.wizard1.HeaderStyle.BackColorGradientAngle = 90;
            this.wizard1.HeaderStyle.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.wizard1.HeaderStyle.BorderBottomColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 121 ) ) ) ), ( (int)( ( (byte)( 157 ) ) ) ), ( (int)( ( (byte)( 182 ) ) ) ));
            this.wizard1.HeaderStyle.BorderBottomWidth = 1;
            this.wizard1.HeaderStyle.BorderColor = System.Drawing.SystemColors.Control;
            this.wizard1.HeaderStyle.BorderLeftWidth = 1;
            this.wizard1.HeaderStyle.BorderRightWidth = 1;
            this.wizard1.HeaderStyle.BorderTopWidth = 1;
            this.wizard1.HeaderStyle.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.wizard1.HeaderStyle.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.wizard1.HeaderTitleIndent = 5;
            this.wizard1.HelpButtonVisible = false;
            this.wizard1.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.wizard1.Location = new System.Drawing.Point(0, 0);
            this.wizard1.Name = "wizard1";
            this.wizard1.NextButtonText = "下一步 >";
            this.wizard1.NextButtonWidth = 65;
            this.wizard1.Size = new System.Drawing.Size(261, 150);
            this.wizard1.TabIndex = 0;
            this.wizard1.WizardPages.AddRange(new DevComponents.DotNetBar.WizardPage[] {
            this.wizardPage1,
            this.wizardPage2,
            this.wizardPage4});
            this.wizard1.CancelButtonClick += new System.ComponentModel.CancelEventHandler(this.CloseForm);
            this.wizard1.FinishButtonClick += new System.ComponentModel.CancelEventHandler(this.CloseForm);
            // 
            // wizardPage1
            // 
            this.wizardPage1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.wizardPage1.AntiAlias = false;
            this.wizardPage1.BackButtonEnabled = DevComponents.DotNetBar.eWizardButtonState.False;
            this.wizardPage1.BackButtonVisible = DevComponents.DotNetBar.eWizardButtonState.False;
            this.wizardPage1.BackColor = System.Drawing.Color.Transparent;
            this.wizardPage1.Controls.Add(this.numericUpDown1);
            this.wizardPage1.Controls.Add(this.labelX2);
            this.wizardPage1.Controls.Add(this.labelX1);
            this.wizardPage1.Location = new System.Drawing.Point(7, 42);
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.PageDescription = "< Wizard step description >";
            this.wizardPage1.PageTitle = "選擇學年度";
            this.wizardPage1.Size = new System.Drawing.Size(247, 71);
            this.wizardPage1.TabIndex = 7;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(71, 34);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(64, 25);
            this.numericUpDown1.TabIndex = 8;
            this.numericUpDown1.Value = new decimal(new int[] {
            96,
            0,
            0,
            0});
            // 
            // labelX2
            // 
            this.labelX2.AutoSize = true;
            this.labelX2.Location = new System.Drawing.Point(22, 37);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(47, 19);
            this.labelX2.TabIndex = 4;
            this.labelX2.Text = "學年度";
            // 
            // labelX1
            // 
            this.labelX1.AutoSize = true;
            this.labelX1.Location = new System.Drawing.Point(6, 3);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(74, 19);
            this.labelX1.TabIndex = 5;
            this.labelX1.Text = "選擇學年度";
            // 
            // wizardPage2
            // 
            this.wizardPage2.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.wizardPage2.AntiAlias = false;
            this.wizardPage2.BackButtonVisible = DevComponents.DotNetBar.eWizardButtonState.True;
            this.wizardPage2.BackColor = System.Drawing.Color.Transparent;
            this.wizardPage2.CancelButtonVisible = DevComponents.DotNetBar.eWizardButtonState.True;
            this.wizardPage2.Controls.Add(this.linkLabel1);
            this.wizardPage2.Controls.Add(this.progressBarX1);
            this.wizardPage2.Controls.Add(this.labelX4);
            this.wizardPage2.Location = new System.Drawing.Point(7, 42);
            this.wizardPage2.Name = "wizardPage2";
            this.wizardPage2.NextButtonVisible = DevComponents.DotNetBar.eWizardButtonState.False;
            this.wizardPage2.PageDescription = "< Wizard step description >";
            this.wizardPage2.PageTitle = "計算學年德行成績";
            this.wizardPage2.Size = new System.Drawing.Size(247, 71);
            this.wizardPage2.TabIndex = 8;
            this.wizardPage2.BackButtonClick += new System.ComponentModel.CancelEventHandler(this.wizardPage2_BackButtonClick);
            this.wizardPage2.AfterPageDisplayed += new DevComponents.DotNetBar.WizardPageChangeEventHandler(this.wizardPage2_AfterPageDisplayed);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel1.Location = new System.Drawing.Point(154, 52);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(86, 17);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "檢視錯誤訊息";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // progressBarX1
            // 
            this.progressBarX1.Location = new System.Drawing.Point(35, 23);
            this.progressBarX1.Name = "progressBarX1";
            this.progressBarX1.Size = new System.Drawing.Size(189, 23);
            this.progressBarX1.TabIndex = 1;
            this.progressBarX1.Text = "progressBarX1";
            // 
            // labelX4
            // 
            this.labelX4.AutoSize = true;
            this.labelX4.Location = new System.Drawing.Point(3, -2);
            this.labelX4.Name = "labelX4";
            this.labelX4.Size = new System.Drawing.Size(110, 19);
            this.labelX4.TabIndex = 0;
            this.labelX4.Text = "德行成績計算中...";
            // 
            // wizardPage4
            // 
            this.wizardPage4.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.wizardPage4.AntiAlias = false;
            this.wizardPage4.BackButtonVisible = DevComponents.DotNetBar.eWizardButtonState.False;
            this.wizardPage4.BackColor = System.Drawing.Color.Transparent;
            this.wizardPage4.CancelButtonVisible = DevComponents.DotNetBar.eWizardButtonState.False;
            this.wizardPage4.Controls.Add(this.progressBarX2);
            this.wizardPage4.Controls.Add(this.labelX5);
            this.wizardPage4.FinishButtonEnabled = DevComponents.DotNetBar.eWizardButtonState.False;
            this.wizardPage4.Location = new System.Drawing.Point(7, 42);
            this.wizardPage4.Name = "wizardPage4";
            this.wizardPage4.NextButtonVisible = DevComponents.DotNetBar.eWizardButtonState.False;
            this.wizardPage4.PageDescription = "< Wizard step description >";
            this.wizardPage4.PageTitle = "上傳學期德行成績";
            this.wizardPage4.Size = new System.Drawing.Size(247, 71);
            this.wizardPage4.TabIndex = 10;
            this.wizardPage4.CancelButtonClick += new System.ComponentModel.CancelEventHandler(this.wizardPage4_CancelButtonClick);
            // 
            // progressBarX2
            // 
            this.progressBarX2.Location = new System.Drawing.Point(45, 33);
            this.progressBarX2.Name = "progressBarX2";
            this.progressBarX2.Size = new System.Drawing.Size(189, 23);
            this.progressBarX2.TabIndex = 3;
            this.progressBarX2.Text = "progressBarX2";
            // 
            // labelX5
            // 
            this.labelX5.AutoSize = true;
            this.labelX5.Location = new System.Drawing.Point(13, 6);
            this.labelX5.Name = "labelX5";
            this.labelX5.Size = new System.Drawing.Size(123, 19);
            this.labelX5.TabIndex = 2;
            this.labelX5.Text = "上傳學年德行成績...";
            // 
            // CalcSchoolYearMoralScoreWizard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(261, 150);
            this.ControlBox = false;
            this.Controls.Add(this.wizard1);
            this.Font = new System.Drawing.Font(SmartSchool.Common.FontStyles.GeneralFontFamily, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 136 ) ));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CalcSchoolYearMoralScoreWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "計算學年德行成績";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CalcSemesterSubjectScoreWizard_FormClosing);
            this.wizard1.ResumeLayout(false);
            this.wizardPage1.ResumeLayout(false);
            this.wizardPage1.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.numericUpDown1 ) ).EndInit();
            this.wizardPage2.ResumeLayout(false);
            this.wizardPage2.PerformLayout();
            this.wizardPage4.ResumeLayout(false);
            this.wizardPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Wizard wizard1;
        private DevComponents.DotNetBar.WizardPage wizardPage1;
        private DevComponents.DotNetBar.WizardPage wizardPage2;
        protected System.Windows.Forms.NumericUpDown numericUpDown1;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.Controls.ProgressBarX progressBarX1;
        private DevComponents.DotNetBar.LabelX labelX4;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private DevComponents.DotNetBar.WizardPage wizardPage4;
        private DevComponents.DotNetBar.Controls.ProgressBarX progressBarX2;
        private DevComponents.DotNetBar.LabelX labelX5;
    }
}