namespace SmartSchool.Evaluation.Reports.Retake
{
    partial class RetakeSelectSemesterForm
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
            this.chkAllSemester = new DevComponents.DotNetBar.Controls.CheckBoxX();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.SuspendLayout();
            // 
            // chkAllSemester
            // 
            this.chkAllSemester.AutoSize = true;
            this.chkAllSemester.BackColor = System.Drawing.Color.Transparent;
            this.chkAllSemester.Location = new System.Drawing.Point(6, 81);
            this.chkAllSemester.Name = "chkAllSemester";
            this.chkAllSemester.Size = new System.Drawing.Size(79, 21);
            this.chkAllSemester.TabIndex = 4;
            this.chkAllSemester.Text = "列印全部";
            this.chkAllSemester.CheckedChanged += new System.EventHandler(this.chkAllSemester_CheckedChanged);
            // 
            // RetakeSelectSemesterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(246, 110);
            this.Controls.Add(this.chkAllSemester);
            this.Name = "RetakeSelectSemesterForm";
            this.Text = "RetakeSelectSemesterForm";
            this.Controls.SetChildIndex(this.chkAllSemester, 0);
            this.Controls.SetChildIndex(this.buttonX1, 0);
            this.Controls.SetChildIndex(this.numericUpDown1, 0);
            this.Controls.SetChildIndex(this.numericUpDown2, 0);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.Controls.CheckBoxX chkAllSemester;

    }
}