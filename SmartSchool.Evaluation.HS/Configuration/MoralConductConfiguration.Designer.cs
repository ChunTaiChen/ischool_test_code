namespace SmartSchool.Evaluation.Configuration
{
    partial class MoralConductConfiguration
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

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.cardPanelEx1 = new SmartSchool.Common.CardPanelEx();
            this.panel1 = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.groupPanel3 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.basicScoreEditor1 = new SmartSchool.Evaluation.Configuration.MoralConductEditors.BasicScoreEditor();
            this.panel2 = new System.Windows.Forms.Panel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.groupPanel4 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.appraiseRuleEditor1 = new SmartSchool.Evaluation.Configuration.MoralConductEditors.AppraiseRuleEditor();
            this.panel3 = new System.Windows.Forms.Panel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.groupPanel1 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.periodAbsenceCalcRuleEditor1 = new SmartSchool.Evaluation.Configuration.MoralConductEditors.PeriodAbsenceCalcRuleEditor();
            this.panel4 = new System.Windows.Forms.Panel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.groupPanel2 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.rewardCalcRuleEditor1 = new SmartSchool.Evaluation.Configuration.MoralConductEditors.RewardCalcRuleEditor();
            this.contentPanel.SuspendLayout();
            this.cardPanelEx1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupPanel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupPanel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupPanel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.groupPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlPanel
            // 
            this.controlPanel.Size = new System.Drawing.Size(200, 645);
            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.cardPanelEx1);
            this.contentPanel.Size = new System.Drawing.Size(647, 645);
            this.contentPanel.SizeChanged += new System.EventHandler(this.contentPanel_SizeChanged);
            // 
            // cardPanelEx1
            // 
            this.cardPanelEx1.AutoScroll = true;
            this.cardPanelEx1.CanvasColor = System.Drawing.SystemColors.Control;
            this.cardPanelEx1.CardWidth = 400;
            this.cardPanelEx1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.cardPanelEx1.Controls.Add(this.panel1);
            this.cardPanelEx1.Controls.Add(this.panel2);
            this.cardPanelEx1.Controls.Add(this.panel3);
            this.cardPanelEx1.Controls.Add(this.panel4);
            this.cardPanelEx1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardPanelEx1.Location = new System.Drawing.Point(0, 0);
            this.cardPanelEx1.MinWidth = 10;
            this.cardPanelEx1.Name = "cardPanelEx1";
            this.cardPanelEx1.Padding = new System.Windows.Forms.Padding(0, 25, 0, 0);
            this.cardPanelEx1.Size = new System.Drawing.Size(647, 645);
            this.cardPanelEx1.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.cardPanelEx1.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.cardPanelEx1.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.cardPanelEx1.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.cardPanelEx1.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.cardPanelEx1.Style.GradientAngle = 90;
            this.cardPanelEx1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.linkLabel1);
            this.panel1.Controls.Add(this.groupPanel3);
            this.panel1.Location = new System.Drawing.Point(112, 35);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(400, 140);
            this.panel1.TabIndex = 4;
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.Orange;
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkColor = System.Drawing.Color.DarkOrange;
            this.linkLabel1.Location = new System.Drawing.Point(328, 3);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(60, 17);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "全部儲存";
            this.linkLabel1.Visible = false;
            this.linkLabel1.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupPanel3
            // 
            this.groupPanel3.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel3.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel3.Controls.Add(this.basicScoreEditor1);
            this.groupPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupPanel3.Location = new System.Drawing.Point(0, 0);
            this.groupPanel3.Name = "groupPanel3";
            this.groupPanel3.Size = new System.Drawing.Size(400, 140);
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
            this.groupPanel3.TabIndex = 2;
            this.groupPanel3.TabStop = true;
            this.groupPanel3.Text = "德行基本分設定";
            // 
            // basicScoreEditor1
            // 
            this.basicScoreEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.basicScoreEditor1.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.basicScoreEditor1.IsDirty = true;
            this.basicScoreEditor1.Location = new System.Drawing.Point(0, 0);
            this.basicScoreEditor1.Margin = new System.Windows.Forms.Padding(4);
            this.basicScoreEditor1.Name = "basicScoreEditor1";
            this.basicScoreEditor1.Size = new System.Drawing.Size(394, 113);
            this.basicScoreEditor1.TabIndex = 0;
            this.basicScoreEditor1.IsDirtyChanged += new System.EventHandler(this.basicScoreEditor1_IsDirtyChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.linkLabel2);
            this.panel2.Controls.Add(this.groupPanel4);
            this.panel2.Location = new System.Drawing.Point(112, 185);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(400, 216);
            this.panel2.TabIndex = 5;
            // 
            // linkLabel2
            // 
            this.linkLabel2.ActiveLinkColor = System.Drawing.Color.Orange;
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.LinkColor = System.Drawing.Color.DarkOrange;
            this.linkLabel2.Location = new System.Drawing.Point(328, 3);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(60, 17);
            this.linkLabel2.TabIndex = 3;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "全部儲存";
            this.linkLabel2.Visible = false;
            this.linkLabel2.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupPanel4
            // 
            this.groupPanel4.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel4.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel4.Controls.Add(this.appraiseRuleEditor1);
            this.groupPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupPanel4.Location = new System.Drawing.Point(0, 0);
            this.groupPanel4.Name = "groupPanel4";
            this.groupPanel4.Size = new System.Drawing.Size(400, 216);
            // 
            // 
            // 
            this.groupPanel4.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanel4.Style.BackColorGradientAngle = 90;
            this.groupPanel4.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanel4.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel4.Style.BorderBottomWidth = 1;
            this.groupPanel4.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanel4.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel4.Style.BorderLeftWidth = 1;
            this.groupPanel4.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel4.Style.BorderRightWidth = 1;
            this.groupPanel4.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel4.Style.BorderTopWidth = 1;
            this.groupPanel4.Style.CornerDiameter = 4;
            this.groupPanel4.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel4.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel4.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanel4.TabIndex = 3;
            this.groupPanel4.Text = "評分項目設定";
            // 
            // appraiseRuleEditor1
            // 
            this.appraiseRuleEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appraiseRuleEditor1.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.appraiseRuleEditor1.IsDirty = false;
            this.appraiseRuleEditor1.Location = new System.Drawing.Point(0, 0);
            this.appraiseRuleEditor1.Margin = new System.Windows.Forms.Padding(4);
            this.appraiseRuleEditor1.Name = "appraiseRuleEditor1";
            this.appraiseRuleEditor1.Size = new System.Drawing.Size(394, 189);
            this.appraiseRuleEditor1.TabIndex = 0;
            this.appraiseRuleEditor1.IsDirtyChanged += new System.EventHandler(this.appraiseRuleEditor1_IsDirtyChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.linkLabel3);
            this.panel3.Controls.Add(this.groupPanel1);
            this.panel3.Location = new System.Drawing.Point(112, 411);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(400, 411);
            this.panel3.TabIndex = 6;
            // 
            // linkLabel3
            // 
            this.linkLabel3.ActiveLinkColor = System.Drawing.Color.Orange;
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.LinkColor = System.Drawing.Color.DarkOrange;
            this.linkLabel3.Location = new System.Drawing.Point(328, 3);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(60, 17);
            this.linkLabel3.TabIndex = 3;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "全部儲存";
            this.linkLabel3.Visible = false;
            this.linkLabel3.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupPanel1
            // 
            this.groupPanel1.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel1.Controls.Add(this.periodAbsenceCalcRuleEditor1);
            this.groupPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupPanel1.Location = new System.Drawing.Point(0, 0);
            this.groupPanel1.Name = "groupPanel1";
            this.groupPanel1.Size = new System.Drawing.Size(400, 411);
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
            this.groupPanel1.Text = "缺曠加扣分設定";
            // 
            // periodAbsenceCalcRuleEditor1
            // 
            this.periodAbsenceCalcRuleEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.periodAbsenceCalcRuleEditor1.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.periodAbsenceCalcRuleEditor1.IsDirty = false;
            this.periodAbsenceCalcRuleEditor1.Location = new System.Drawing.Point(0, 0);
            this.periodAbsenceCalcRuleEditor1.Name = "periodAbsenceCalcRuleEditor1";
            this.periodAbsenceCalcRuleEditor1.Size = new System.Drawing.Size(394, 384);
            this.periodAbsenceCalcRuleEditor1.TabIndex = 0;
            this.periodAbsenceCalcRuleEditor1.IsDirtyChanged += new System.EventHandler(this.periodAbsenceCalcRuleEditor1_IsDirtyChanged);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.linkLabel4);
            this.panel4.Controls.Add(this.groupPanel2);
            this.panel4.Location = new System.Drawing.Point(112, 832);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(400, 363);
            this.panel4.TabIndex = 7;
            // 
            // linkLabel4
            // 
            this.linkLabel4.ActiveLinkColor = System.Drawing.Color.Orange;
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.LinkColor = System.Drawing.Color.DarkOrange;
            this.linkLabel4.Location = new System.Drawing.Point(328, 3);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(60, 17);
            this.linkLabel4.TabIndex = 3;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "全部儲存";
            this.linkLabel4.Visible = false;
            this.linkLabel4.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupPanel2
            // 
            this.groupPanel2.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel2.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel2.Controls.Add(this.rewardCalcRuleEditor1);
            this.groupPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupPanel2.Location = new System.Drawing.Point(0, 0);
            this.groupPanel2.Name = "groupPanel2";
            this.groupPanel2.Size = new System.Drawing.Size(400, 363);
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
            this.groupPanel2.TabIndex = 1;
            this.groupPanel2.Text = "獎懲加扣分設定";
            // 
            // rewardCalcRuleEditor1
            // 
            this.rewardCalcRuleEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rewardCalcRuleEditor1.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rewardCalcRuleEditor1.IsDirty = false;
            this.rewardCalcRuleEditor1.Location = new System.Drawing.Point(0, 0);
            this.rewardCalcRuleEditor1.Name = "rewardCalcRuleEditor1";
            this.rewardCalcRuleEditor1.Size = new System.Drawing.Size(394, 336);
            this.rewardCalcRuleEditor1.TabIndex = 0;
            this.rewardCalcRuleEditor1.IsDirtyChanged += new System.EventHandler(this.rewardCalcRuleEditor1_IsDirtyChanged);
            // 
            // MoralConductConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Caption = "德行成績計算規則";
            this.Name = "MoralConductConfiguration";
            this.Size = new System.Drawing.Size(850, 664);
            this.TabGroup = "學務作業";
            this.contentPanel.ResumeLayout(false);
            this.cardPanelEx1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupPanel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupPanel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupPanel1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.groupPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SmartSchool.Common.CardPanelEx cardPanelEx1;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel1;
        private MoralConductEditors.PeriodAbsenceCalcRuleEditor periodAbsenceCalcRuleEditor1;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel2;
        private MoralConductEditors.RewardCalcRuleEditor rewardCalcRuleEditor1;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel3;
        private SmartSchool.Evaluation.Configuration.MoralConductEditors.BasicScoreEditor basicScoreEditor1;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel4;
        private SmartSchool.Evaluation.Configuration.MoralConductEditors.AppraiseRuleEditor appraiseRuleEditor1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.LinkLabel linkLabel4;

    }
}
