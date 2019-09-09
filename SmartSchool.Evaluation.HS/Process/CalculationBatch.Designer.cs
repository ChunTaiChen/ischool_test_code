namespace SmartSchool.Evaluation.Process
{
    partial class CalculationBatch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalculationBatch));
            this.buttonItem11 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem12 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem14 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem15 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem103 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem6 = new DevComponents.DotNetBar.ButtonItem();
            this.labelItem2 = new DevComponents.DotNetBar.LabelItem();
            this.buttonItem1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem4 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem5 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem7 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem2 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem3 = new DevComponents.DotNetBar.ButtonItem();
            this.lblCalcRank = new DevComponents.DotNetBar.LabelItem();
            this.btnSemesterRank = new DevComponents.DotNetBar.ButtonItem();
            this.btnSchoolYearRank = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem9 = new DevComponents.DotNetBar.ButtonItem();
            this.labelItem4 = new DevComponents.DotNetBar.LabelItem();
            this.buttonItem16 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem17 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem8 = new DevComponents.DotNetBar.ButtonItem();
            this.SuspendLayout();
            // 
            // MainRibbonBar
            // 
            this.MainRibbonBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem103,
            this.buttonItem9});
            this.MainRibbonBar.Location = new System.Drawing.Point(4, 4);
            this.MainRibbonBar.Margin = new System.Windows.Forms.Padding(4);
            this.MainRibbonBar.Size = new System.Drawing.Size(287, 121);
            this.MainRibbonBar.Text = "成績處理";
            // 
            // buttonItem11
            // 
            this.buttonItem11.ImagePaddingHorizontal = 8;
            this.buttonItem11.Name = "buttonItem11";
            this.buttonItem11.Text = "計算學期科目成績";
            // 
            // buttonItem12
            // 
            this.buttonItem12.ImagePaddingHorizontal = 8;
            this.buttonItem12.Name = "buttonItem12";
            this.buttonItem12.Text = "計算學期分項成績";
            // 
            // buttonItem14
            // 
            this.buttonItem14.ImagePaddingHorizontal = 8;
            this.buttonItem14.Name = "buttonItem14";
            this.buttonItem14.Text = "計算學年科目成績";
            // 
            // buttonItem15
            // 
            this.buttonItem15.ImagePaddingHorizontal = 8;
            this.buttonItem15.Name = "buttonItem15";
            this.buttonItem15.Text = "計算學年分項成績";
            // 
            // buttonItem103
            // 
            this.buttonItem103.AutoExpandOnClick = true;
            this.buttonItem103.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem103.GlobalItem = false;
            this.buttonItem103.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem103.Image") ) );
            this.buttonItem103.ImageFixedSize = new System.Drawing.Size(24, 24);
            this.buttonItem103.ImagePaddingHorizontal = 3;
            this.buttonItem103.ImagePaddingVertical = 10;
            this.buttonItem103.Name = "buttonItem103";
            this.buttonItem103.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem6,
            this.buttonItem8,
            this.labelItem2,
            this.buttonItem1,
            this.buttonItem7,
            this.lblCalcRank,
            this.btnSemesterRank,
            this.btnSchoolYearRank});
            this.buttonItem103.SubItemsExpandWidth = 14;
            this.buttonItem103.Text = "計算成績";
            // 
            // buttonItem6
            // 
            this.buttonItem6.ImagePaddingHorizontal = 8;
            this.buttonItem6.Name = "buttonItem6";
            this.buttonItem6.Text = "學期成績處理";
            this.buttonItem6.Click += new System.EventHandler(this.buttonItem6_Click_1);
            // 
            // labelItem2
            // 
            this.labelItem2.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 221 ) ) ) ), ( (int)( ( (byte)( 231 ) ) ) ), ( (int)( ( (byte)( 238 ) ) ) ));
            this.labelItem2.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.labelItem2.BorderType = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.labelItem2.ForeColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 0 ) ) ) ), ( (int)( ( (byte)( 21 ) ) ) ), ( (int)( ( (byte)( 110 ) ) ) ));
            this.labelItem2.Name = "labelItem2";
            this.labelItem2.PaddingBottom = 1;
            this.labelItem2.PaddingLeft = 10;
            this.labelItem2.PaddingTop = 1;
            this.labelItem2.SingleLineColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 197 ) ) ) ), ( (int)( ( (byte)( 197 ) ) ) ), ( (int)( ( (byte)( 197 ) ) ) ));
            this.labelItem2.Text = "學業成績處理";
            this.labelItem2.Visible = false;
            // 
            // buttonItem1
            // 
            this.buttonItem1.CanCustomize = false;
            this.buttonItem1.ClickRepeatInterval = 0;
            this.buttonItem1.ImagePaddingHorizontal = 8;
            this.buttonItem1.Name = "buttonItem1";
            this.buttonItem1.PersonalizedMenus = DevComponents.DotNetBar.ePersonalizedMenus.Both;
            this.buttonItem1.PulseSpeed = 1;
            this.buttonItem1.SplitButton = true;
            this.buttonItem1.StopPulseOnMouseOver = false;
            this.buttonItem1.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem4,
            this.buttonItem5});
            this.buttonItem1.Text = "計算學期成績";
            this.buttonItem1.Visible = false;
            // 
            // buttonItem4
            // 
            this.buttonItem4.ImagePaddingHorizontal = 8;
            this.buttonItem4.Name = "buttonItem4";
            this.buttonItem4.Text = "計算學期科目成績";
            this.buttonItem4.Click += new System.EventHandler(this.buttonItem4_Click);
            // 
            // buttonItem5
            // 
            this.buttonItem5.ImagePaddingHorizontal = 8;
            this.buttonItem5.Name = "buttonItem5";
            this.buttonItem5.Text = "計算學期分項成績";
            this.buttonItem5.Click += new System.EventHandler(this.buttonItem5_Click);
            // 
            // buttonItem7
            // 
            this.buttonItem7.ImagePaddingHorizontal = 8;
            this.buttonItem7.Name = "buttonItem7";
            this.buttonItem7.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem2,
            this.buttonItem3});
            this.buttonItem7.Text = "計算學年成績";
            this.buttonItem7.Visible = false;
            // 
            // buttonItem2
            // 
            this.buttonItem2.ImagePaddingHorizontal = 8;
            this.buttonItem2.Name = "buttonItem2";
            this.buttonItem2.Text = "計算學年科目成績";
            this.buttonItem2.Click += new System.EventHandler(this.buttonItem2_Click);
            // 
            // buttonItem3
            // 
            this.buttonItem3.ImagePaddingHorizontal = 8;
            this.buttonItem3.Name = "buttonItem3";
            this.buttonItem3.Text = "計算學年分項成績";
            this.buttonItem3.Click += new System.EventHandler(this.buttonItem3_Click);
            // 
            // lblCalcRank
            // 
            this.lblCalcRank.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 221 ) ) ) ), ( (int)( ( (byte)( 231 ) ) ) ), ( (int)( ( (byte)( 238 ) ) ) ));
            this.lblCalcRank.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.lblCalcRank.BorderType = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.lblCalcRank.ForeColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 0 ) ) ) ), ( (int)( ( (byte)( 21 ) ) ) ), ( (int)( ( (byte)( 110 ) ) ) ));
            this.lblCalcRank.Name = "lblCalcRank";
            this.lblCalcRank.PaddingBottom = 1;
            this.lblCalcRank.PaddingLeft = 10;
            this.lblCalcRank.PaddingTop = 1;
            this.lblCalcRank.SingleLineColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 197 ) ) ) ), ( (int)( ( (byte)( 197 ) ) ) ), ( (int)( ( (byte)( 197 ) ) ) ));
            this.lblCalcRank.Text = "計算排名";
            this.lblCalcRank.Visible = false;
            // 
            // btnSemesterRank
            // 
            this.btnSemesterRank.ImagePaddingHorizontal = 8;
            this.btnSemesterRank.Name = "btnSemesterRank";
            this.btnSemesterRank.Text = "學期成績排名";
            this.btnSemesterRank.Visible = false;
            this.btnSemesterRank.Click += new System.EventHandler(this.btnSemesterRank_Click);
            // 
            // btnSchoolYearRank
            // 
            this.btnSchoolYearRank.ImagePaddingHorizontal = 8;
            this.btnSchoolYearRank.Name = "btnSchoolYearRank";
            this.btnSchoolYearRank.Text = "學年成績排名";
            this.btnSchoolYearRank.Visible = false;
            this.btnSchoolYearRank.Click += new System.EventHandler(this.btnSchoolYearRank_Click);
            // 
            // buttonItem9
            // 
            this.buttonItem9.AutoExpandOnClick = true;
            this.buttonItem9.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem9.GlobalItem = false;
            this.buttonItem9.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem9.Image") ) );
            this.buttonItem9.ImageFixedSize = new System.Drawing.Size(24, 24);
            this.buttonItem9.ImagePaddingHorizontal = 3;
            this.buttonItem9.ImagePaddingVertical = 10;
            this.buttonItem9.Name = "buttonItem9";
            this.buttonItem9.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.labelItem4,
            this.buttonItem16,
            this.buttonItem17});
            this.buttonItem9.SubItemsExpandWidth = 14;
            this.buttonItem9.Text = "德行成績";
            // 
            // labelItem4
            // 
            this.labelItem4.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 221 ) ) ) ), ( (int)( ( (byte)( 231 ) ) ) ), ( (int)( ( (byte)( 238 ) ) ) ));
            this.labelItem4.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.labelItem4.BorderType = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.labelItem4.ForeColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 0 ) ) ) ), ( (int)( ( (byte)( 21 ) ) ) ), ( (int)( ( (byte)( 110 ) ) ) ));
            this.labelItem4.Name = "labelItem4";
            this.labelItem4.PaddingBottom = 1;
            this.labelItem4.PaddingLeft = 10;
            this.labelItem4.PaddingTop = 1;
            this.labelItem4.SingleLineColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 197 ) ) ) ), ( (int)( ( (byte)( 197 ) ) ) ), ( (int)( ( (byte)( 197 ) ) ) ));
            this.labelItem4.Text = "德行成績處理";
            // 
            // buttonItem16
            // 
            this.buttonItem16.ImagePaddingHorizontal = 8;
            this.buttonItem16.Name = "buttonItem16";
            this.buttonItem16.Text = "計算德行學期成績";
            this.buttonItem16.Click += new System.EventHandler(this.buttonItem6_Click);
            // 
            // buttonItem17
            // 
            this.buttonItem17.ImagePaddingHorizontal = 8;
            this.buttonItem17.Name = "buttonItem17";
            this.buttonItem17.Text = "計算德行學年成績";
            this.buttonItem17.Click += new System.EventHandler(this.buttonItem8_Click);
            // 
            // buttonItem8
            // 
            this.buttonItem8.ImagePaddingHorizontal = 8;
            this.buttonItem8.Name = "buttonItem8";
            this.buttonItem8.Text = "學年成績處理";
            this.buttonItem8.Click += new System.EventHandler(this.buttonItem8_Click_1);
            // 
            // CalculationBatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.Font = new System.Drawing.Font(SmartSchool.Common.FontStyles.GeneralFontFamily, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 136 ) ));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CalculationBatch";
            this.Size = new System.Drawing.Size(389, 204);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.ButtonItem buttonItem9;
        private DevComponents.DotNetBar.ButtonItem buttonItem11;
        private DevComponents.DotNetBar.ButtonItem buttonItem12;
        private DevComponents.DotNetBar.ButtonItem buttonItem14;
        private DevComponents.DotNetBar.ButtonItem buttonItem15;
        private DevComponents.DotNetBar.LabelItem labelItem4;
        private DevComponents.DotNetBar.ButtonItem buttonItem16;
        private DevComponents.DotNetBar.ButtonItem buttonItem17;
        private DevComponents.DotNetBar.ButtonItem buttonItem103;
        private DevComponents.DotNetBar.LabelItem labelItem2;
        private DevComponents.DotNetBar.ButtonItem buttonItem1;
        private DevComponents.DotNetBar.ButtonItem buttonItem4;
        private DevComponents.DotNetBar.ButtonItem buttonItem5;
        private DevComponents.DotNetBar.ButtonItem buttonItem7;
        private DevComponents.DotNetBar.ButtonItem buttonItem2;
        private DevComponents.DotNetBar.ButtonItem buttonItem3;
        private DevComponents.DotNetBar.LabelItem lblCalcRank;
        private DevComponents.DotNetBar.ButtonItem btnSemesterRank;
        private DevComponents.DotNetBar.ButtonItem btnSchoolYearRank;
        private DevComponents.DotNetBar.ButtonItem buttonItem6;
        private DevComponents.DotNetBar.ButtonItem buttonItem8;
    }
}
