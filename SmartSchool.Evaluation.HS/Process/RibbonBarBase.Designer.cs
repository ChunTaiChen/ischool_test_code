namespace SmartSchool.Evaluation.Process
{
    partial class RibbonBarBase
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
            this.MainRibbonBar = new DevComponents.DotNetBar.RibbonBar();
            this.SuspendLayout();
            // 
            // MainRibbonBar
            // 
            this.MainRibbonBar.AutoOverflowEnabled = true;
            this.MainRibbonBar.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.MainRibbonBar.Location = new System.Drawing.Point(3, 3);
            this.MainRibbonBar.Name = "MainRibbonBar";
            this.MainRibbonBar.Size = new System.Drawing.Size(261, 104);
            this.MainRibbonBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.MainRibbonBar.TabIndex = 0;
            this.MainRibbonBar.Text = "ribbonBar1";
            // 
            // RibbonBarBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainRibbonBar);
            this.Name = "RibbonBarBase";
            this.Size = new System.Drawing.Size(292, 144);
            this.ResumeLayout(false);

        }

        #endregion

        protected DevComponents.DotNetBar.RibbonBar MainRibbonBar;

    }
}
