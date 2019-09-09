using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using DevComponents.DotNetBar.Controls;
using FISCA.DSAUtil;
using FISCA.Presentation;
using SmartSchool.Common;
using SmartSchool.Feature.ScoreCalcRule;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class MoralConductConfiguration : SmartSchool.Customization.PlugIn.Configure.ConfigurationItem
    {
        //private ButtonItem _SelectMoralConductItem;
        private XmlElement _MoralConductElement;
        //private List<Control> _Controls = new List<Control>();
        private List<IMoralConductInstance> _Instances = new List<IMoralConductInstance>();

        public MoralConductConfiguration()
        {
            InitializeComponent();
            _Instances.Add(this.periodAbsenceCalcRuleEditor1);
            _Instances.Add(this.basicScoreEditor1);
            _Instances.Add(this.appraiseRuleEditor1);
            _Instances.Add(this.rewardCalcRuleEditor1);
            //_Controls.AddRange(new Control[] { periodAbsenceCalcRuleEditor1, rewardCalcRuleEditor1, appraiseRuleEditor1 });
            Category = "成績作業";
        }

        protected override void OnActive()
        {
            //MotherForm.SetWaitCursor();
            _MoralConductElement = QueryScoreCalcRule.GetMoralConductCalcRule();
            if (_MoralConductElement == null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<MoralConductScoreCalcRule/>");
                _MoralConductElement = doc.DocumentElement;
            }
            DSXmlHelper helper = new DSXmlHelper(_MoralConductElement); 
            foreach (IMoralConductInstance item in _Instances)
            {
                item.SetSource(helper.GetElement(item.XPath));
                item.GetDependenceData();

                if(!item.IsValidate)
                    SetWarring(item,(GroupPanel)((Control)item).Parent);
                item.IsValidateChanged+=new EventHandler(item_IsValidateChanged);
            }
            //MotherForm.ResetWaitCursor();
        }

        private void SetWarring(IMoralConductInstance item, GroupPanel groupPanel)
        {
            if (!item.IsValidate)
            {
                //Resize圖片成16*16大小
                Bitmap b = new Bitmap(14, 14);
                using (Graphics g = Graphics.FromImage(b))
                    g.DrawImage(Properties.Resources.warning1, 0, 0, 14, 14);
                groupPanel.TitleImage = b;
            }
            else
                groupPanel.TitleImage = null;
        }

        void item_IsValidateChanged(object sender, EventArgs e)
        {
            SetWarring((IMoralConductInstance)sender, (GroupPanel)((Control)sender).Parent);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool allPass = true;
            foreach (IMoralConductInstance var in _Instances)
            {
                allPass &= var.IsValidate;
            }
            if (allPass)
            {
                foreach (IMoralConductInstance var in _Instances)
                {
                    XmlElement element = (XmlElement)_MoralConductElement.SelectSingleNode(var.XPath);
                    XmlElement newelement = var.GetSource();
                    if (element != null)
                    {
                        if (element.OwnerDocument != newelement.OwnerDocument)
                            newelement = (XmlElement)element.OwnerDocument.ImportNode(newelement, true);

                        _MoralConductElement.ReplaceChild(newelement, element);
                    }
                    else
                        _MoralConductElement.AppendChild(_MoralConductElement.OwnerDocument.ImportNode(newelement, true));
                }
                EditScoreCalcRule.SetMoralConductCalcRule(_MoralConductElement);

                foreach (IMoralConductInstance var in _Instances)
                {
                    var.SetDependenceData();
                }
                MotherForm.SetStatusBarMessage("德行成績規則儲存完成。");
                OnActive();
            }
            else
                MsgBox.Show("資料有誤，無法儲存。\n請檢查輸入資料。");
        }

        private void contentPanel_SizeChanged(object sender, EventArgs e)
        {
            this.cardPanelEx1.Location = new Point(0, 0);
            this.cardPanelEx1.Size = contentPanel.Size;
        }

        private void basicScoreEditor1_IsDirtyChanged(object sender, EventArgs e)
        {
            linkLabel1.Visible = basicScoreEditor1.IsDirty && basicScoreEditor1.IsValidate;
        }

        private void appraiseRuleEditor1_IsDirtyChanged(object sender, EventArgs e)
        {
            linkLabel2.Visible = appraiseRuleEditor1.IsDirty && appraiseRuleEditor1.IsValidate;
        }

        private void periodAbsenceCalcRuleEditor1_IsDirtyChanged(object sender, EventArgs e)
        {
            linkLabel3.Visible = periodAbsenceCalcRuleEditor1.IsDirty && periodAbsenceCalcRuleEditor1.IsValidate;
        }

        private void rewardCalcRuleEditor1_IsDirtyChanged(object sender, EventArgs e)
        {
            linkLabel4.Visible = rewardCalcRuleEditor1.IsDirty && rewardCalcRuleEditor1.IsValidate;
        }


        //private void itemPanel2_ItemClick(object sender, EventArgs e)
        //{
        //    if (_SelectMoralConductItem != null)
        //        _SelectMoralConductItem.Checked = false;
        //    if (sender == null)
        //    {
        //        _SelectMoralConductItem = btnPeriodAbsenceCalcRule;
        //        btnPeriodAbsenceCalcRule.Checked = true;
        //    }
        //    else
        //    {
        //        ((ButtonItem)sender).Checked = true;
        //    }
        //    periodAbsenceCalcRuleEditor1.Visible = btnPeriodAbsenceCalcRule.Checked;
        //}

        //private void periodAbsenceCalcRuleEditor1_IsValidateChanged(object sender, EventArgs e)
        //{
        //    if (periodAbsenceCalcRuleEditor1.IsValidate)
        //    {
        //        btnPeriodAbsenceCalcRule.Tooltip = "";
        //        btnPeriodAbsenceCalcRule.ButtonStyle = eButtonStyle.TextOnlyAlways;
        //        btnPeriodAbsenceCalcRule.Image = null;
        //        btnPeriodAbsenceCalcRule.Refresh();
        //    }
        //    else
        //    {
        //        btnPeriodAbsenceCalcRule.ButtonStyle = eButtonStyle.ImageAndText;
        //        btnPeriodAbsenceCalcRule.Tooltip = "驗證失敗，請檢查內容。\n否則使用此規劃表之學生將無法加入修課。";
        //        btnPeriodAbsenceCalcRule.Image = Properties.Resources.warning1;
        //        btnPeriodAbsenceCalcRule.Refresh();
        //    }
        //}

        //private void buttonX4_Click(object sender, EventArgs e)
        //{
        //    if (btnPeriodAbsenceCalcRule.ButtonStyle == eButtonStyle.ImageAndText)
        //    {
        //        MsgBox.Show("輸入資料不完整，請檢查資料。");
        //        return;
        //    }
        //    #region 建立一個新的節點
        //    XmlDocument doc = new XmlDocument();
        //    doc.LoadXml("<MoralConductScoreCalcRule/>");
        //    XmlElement periodAbsenceCalcRule = periodAbsenceCalcRuleEditor1.GetSource();
        //    if (periodAbsenceCalcRule.Name != "PeriodAbsenceCalcRule")
        //        throw new Exception("PeriodAbsenceCalcRuleEditor1傳回的節點名稱錯誤");
        //    doc.DocumentElement.AppendChild(doc.ImportNode(periodAbsenceCalcRule, true));
        //    #endregion
        //    //船上去
        //    EditScoreCalcRule.SetMoralConductCalcRule(doc.DocumentElement);
        //    //重新讀下來
        //    _MoralConductElement = QueryScoreCalcRule.GetMoralConductCalcRule();
        //    periodAbsenceCalcRuleEditor1.SetSource((XmlElement)_MoralConductElement.SelectSingleNode("PeriodAbsenceCalcRule"));
        //}

        //private void btnPeriodAbsenceCalcRule_CheckedChanged(object sender, EventArgs e)
        //{
        //    foreach (Control var in _Controls)
        //    {
        //        var.Visible = false;
        //    }
        //    if (btnPeriodAbsenceCalcRule.Checked)
        //    {
        //        if (_MoralConductElement != null)
        //        {
        //            periodAbsenceCalcRuleEditor1.SetSource((XmlElement)_MoralConductElement.SelectSingleNode("PeriodAbsenceCalcRule"));
        //        }
        //        else
        //            periodAbsenceCalcRuleEditor1.SetSource(null);
        //    }
        //    else
        //        periodAbsenceCalcRuleEditor1.SetSource(null);
        //}

        //private void buttonItem1_CheckedChanged(object sender, EventArgs e)
        //{
        //    foreach (Control var in _Controls)
        //    {
        //        var.Visible = false;
        //    }
        //    if (buttonItem1.Checked)
        //        rewardCalcRuleEditor1.Visible = true;
        //}

        //private void buttonItem2_CheckedChanged(object sender, EventArgs e)
        //{
        //    foreach (Control var in _Controls)
        //    {
        //        var.Visible = false;
        //    }
        //    if (buttonItem2.Checked)
        //        appraiseRuleEditor1.Visible = true;
        //}
    }
}
