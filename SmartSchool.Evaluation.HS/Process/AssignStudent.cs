using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
//using SmartSchool.StudentRelated;
using DevComponents.DotNetBar;
//using SmartSchool.ClassRelated;
using FISCA.DSAUtil;
using SmartSchool.Feature.Class;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.Evaluation.ScoreCalcRule;
using SmartSchool.Common;
using SmartSchool.Customization.Data;
using SmartSchool.StudentRelated;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Process
{
    public partial class AssignStudent : RibbonBarBase
    {
        public AssignStudent()
        {
            //InitializeComponent();
            ////SmartSchool.StudentRelated.Student.Instance.SelectionChanged+=new EventHandler(Instance_SelectionChanged);
            //SmartSchool.Broadcaster.Events.Items["學生/選取變更"].Handler += delegate
            //{
            //    buttonItem65.Enabled = buttonItem56.Enabled = new AccessHelper().StudentHelper.GetSelectedStudent().Count > 0;
            //    if (!CurrentUser.Acl["Button0113"].Executable)
            //        buttonItem56.Enabled = false;
            //    if (!CurrentUser.Acl["Button0116"].Executable)
            //        buttonItem65.Enabled = false;
            //};
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonItem>.Instance["學生/指定"].Add(buttonItem56);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonItem>.Instance["學生/指定"].Add(buttonItem65);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssignStudent));
            var buttonItem56 = K12.Presentation.NLDPanels.Student.RibbonBarItems["指定"]["課程規劃"];
            buttonItem56.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem56.Image") ) );
            buttonItem56.Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0 && CurrentUser.Acl["Button0113"].Executable;
            buttonItem56.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(buttonItem56_PopupOpen);
            buttonItem56.SupposeHasChildern = true;
            var buttonItem65 = K12.Presentation.NLDPanels.Student.RibbonBarItems["指定"]["計算規則"];
            buttonItem65.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem65.Image") ) );
            buttonItem65.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(buttonItem65_PopupOpen);
            buttonItem65.Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0 && CurrentUser.Acl["Button0116"].Executable;
            buttonItem65.SupposeHasChildern = true;
            K12.Presentation.NLDPanels.Student.SelectedSourceChanged+= delegate
            {
                buttonItem56.Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0 && CurrentUser.Acl["Button0113"].Executable;
                buttonItem65.Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0 && CurrentUser.Acl["Button0116"].Executable;

            };
        }

        //void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    IsButtonEnable();

        //    if (!CurrentUser.Acl["Button0113"].Executable)
        //        buttonItem56.Enabled = false;
        //    if (!CurrentUser.Acl["Button0116"].Executable)
        //        buttonItem65.Enabled = false;
        //}

        //private bool IsButtonEnable()
        //{
        //    buttonItem56.Enabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
        //    buttonItem65.Enabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
        //    return true;
        //}

        #region 課程規劃
        //private void buttonItem56_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        //{
        //    GraduationPlanSelector selector = GraduationPlan.GraduationPlan.Instance.GetSelector();
        //    selector.GraduationPlanSelected += new EventHandler<GraduationPlanSelectedEventArgs>(selector_GraduationPlanSelected);
        //    #region 插入不指定按鈕
        //    DevComponents.DotNetBar.ButtonX item = new DevComponents.DotNetBar.ButtonX();
        //    item.Text = "不指定";
        //    item.Tooltip = "參照所屬班級設定，\n而不直接指定學生課程規劃表，\n在學生更換班級或變更班級的課程規劃表設定時，\n學生將直接採用新的班級課程規劃表設定。";
        //    item.TextAlignment = eButtonTextAlignment.Left;
        //    item.ColorTable = eButtonColor.OrangeWithBackground;
        //    item.Size = new Size(140, 23);
        //    item.Click += new EventHandler(item_Click);
        //    selector.Controls[0].Controls.Add(item);
        //    selector.Controls[0].Controls.SetChildIndex(item, 0);
        //    #endregion
        //    controlContainerItem1.Control = selector;
        //    //controlContainerItem1.RecalcSize();
        //}


        void buttonItem56_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            var item = e.VirtualButtons["不指定"];
            item.Click += new EventHandler(item_Click);
            bool b = true;
            foreach ( GraduationPlanInfo info in SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Items )
            {
                var btn = e.VirtualButtons[info.Name];
                btn.Tag = info;
                if ( b )
                {
                    b = false;
                    btn.BeginGroup = true;
                }
                btn.Click += new EventHandler(btn_Click);
            }
        }

        void btn_Click(object sender, EventArgs e)
        {
            AccessHelper accessHelper = new AccessHelper();
            if ( accessHelper.StudentHelper.GetSelectedStudent().Count > 0 )
            {
                string ErrorMessage = "";
                try
                {
                    var info = ( (MenuButton)sender ).Tag as GraduationPlanInfo;
                    DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");
                    helper.AddElement("Student");
                    helper.AddElement("Student", "Field");
                    helper.AddElement("Student/Field", "RefGraduationPlanID", info.ID);
                    helper.AddElement("Student", "Condition");
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        helper.AddElement("Student/Condition", "ID", studentInfo.StudentID);
                    }
                    SmartSchool.Feature.EditStudent.Update(new DSRequest(helper));

                    //log
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        CurrentUser.Instance.AppLog.Write(
                            SmartSchool.ApplicationLog.EntityType.Student,
                            "指定學生課程規劃",
                            studentInfo.StudentID,
                            string.Format("指定「{0}」採用課程規劃：{1}", studentInfo.StudentName + ( string.IsNullOrEmpty(studentInfo.StudentNumber) ? "" : " (" + studentInfo.StudentNumber + ")" ), info.Name),
                            "學生",
                            string.Format("學生ID: {0}，課程規劃ID: {1}", studentInfo.StudentID, info.ID));
                    }
                }
                catch
                {
                    GraduationPlan.GraduationPlan.Instance.LoadStudentReference();
                    EventHub.Instance.InvokeStudentReferenceGranduationPlanChanged();
                    MsgBox.Show("設定課程規劃表發生錯誤。");
                    return;
                }
                GraduationPlan.GraduationPlan.Instance.LoadStudentReference();
                EventHub.Instance.InvokeStudentReferenceGranduationPlanChanged();
                Global.SetStatusBarMessage("課程規劃表設定完成");
                //MsgBox.Show("課程規劃表設定完成");
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            AccessHelper accessHelper = new AccessHelper();
            if ( accessHelper.StudentHelper.GetSelectedStudent().Count > 0 )
            {
                string ErrorMessage = "";
                try
                {
                    DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");
                    helper.AddElement("Student");
                    helper.AddElement("Student", "Field");
                    helper.AddElement("Student/Field", "RefGraduationPlanID", "");
                    helper.AddElement("Student", "Condition");
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        helper.AddElement("Student/Condition", "ID", studentInfo.StudentID);
                    }
                    SmartSchool.Feature.EditStudent.Update(new DSRequest(helper));

                    //log
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        CurrentUser.Instance.AppLog.Write(
                            SmartSchool.ApplicationLog.EntityType.Student,
                            "指定學生課程規劃",
                            studentInfo.StudentID,
                            string.Format("指定「{0}」採用課程規劃：{1}", studentInfo.StudentName + ( string.IsNullOrEmpty(studentInfo.StudentNumber) ? "" : " (" + studentInfo.StudentNumber + ")" ), "<不指定>"),
                            "學生",
                            string.Format("學生ID: {0}，課程規劃ID: {1}", studentInfo.StudentID, "<空白>"));
                    }
                }
                catch
                {
                    GraduationPlan.GraduationPlan.Instance.LoadStudentReference();
                    EventHub.Instance.InvokeStudentReferenceGranduationPlanChanged();
                    MsgBox.Show("設定程規劃表發生錯誤。");
                    return;
                }
                GraduationPlan.GraduationPlan.Instance.LoadStudentReference();
                EventHub.Instance.InvokeStudentReferenceGranduationPlanChanged();
                Global.SetStatusBarMessage("課程規劃表設定完成");
                //MsgBox.Show("課程規劃表設定完成");
            }
        }

        //void selector_GraduationPlanSelected(object sender, GraduationPlanSelectedEventArgs e)
        //{
            //AccessHelper accessHelper = new AccessHelper();
            //if (accessHelper.StudentHelper.GetSelectedStudent().Count > 0)
            //{
            //    string ErrorMessage = "";
            //    try
            //    {
            //        DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");
            //        helper.AddElement("Student");
            //        helper.AddElement("Student", "Field");
            //        helper.AddElement("Student/Field", "RefGraduationPlanID", e.Item.ID);
            //        helper.AddElement("Student", "Condition");
            //        foreach (StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent())
            //        {
            //            helper.AddElement("Student/Condition", "ID", studentInfo.StudentID);
            //        }
            //        SmartSchool.Feature.EditStudent.Update(new DSRequest(helper));

            //        //log
            //        foreach (StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent())
            //        {
            //            CurrentUser.Instance.AppLog.Write(
            //                SmartSchool.ApplicationLog.EntityType.Student,
            //                "指定學生課程規劃",
            //                studentInfo.StudentID,
            //                string.Format("指定「{0}」採用課程規劃：{1}", studentInfo.StudentName + (string.IsNullOrEmpty(studentInfo.StudentNumber) ? "" : " (" + studentInfo.StudentNumber + ")"), e.Item.Name),
            //                "學生",
            //                string.Format("學生ID: {0}，課程規劃ID: {1}", studentInfo.StudentID, e.Item.ID));
            //        }
            //    }
            //    catch
            //    {
            //        GraduationPlan.GraduationPlan.Instance.LoadStudentReference();
            //        EventHub.Instance.InvokeStudentReferenceGranduationPlanChanged();
            //        MsgBox.Show("設定課程規劃表發生錯誤。");
            //        return;
            //    }
            //    GraduationPlan.GraduationPlan.Instance.LoadStudentReference();
            //    EventHub.Instance.InvokeStudentReferenceGranduationPlanChanged();
            //    Global.SetStatusBarMessage("課程規劃表設定完成");
            //    //MsgBox.Show("課程規劃表設定完成");
            //}
        //}
        #endregion

        #region 計算規則

        //private void buttonItem65_PopupOpen(object sender, PopupOpenEventArgs e)
        //{
        //    foreach ( ButtonItem var in buttonItem65.SubItems )
        //    {
        //        var.Click -= new EventHandler(item_Click2);
        //    }

        //    buttonItem65.SubItems.Clear();
        //    #region 插入不指定按鈕
        //    ButtonItem item = new ButtonItem("不指定", "不指定");
        //    item.Tooltip = "參照所屬班級設定，\n而不直接指定學生計算規則，\n在學生更換班級或變更班級的計算規則設定時，\n學生將直接採用新的班級計算規則設定。";
        //    item.Click += new EventHandler(item_Click3);
        //    buttonItem65.SubItems.Add(item);
        //    #endregion
        //    foreach ( SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRuleInfo var in SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRule.Instance.Items )
        //    {
        //        item = new ButtonItem(var.ID, var.Name);
        //        item.Tag = var;
        //        item.Click += new EventHandler(item_Click2);
        //        buttonItem65.SubItems.Add(item);
        //    }
        //}

        void buttonItem65_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            var item = e.VirtualButtons["不指定"];
            item.Click += new EventHandler(item_Click3);
            bool b=true;
            foreach ( SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRuleInfo var in SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRule.Instance.Items )
            {
                var btn = e.VirtualButtons[var.Name];
                if ( b )
                {
                    b = false;
                    btn.BeginGroup = true;
                }
                btn.Tag = var;
                btn.Click += new EventHandler(item_Click2);
            }
        }

        void item_Click3(object sender, EventArgs e)
        {
            AccessHelper accessHelper = new AccessHelper();
            if ( accessHelper.StudentHelper.GetSelectedStudent().Count > 0 )
            {
                try
                {
                    //MenuButton item = sender as MenuButton;
                    DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");
                    helper.AddElement("Student");
                    helper.AddElement("Student", "Field");
                    helper.AddElement("Student/Field", "RefScoreCalcRuleID", "");
                    helper.AddElement("Student", "Condition");
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        helper.AddElement("Student/Condition", "ID", studentInfo.StudentID);
                    }
                    SmartSchool.Feature.EditStudent.Update(new DSRequest(helper));

                    //log
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        CurrentUser.Instance.AppLog.Write(
                            SmartSchool.ApplicationLog.EntityType.Student,
                            "指定學生計算規則",
                            studentInfo.StudentID,
                            string.Format("指定「{0}」採用計算規則：{1}", studentInfo.StudentName + ( string.IsNullOrEmpty(studentInfo.StudentNumber) ? "" : " (" + studentInfo.StudentNumber + ")" ), "<不指定>"),
                            "學生",
                            string.Format("學生ID: {0}，計算規則ID: {1}", studentInfo.StudentID, "<空白>"));
                    }
                }
                catch
                {
                    ScoreCalcRule.ScoreCalcRule.Instance.LoadStudentReference();
                    EventHub.Instance.InvokeStudentReferenceCaleRuleChanged();
                    MsgBox.Show("設定計算規則發生錯誤。");
                    return;
                }
                ScoreCalcRule.ScoreCalcRule.Instance.LoadStudentReference();
                EventHub.Instance.InvokeStudentReferenceCaleRuleChanged();
                Global.SetStatusBarMessage("計算規則設定完成");
                //MsgBox.Show("計算規則設定完成");
            }
        }

        void item_Click2(object sender, EventArgs e)
        {
            AccessHelper accessHelper = new AccessHelper();
            if ( accessHelper.StudentHelper.GetSelectedStudent().Count > 0 )
            {
                try
                {
                    MenuButton item = sender as MenuButton;
                    DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");
                    helper.AddElement("Student");
                    helper.AddElement("Student", "Field");
                    helper.AddElement("Student/Field", "RefScoreCalcRuleID", ( item.Tag as ScoreCalcRuleInfo ).ID);
                    helper.AddElement("Student", "Condition");
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        helper.AddElement("Student/Condition", "ID", studentInfo.StudentID);
                    }
                    SmartSchool.Feature.EditStudent.Update(new DSRequest(helper));

                    //log
                    foreach ( StudentRecord studentInfo in accessHelper.StudentHelper.GetSelectedStudent() )
                    {
                        CurrentUser.Instance.AppLog.Write(
                            SmartSchool.ApplicationLog.EntityType.Student,
                            "指定學生計算規則",
                            studentInfo.StudentID,
                            string.Format("指定「{0}」採用計算規則：{1}", studentInfo.StudentName + ( string.IsNullOrEmpty(studentInfo.StudentNumber) ? "" : " (" + studentInfo.StudentNumber + ")" ), ( item.Tag as ScoreCalcRuleInfo ).Name),
                            "學生",
                            string.Format("學生ID: {0}，計算規則ID: {1}", studentInfo.StudentID, ( item.Tag as ScoreCalcRuleInfo ).ID));
                    }
                }
                catch
                {
                    ScoreCalcRule.ScoreCalcRule.Instance.LoadStudentReference();
                    EventHub.Instance.InvokeStudentReferenceCaleRuleChanged();
                    MsgBox.Show("設定計算規則發生錯誤。");
                    return;
                }
                ScoreCalcRule.ScoreCalcRule.Instance.LoadStudentReference();
                EventHub.Instance.InvokeStudentReferenceCaleRuleChanged();
                Global.SetStatusBarMessage("計算規則設定完成");
                //MsgBox.Show("計算規則設定完成");
            }
        }
        #endregion
    }
}
