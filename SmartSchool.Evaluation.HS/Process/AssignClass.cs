using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.StudentRelated;
using DevComponents.DotNetBar;
using SmartSchool.ClassRelated;
using FISCA.DSAUtil;
using SmartSchool.Feature.Class;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.Evaluation.ScoreCalcRule;
using SmartSchool.Common;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Process
{
    public partial class AssignClass : RibbonBarBase
    {
        public AssignClass()
        {
            //InitializeComponent();
            ////SmartSchool.ClassRelated.Class.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);

            //SmartSchool.Broadcaster.Events.Items["班級/選取變更"].Handler += delegate
            //{
            //    IsButtonEnable();

            //    if (!CurrentUser.Acl["Button0390"].Executable)
            //        buttonItem56.Enabled = false;
            //    if (!CurrentUser.Acl["Button0400"].Executable)
            //        buttonItem65.Enabled = false;
            //};

            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonItem>.Instance["班級/指定"].Add(buttonItem56);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonItem>.Instance["班級/指定"].Add(buttonItem65);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssignClass));
            var buttonItem56 = K12.Presentation.NLDPanels.Class.RibbonBarItems["指定"]["課程規劃"];
            buttonItem56.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem56.Image") ) );
            buttonItem56.Enable = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 && CurrentUser.Acl["Button0390"].Executable;
            buttonItem56.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(buttonItem56_PopupOpen);
            buttonItem56.SupposeHasChildern = true;

            var buttonItem65 = K12.Presentation.NLDPanels.Class.RibbonBarItems["指定"]["計算規則"];
            buttonItem65.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem65.Image") ) );
            buttonItem65.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(buttonItem65_PopupOpen);
            buttonItem65.Enable = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 && CurrentUser.Acl["Button0400"].Executable;
            buttonItem65.SupposeHasChildern = true;

            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
            {
                buttonItem56.Enable = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 && CurrentUser.Acl["Button0390"].Executable;
                buttonItem65.Enable = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 && CurrentUser.Acl["Button0400"].Executable;
            };
        }

        void buttonItem65_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            foreach ( SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRuleInfo var in SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRule.Instance.Items )
            {
                var item = e.VirtualButtons[var.Name];
                item.Tag = var;
                item.Click += new EventHandler(item_Click);
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 )
            {
                try
                {
                    MenuButton item = sender as MenuButton;
                    DSXmlHelper helper = new DSXmlHelper("UpdateRequest");
                    helper.AddElement("Class");
                    helper.AddElement("Class", "Field");
                    helper.AddElement("Class/Field", "RefScoreCalcRuleID", ( item.Tag as ScoreCalcRuleInfo ).ID);
                    helper.AddElement("Class", "Condition");
                    foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
                    {
                        helper.AddElement("Class/Condition", "ID", classinfo.ClassID);
                    }
                    EditClass.Update(new DSRequest(helper));

                    //log
                    foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
                    {
                        CurrentUser.Instance.AppLog.Write(
                            SmartSchool.ApplicationLog.EntityType.Class,
                            "指定班級計算規則",
                            classinfo.ClassID,
                            string.Format("指定「{0}」採用計算規則：{1}", classinfo.ClassName, ( item.Tag as ScoreCalcRuleInfo ).Name),
                            "班級",
                            string.Format("班級ID: {0}，計算規則ID: {1}", classinfo.ClassID, ( item.Tag as ScoreCalcRuleInfo ).ID));
                    }
                }
                catch
                {
                    ScoreCalcRule.ScoreCalcRule.Instance.LoadClassReference();
                    EventHub.Instance.InvokeClassReferenceCaleRuleChanged();
                    MsgBox.Show("設定計算規則發生錯誤。");
                    return;
                }
                ScoreCalcRule.ScoreCalcRule.Instance.LoadClassReference();
                EventHub.Instance.InvokeClassReferenceCaleRuleChanged();
                //MsgBox.Show("計算規則設定完成");
            }
        }

        void buttonItem56_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            foreach ( GraduationPlanInfo info in SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Items )
            {
                var btn = e.VirtualButtons[info.Name];
                btn.Tag = info;
                btn.Click += new EventHandler(btn_Click);
            }
        }

        void btn_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 )
            {
                string ErrorMessage = "";
                try
                {
                    //var ID = "" + ( (MenuButton)sender ).Tag;
                    var info = ( (MenuButton)sender ).Tag as GraduationPlanInfo;
                    var ID = info.ID;
                    DSXmlHelper helper = new DSXmlHelper("UpdateRequest");
                    helper.AddElement("Class");
                    helper.AddElement("Class", "Field");
                    helper.AddElement("Class/Field", "RefGraduationPlanID", ID);
                    helper.AddElement("Class", "Condition");
                    foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
                    {
                        helper.AddElement("Class/Condition", "ID", classinfo.ClassID);
                    }
                    EditClass.Update(new DSRequest(helper));

                    //log
                    foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
                    {
                        CurrentUser.Instance.AppLog.Write(
                            SmartSchool.ApplicationLog.EntityType.Class,
                            "指定班級課程規劃",
                            classinfo.ClassID,
                            string.Format("指定「{0}」採用課程規劃：{1}", classinfo.ClassName, info.Name),
                            "班級",
                            string.Format("班級ID: {0}，課程規劃ID: {1}", classinfo.ClassID, ID));
                    }
                }
                catch
                {
                    GraduationPlan.GraduationPlan.Instance.LoadClassReference();
                    EventHub.Instance.InvokeClassReferenceGranduationPlanChanged();
                    MsgBox.Show("設定班級課程規劃表發生錯誤。");
                    return;
                }
                GraduationPlan.GraduationPlan.Instance.LoadClassReference();
                EventHub.Instance.InvokeClassReferenceGranduationPlanChanged();
                //MsgBox.Show("課程規劃表設定完成");
            }
        }

        //void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    IsButtonEnable();

        //    if (!CurrentUser.Acl["Button0390"].Executable)
        //        buttonItem56.Enabled = false;
        //    if (!CurrentUser.Acl["Button0400"].Executable)
        //        buttonItem65.Enabled = false;
        //}

        //private bool IsButtonEnable()
        //{
        //    buttonItem56.Enabled = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0;
        //    buttonItem65.Enabled = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0;
        //    return true;
        //}

        //#region 課程規劃
        //private void buttonItem56_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        //{
        //    GraduationPlanSelector selector = GraduationPlan.GraduationPlan.Instance.GetSelector();
        //    selector.GraduationPlanSelected += new EventHandler<GraduationPlanSelectedEventArgs>(selector_GraduationPlanSelected);
        //    controlContainerItem1.Control = selector;
        //    //controlContainerItem1.RecalcSize();
        //}

        //void selector_GraduationPlanSelected(object sender, GraduationPlanSelectedEventArgs e)
        //{

        //    if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 )
        //    {
        //        string ErrorMessage = "";
        //        try
        //        {
        //            DSXmlHelper helper = new DSXmlHelper("UpdateRequest");
        //            helper.AddElement("Class");
        //            helper.AddElement("Class", "Field");
        //            helper.AddElement("Class/Field", "RefGraduationPlanID", e.Item.ID);
        //            helper.AddElement("Class", "Condition");
        //            foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
        //            {
        //                helper.AddElement("Class/Condition", "ID", classinfo.ClassID);
        //            }
        //            EditClass.Update(new DSRequest(helper));

        //            //log
        //            foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
        //            {
        //                CurrentUser.Instance.AppLog.Write(
        //                    SmartSchool.ApplicationLog.EntityType.Class,
        //                    "指定班級課程規劃",
        //                    classinfo.ClassID,
        //                    string.Format("指定「{0}」採用課程規劃：{1}", classinfo.ClassName, e.Item.Name),
        //                    "班級",
        //                    string.Format("班級ID: {0}，課程規劃ID: {1}", classinfo.ClassID, e.Item.ID));
        //            }
        //        }
        //        catch
        //        {
        //            GraduationPlan.GraduationPlan.Instance.LoadClassReference();
        //            EventHub.Instance.InvokeClassReferenceGranduationPlanChanged();
        //            MsgBox.Show("設定班級課程規劃表發生錯誤。");
        //            return;
        //        }
        //        GraduationPlan.GraduationPlan.Instance.LoadClassReference();
        //        EventHub.Instance.InvokeClassReferenceGranduationPlanChanged();
        //        //MsgBox.Show("課程規劃表設定完成");
        //    }
        //}
        //#endregion

        //#region 計算規則

        //private void buttonItem65_PopupOpen(object sender, PopupOpenEventArgs e)
        //{
        //    foreach ( ButtonItem var in buttonItem65.SubItems )
        //    {
        //        var.Click -= new EventHandler(item_Click2);
        //    }

        //    buttonItem65.SubItems.Clear();
        //    foreach ( SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRuleInfo var in SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRule.Instance.Items )
        //    {
        //        ButtonItem item = new ButtonItem(var.ID, var.Name);
        //        item.Tag = var;
        //        item.Click += new EventHandler(item_Click2);
        //        buttonItem65.SubItems.Add(item);
        //    }
        //}

        //void item_Click2(object sender, EventArgs e)
        //{
        //    if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 )
        //    {
        //        try
        //        {
        //            ButtonItem item = sender as ButtonItem;
        //            DSXmlHelper helper = new DSXmlHelper("UpdateRequest");
        //            helper.AddElement("Class");
        //            helper.AddElement("Class", "Field");
        //            helper.AddElement("Class/Field", "RefScoreCalcRuleID", ( item.Tag as ScoreCalcRuleInfo ).ID);
        //            helper.AddElement("Class", "Condition");
        //            foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
        //            {
        //                helper.AddElement("Class/Condition", "ID", classinfo.ClassID);
        //            }
        //            EditClass.Update(new DSRequest(helper));

        //            //log
        //            foreach ( ClassInfo classinfo in SmartSchool.ClassRelated.Class.Instance.SelectionClasses )
        //            {
        //                CurrentUser.Instance.AppLog.Write(
        //                    SmartSchool.ApplicationLog.EntityType.Class,
        //                    "指定班級計算規則",
        //                    classinfo.ClassID,
        //                    string.Format("指定「{0}」採用計算規則：{1}", classinfo.ClassName, ( item.Tag as ScoreCalcRuleInfo ).Name),
        //                    "班級",
        //                    string.Format("班級ID: {0}，計算規則ID: {1}", classinfo.ClassID, ( item.Tag as ScoreCalcRuleInfo ).ID));
        //            }
        //        }
        //        catch
        //        {
        //            ScoreCalcRule.ScoreCalcRule.Instance.LoadClassReference();
        //            EventHub.Instance.InvokeClassReferenceCaleRuleChanged();
        //            MsgBox.Show("設定計算規則發生錯誤。");
        //            return;
        //        }
        //        ScoreCalcRule.ScoreCalcRule.Instance.LoadClassReference();
        //        EventHub.Instance.InvokeClassReferenceCaleRuleChanged();
        //        //MsgBox.Show("計算規則設定完成");
        //    }
        //}
        //#endregion
    }
}
