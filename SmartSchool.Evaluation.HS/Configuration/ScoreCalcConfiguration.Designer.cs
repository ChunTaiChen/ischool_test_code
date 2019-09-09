namespace SmartSchool.Evaluation.Configuration
{
    partial class ScoreCalcConfiguration
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
            this._WaitingPicture = new System.Windows.Forms.PictureBox();
            this.expandablePanel1 = new DevComponents.DotNetBar.ExpandablePanel();
            this.navigationPanePanel2 = new DevComponents.DotNetBar.NavigationPanePanel();
            this.itemPanel1 = new DevComponents.DotNetBar.ItemPanel();
            this.btn_create = new DevComponents.DotNetBar.ButtonX();
            this.btn_update = new DevComponents.DotNetBar.ButtonX();
            this.btn_delete = new DevComponents.DotNetBar.ButtonX();
            this.scoreCalcRuleEditor1 = new SmartSchool.Evaluation.Configuration.ScoreCalcRuleEditor();
            this.controlPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._WaitingPicture)).BeginInit();
            this.expandablePanel1.SuspendLayout();
            this.navigationPanePanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlPanel
            // 
            this.controlPanel.Controls.Add(this.expandablePanel1);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.None;
            this.controlPanel.Location = new System.Drawing.Point(9, 37);
            this.controlPanel.Size = new System.Drawing.Size(200, 440);
            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.scoreCalcRuleEditor1);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.None;
            this.contentPanel.Location = new System.Drawing.Point(231, 37);
            this.contentPanel.Size = new System.Drawing.Size(554, 460);
            // 
            // _WaitingPicture
            // 
            this._WaitingPicture.BackColor = System.Drawing.Color.White;
            this._WaitingPicture.Location = new System.Drawing.Point(55, 148);
            this._WaitingPicture.Name = "_WaitingPicture";
            this._WaitingPicture.Size = new System.Drawing.Size(32, 32);
            this._WaitingPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this._WaitingPicture.TabIndex = 3;
            this._WaitingPicture.TabStop = false;
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.CanvasColor = System.Drawing.SystemColors.Control;
            this.expandablePanel1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.expandablePanel1.Controls.Add(this.navigationPanePanel2);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expandablePanel1.ExpandButtonVisible = false;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(200, 440);
            this.expandablePanel1.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.expandablePanel1.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.expandablePanel1.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.expandablePanel1.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.expandablePanel1.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.expandablePanel1.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.expandablePanel1.Style.GradientAngle = 90;
            this.expandablePanel1.TabIndex = 0;
            this.expandablePanel1.TitleHeight = 23;
            this.expandablePanel1.TitleStyle.Alignment = System.Drawing.StringAlignment.Center;
            this.expandablePanel1.TitleStyle.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.expandablePanel1.TitleStyle.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.expandablePanel1.TitleStyle.Border = DevComponents.DotNetBar.eBorderType.RaisedInner;
            this.expandablePanel1.TitleStyle.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.expandablePanel1.TitleStyle.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.expandablePanel1.TitleStyle.GradientAngle = 90;
            this.expandablePanel1.TitleText = "學業成績計算規則";
            // 
            // navigationPanePanel2
            // 
            this.navigationPanePanel2.AutoScroll = true;
            this.navigationPanePanel2.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.navigationPanePanel2.Controls.Add(this._WaitingPicture);
            this.navigationPanePanel2.Controls.Add(this.itemPanel1);
            this.navigationPanePanel2.Controls.Add(this.btn_create);
            this.navigationPanePanel2.Controls.Add(this.btn_update);
            this.navigationPanePanel2.Controls.Add(this.btn_delete);
            this.navigationPanePanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigationPanePanel2.Location = new System.Drawing.Point(0, 23);
            this.navigationPanePanel2.Name = "navigationPanePanel2";
            this.navigationPanePanel2.ParentItem = null;
            this.navigationPanePanel2.Size = new System.Drawing.Size(200, 417);
            this.navigationPanePanel2.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.navigationPanePanel2.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground2;
            this.navigationPanePanel2.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.navigationPanePanel2.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.navigationPanePanel2.Style.GradientAngle = 90;
            this.navigationPanePanel2.Style.LineAlignment = System.Drawing.StringAlignment.Near;
            this.navigationPanePanel2.Style.MarginLeft = 6;
            this.navigationPanePanel2.Style.MarginTop = 6;
            this.navigationPanePanel2.Style.WordWrap = true;
            this.navigationPanePanel2.TabIndex = 5;
            // 
            // itemPanel1
            // 
            this.itemPanel1.AutoScroll = true;
            // 
            // 
            // 
            this.itemPanel1.BackgroundStyle.BackColor = System.Drawing.Color.White;
            this.itemPanel1.BackgroundStyle.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.itemPanel1.BackgroundStyle.BorderBottomWidth = 1;
            this.itemPanel1.BackgroundStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(157)))), ((int)(((byte)(185)))));
            this.itemPanel1.BackgroundStyle.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.itemPanel1.BackgroundStyle.BorderLeftWidth = 1;
            this.itemPanel1.BackgroundStyle.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.itemPanel1.BackgroundStyle.BorderRightWidth = 1;
            this.itemPanel1.BackgroundStyle.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.itemPanel1.BackgroundStyle.BorderTopWidth = 1;
            this.itemPanel1.BackgroundStyle.Class = "";
            this.itemPanel1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemPanel1.BackgroundStyle.PaddingBottom = 1;
            this.itemPanel1.BackgroundStyle.PaddingLeft = 1;
            this.itemPanel1.BackgroundStyle.PaddingRight = 1;
            this.itemPanel1.BackgroundStyle.PaddingTop = 1;
            this.itemPanel1.ContainerControlProcessDialogKey = true;
            this.itemPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemPanel1.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemPanel1.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.itemPanel1.Location = new System.Drawing.Point(0, 0);
            this.itemPanel1.Name = "itemPanel1";
            this.itemPanel1.Size = new System.Drawing.Size(200, 348);
            this.itemPanel1.TabIndex = 4;
            this.itemPanel1.Text = "itemPanel2";
            // 
            // btn_create
            // 
            this.btn_create.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btn_create.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btn_create.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_create.Location = new System.Drawing.Point(0, 348);
            this.btn_create.Name = "btn_create";
            this.btn_create.Size = new System.Drawing.Size(200, 23);
            this.btn_create.TabIndex = 0;
            this.btn_create.Text = "新增";
            this.btn_create.Click += new System.EventHandler(this.btn_create_Click);
            // 
            // btn_update
            // 
            this.btn_update.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btn_update.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btn_update.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_update.Location = new System.Drawing.Point(0, 371);
            this.btn_update.Name = "btn_update";
            this.btn_update.Size = new System.Drawing.Size(200, 23);
            this.btn_update.TabIndex = 0;
            this.btn_update.Text = "儲存";
            this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
            // 
            // btn_delete
            // 
            this.btn_delete.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btn_delete.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btn_delete.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btn_delete.Enabled = false;
            this.btn_delete.Location = new System.Drawing.Point(0, 394);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(200, 23);
            this.btn_delete.TabIndex = 0;
            this.btn_delete.Text = "刪除";
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // scoreCalcRuleEditor1
            // 
            this.scoreCalcRuleEditor1.AutoScroll = true;
            this.scoreCalcRuleEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scoreCalcRuleEditor1.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.scoreCalcRuleEditor1.Location = new System.Drawing.Point(0, 0);
            this.scoreCalcRuleEditor1.Margin = new System.Windows.Forms.Padding(4);
            this.scoreCalcRuleEditor1.Name = "scoreCalcRuleEditor1";
            this.scoreCalcRuleEditor1.ScoreCalcRuleName = "";
            this.scoreCalcRuleEditor1.Size = new System.Drawing.Size(554, 460);
            this.scoreCalcRuleEditor1.TabIndex = 6;
            this.scoreCalcRuleEditor1.Visible = false;
            // 
            // ScoreCalcConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Caption = "成績計算規則";
            this.Category = "成績作業";
            this.HasControlPanel = true;
            this.Name = "ScoreCalcConfiguration";
            this.Size = new System.Drawing.Size(842, 557);
            this.TabGroup = "教務作業";
            this.controlPanel.ResumeLayout(false);
            this.contentPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._WaitingPicture)).EndInit();
            this.expandablePanel1.ResumeLayout(false);
            this.navigationPanePanel2.ResumeLayout(false);
            this.navigationPanePanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _WaitingPicture;
        private ScoreCalcRuleEditor scoreCalcRuleEditor1;
        private DevComponents.DotNetBar.ExpandablePanel expandablePanel1;
        private DevComponents.DotNetBar.NavigationPanePanel navigationPanePanel2;
        private DevComponents.DotNetBar.ItemPanel itemPanel1;
        private DevComponents.DotNetBar.ButtonX btn_create;
        private DevComponents.DotNetBar.ButtonX btn_update;
        private DevComponents.DotNetBar.ButtonX btn_delete;

    }
}
