namespace SmartSchool.Evaluation.GraduationPlan
{
    partial class GraduationPlanSelector
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
            this.SuspendLayout();
            // 
            // cardPanelEx1
            // 
            this.cardPanelEx1.AutoScroll = true;
            this.cardPanelEx1.CanvasColor = System.Drawing.SystemColors.Control;
            this.cardPanelEx1.CardWidth = 140;
            this.cardPanelEx1.ColorScheme.ItemDesignTimeBorder = System.Drawing.Color.Black;
            this.cardPanelEx1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.cardPanelEx1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardPanelEx1.Location = new System.Drawing.Point(0, 0);
            this.cardPanelEx1.MinWidth = 4;
            this.cardPanelEx1.Name = "cardPanelEx1";
            this.cardPanelEx1.Size = new System.Drawing.Size(314, 350);
            this.cardPanelEx1.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.cardPanelEx1.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.cardPanelEx1.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.cardPanelEx1.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.cardPanelEx1.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.cardPanelEx1.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.cardPanelEx1.Style.GradientAngle = 90;
            this.cardPanelEx1.TabIndex = 0;
            this.cardPanelEx1.MouseHover += new System.EventHandler(this.SetFocus);
            // 
            // GraduationPlanSelector
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.cardPanelEx1);
            this.Font = new System.Drawing.Font(SmartSchool.Common.FontStyles.GeneralFontFamily, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Name = "GraduationPlanSelector";
            this.Size = new System.Drawing.Size(314, 350);
            this.ResumeLayout(false);

        }

        #endregion

        private SmartSchool.Common.CardPanelEx cardPanelEx1;
    }
}
