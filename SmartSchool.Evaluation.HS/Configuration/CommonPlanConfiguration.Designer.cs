namespace SmartSchool.Evaluation.Configuration
{
    partial class CommonPlanConfiguration
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
            this.navigationPanePanel2 = new DevComponents.DotNetBar.NavigationPanePanel();
            this.panelEx1 = new DevComponents.DotNetBar.PanelEx();
            this.panel1 = new System.Windows.Forms.Panel();
            this.commonPlanEditor1 = new SmartSchool.Evaluation.GraduationPlan.Editor.CommonPlanEditor();
            this.buttonX4 = new DevComponents.DotNetBar.ButtonX();
            this.controlPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            this.panelEx1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlPanel
            // 
            this.controlPanel.Controls.Add(this.navigationPanePanel2);
            this.controlPanel.Size = new System.Drawing.Size(200, 634);
            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.panelEx1);
            this.contentPanel.Size = new System.Drawing.Size(717, 634);
            // 
            // navigationPanePanel2
            // 
            this.navigationPanePanel2.AutoScroll = true;
            this.navigationPanePanel2.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.navigationPanePanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigationPanePanel2.Location = new System.Drawing.Point(0, 0);
            this.navigationPanePanel2.Name = "navigationPanePanel2";
            this.navigationPanePanel2.ParentItem = null;
            this.navigationPanePanel2.Size = new System.Drawing.Size(200, 634);
            this.navigationPanePanel2.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.navigationPanePanel2.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground2;
            this.navigationPanePanel2.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.navigationPanePanel2.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.navigationPanePanel2.Style.GradientAngle = 90;
            this.navigationPanePanel2.Style.LineAlignment = System.Drawing.StringAlignment.Near;
            this.navigationPanePanel2.Style.MarginLeft = 6;
            this.navigationPanePanel2.Style.MarginTop = 6;
            this.navigationPanePanel2.Style.WordWrap = true;
            this.navigationPanePanel2.TabIndex = 4;
            this.navigationPanePanel2.Text = "通用科目表用以判斷非課程規劃表上之課程相關屬性。當不依據課程規劃進行班級開課而是由使用者自行開課，但是計算學期成績時使用者又希望某一標準定義科目屬性，這類課程之科" +
    "目屬性可使用通用科目表來授予；這些屬性會於計算學期成績時帶至學生修習科目上。\r\n\r\n通用科目表與課程規劃相同，只有在成績計算規則選擇依課程規劃計算學分數及畢業標" +
    "準時才會發揮作用。單一學校只會有一份通用科目表，不分年級、科別。";
            // 
            // panelEx1
            // 
            this.panelEx1.CanvasColor = System.Drawing.SystemColors.Control;
            this.panelEx1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.panelEx1.Controls.Add(this.panel1);
            this.panelEx1.Controls.Add(this.buttonX4);
            this.panelEx1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEx1.Location = new System.Drawing.Point(0, 0);
            this.panelEx1.Name = "panelEx1";
            this.panelEx1.Size = new System.Drawing.Size(717, 634);
            this.panelEx1.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelEx1.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.panelEx1.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.panelEx1.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.panelEx1.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.panelEx1.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.panelEx1.Style.GradientAngle = 90;
            this.panelEx1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.commonPlanEditor1);
            this.panel1.Location = new System.Drawing.Point(3, 35);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(710, 593);
            this.panel1.TabIndex = 4;
            // 
            // commonPlanEditor1
            // 
            this.commonPlanEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commonPlanEditor1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.commonPlanEditor1.Location = new System.Drawing.Point(0, 0);
            this.commonPlanEditor1.Margin = new System.Windows.Forms.Padding(276, 771, 276, 771);
            this.commonPlanEditor1.Name = "commonPlanEditor1";
            this.commonPlanEditor1.Size = new System.Drawing.Size(710, 593);
            this.commonPlanEditor1.TabIndex = 1;
            // 
            // buttonX4
            // 
            this.buttonX4.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonX4.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX4.Enabled = false;
            this.buttonX4.Location = new System.Drawing.Point(633, 6);
            this.buttonX4.Name = "buttonX4";
            this.buttonX4.Size = new System.Drawing.Size(72, 23);
            this.buttonX4.TabIndex = 3;
            this.buttonX4.Text = "儲存";
            this.buttonX4.Click += new System.EventHandler(this.buttonX4_Click);
            // 
            // CommonPlanConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Caption = "通用課程規劃表";
            this.Category = "課務作業";
            this.HasControlPanel = true;
            this.Name = "CommonPlanConfiguration";
            this.Size = new System.Drawing.Size(920, 653);
            this.TabGroup = "教務作業";
            this.controlPanel.ResumeLayout(false);
            this.contentPanel.ResumeLayout(false);
            this.panelEx1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.NavigationPanePanel navigationPanePanel2;
        private DevComponents.DotNetBar.PanelEx panelEx1;
        private System.Windows.Forms.Panel panel1;
        private SmartSchool.Evaluation.GraduationPlan.Editor.CommonPlanEditor commonPlanEditor1;
        private DevComponents.DotNetBar.ButtonX buttonX4;
    }
}
