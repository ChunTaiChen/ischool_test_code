//namespace SmartSchool.Evaluation.Process
//{
//    partial class AssignStudent
//    {
//        /// <summary> 
//        /// 設計工具所需的變數。
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;

//        /// <summary> 
//        /// 清除任何使用中的資源。
//        /// </summary>
//        /// <param name="disposing">如果應該公開 Managed 資源則為 true，否則為 false。</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region 元件設計工具產生的程式碼

//        /// <summary> 
//        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
//        ///
//        /// </summary>
//        private void InitializeComponent()
//        {
//            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssignStudent));
//            this.buttonItem56 = new DevComponents.DotNetBar.ButtonItem();
//            this.itemContainer1 = new DevComponents.DotNetBar.ItemContainer();
//            this.controlContainerItem1 = new DevComponents.DotNetBar.ControlContainerItem();
//            this.buttonItem65 = new DevComponents.DotNetBar.ButtonItem();
//            this.buttonItem71 = new DevComponents.DotNetBar.ButtonItem();
//            this.buttonItem72 = new DevComponents.DotNetBar.ButtonItem();
//            this.buttonItem73 = new DevComponents.DotNetBar.ButtonItem();
//            this.buttonItem74 = new DevComponents.DotNetBar.ButtonItem();
//            this.buttonItem75 = new DevComponents.DotNetBar.ButtonItem();
//            this.buttonItem76 = new DevComponents.DotNetBar.ButtonItem();
//            this.itemContainer2 = new DevComponents.DotNetBar.ItemContainer();
//            this.SuspendLayout();
//            // 
//            // MainRibbonBar
//            // 
//            this.MainRibbonBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
//            this.itemContainer2});
//            this.MainRibbonBar.Size = new System.Drawing.Size(261, 89);
//            this.MainRibbonBar.Text = "指定";
//            // 
//            // buttonItem56
//            // 
//            this.buttonItem56.AutoExpandOnClick = true;
//            this.buttonItem56.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
//            this.buttonItem56.Enabled = false;
//            this.buttonItem56.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem56.Image")));
//            this.buttonItem56.ImageFixedSize = new System.Drawing.Size(32, 32);
//            this.buttonItem56.ImagePaddingHorizontal = 3;
//            this.buttonItem56.ImagePaddingVertical = 0;
//            this.buttonItem56.Name = "buttonItem56";
//            this.buttonItem56.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
//            this.itemContainer1});
//            this.buttonItem56.SubItemsExpandWidth = 14;
//            this.buttonItem56.Text = "課程規劃";
//            this.buttonItem56.PopupOpen += new DevComponents.DotNetBar.DotNetBarManager.PopupOpenEventHandler(this.buttonItem56_PopupOpen);
//            // 
//            // itemContainer1
//            // 
//            this.itemContainer1.MinimumSize = new System.Drawing.Size(0, 0);
//            this.itemContainer1.Name = "itemContainer1";
//            this.itemContainer1.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
//            this.controlContainerItem1});
//            // 
//            // controlContainerItem1
//            // 
//            this.controlContainerItem1.AllowItemResize = true;
//            this.controlContainerItem1.Control = null;
//            this.controlContainerItem1.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
//            this.controlContainerItem1.Name = "controlContainerItem1";
//            this.controlContainerItem1.Text = "controlContainerItem1";
//            // 
//            // buttonItem65
//            // 
//            this.buttonItem65.AutoExpandOnClick = true;
//            this.buttonItem65.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
//            this.buttonItem65.Enabled = false;
//            this.buttonItem65.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem65.Image")));
//            this.buttonItem65.ImageFixedSize = new System.Drawing.Size(32, 32);
//            this.buttonItem65.ImagePaddingHorizontal = 3;
//            this.buttonItem65.ImagePaddingVertical = 0;
//            this.buttonItem65.Name = "buttonItem65";
//            this.buttonItem65.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
//            this.buttonItem71,
//            this.buttonItem72,
//            this.buttonItem73,
//            this.buttonItem74,
//            this.buttonItem75,
//            this.buttonItem76});
//            this.buttonItem65.SubItemsExpandWidth = 14;
//            this.buttonItem65.Text = "計算規則";
//            this.buttonItem65.PopupOpen += new DevComponents.DotNetBar.DotNetBarManager.PopupOpenEventHandler(this.buttonItem65_PopupOpen);
//            // 
//            // buttonItem71
//            // 
//            this.buttonItem71.ImagePaddingHorizontal = 8;
//            this.buttonItem71.Name = "buttonItem71";
//            this.buttonItem71.Text = "一般及格標準";
//            // 
//            // buttonItem72
//            // 
//            this.buttonItem72.ImagePaddingHorizontal = 8;
//            this.buttonItem72.Name = "buttonItem72";
//            this.buttonItem72.Text = "40.50.60";
//            // 
//            // buttonItem73
//            // 
//            this.buttonItem73.ImagePaddingHorizontal = 8;
//            this.buttonItem73.Name = "buttonItem73";
//            this.buttonItem73.Text = "40.40.50";
//            // 
//            // buttonItem74
//            // 
//            this.buttonItem74.ImagePaddingHorizontal = 8;
//            this.buttonItem74.Name = "buttonItem74";
//            this.buttonItem74.Text = "50.50.60";
//            // 
//            // buttonItem75
//            // 
//            this.buttonItem75.ImagePaddingHorizontal = 8;
//            this.buttonItem75.Name = "buttonItem75";
//            this.buttonItem75.Text = "所有成績及格/補考標準…";
//            // 
//            // buttonItem76
//            // 
//            this.buttonItem76.ImagePaddingHorizontal = 8;
//            this.buttonItem76.Name = "buttonItem76";
//            this.buttonItem76.Text = "設定快速點選及格標準";
//            // 
//            // itemContainer2
//            // 
//            this.itemContainer2.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
//            this.itemContainer2.MinimumSize = new System.Drawing.Size(0, 0);
//            this.itemContainer2.MultiLine = true;
//            this.itemContainer2.Name = "itemContainer2";
//            this.itemContainer2.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
//            this.buttonItem56,
//            this.buttonItem65});
//            // 
//            // AssignStudent
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.Name = "AssignStudent";
//            this.ResumeLayout(false);

//        }

//        #endregion

//        private DevComponents.DotNetBar.ButtonItem buttonItem56;
//        private DevComponents.DotNetBar.ItemContainer itemContainer1;
//        private DevComponents.DotNetBar.ControlContainerItem controlContainerItem1;
//        private DevComponents.DotNetBar.ButtonItem buttonItem65;
//        private DevComponents.DotNetBar.ButtonItem buttonItem71;
//        private DevComponents.DotNetBar.ButtonItem buttonItem72;
//        private DevComponents.DotNetBar.ButtonItem buttonItem73;
//        private DevComponents.DotNetBar.ButtonItem buttonItem74;
//        private DevComponents.DotNetBar.ButtonItem buttonItem75;
//        private DevComponents.DotNetBar.ButtonItem buttonItem76;
//        private DevComponents.DotNetBar.ItemContainer itemContainer2;
//    }
//}
