using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Common.Validate;
using SmartSchool.Common;
using System.Threading;
using System.Xml;
using SmartSchool.Evaluation.GraduationPlan.Editor;
using SmartSchool.ExceptionHandler;

namespace SmartSchool.Evaluation.GraduationPlan.Validate
{
    public class ValidateGraduationPlanInfo : IValidater<GraduationPlanInfo>
    {
        private List<IValidater<GraduationPlanInfo>> _ExtendValidater = new List<IValidater<GraduationPlanInfo>>();

        private AutoResetEvent _OneTimeOneCheck = new AutoResetEvent(true);

        private GraduationPlanEditor _Editor;

        static private List<GraduationPlanInfo> _PassedList = new List<GraduationPlanInfo>();


        public ValidateGraduationPlanInfo(params IValidater<GraduationPlanInfo>[] extendValidaters)
        {
            _Editor = new SmartSchool.Evaluation.GraduationPlan.Editor.GraduationPlanEditor();
            _Editor.SuspendLayout();
            ExtendValidater.AddRange( extendValidaters);
        }

        #region IValidater<GraduationPlanInfo> 成員

        public bool Validate(GraduationPlanInfo info, IErrorViewer responseViewer)
        {
            _OneTimeOneCheck.WaitOne();
            bool pass = true;
            try
            {
                if ( !_PassedList.Contains(info) )
                {
                    _Editor.SetSource(info.GraduationPlanElement);
                    pass &= _Editor.IsValidated;
                    pass &= _Editor.GetSource().SelectNodes("Subject").Count == info.GraduationPlanElement.SelectNodes("Subject").Count;
                    if ( pass )
                    {
                        foreach ( XmlNode var in _Editor.GetSource().SelectNodes("Subject") )
                        {
                            XmlElement subject1 = (XmlElement)var;
                            XmlElement subject2 = (XmlElement)info.GraduationPlanElement.SelectSingleNode("Subject[@SubjectName='" + subject1.GetAttribute("SubjectName") + "' and @Level='" + subject1.GetAttribute("Level") + "']");
                            if ( subject2 != null )
                            {
                                foreach ( XmlAttribute attributeInfo in subject1.Attributes )
                                {
                                    if ( subject1.GetAttribute(attributeInfo.Name) != subject2.GetAttribute(attributeInfo.Name) )
                                    {
                                        pass = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                pass = false;
                                break;
                            }
                        }
                    }
                    if ( pass )
                        _PassedList.Add(info);
                }
                if ( pass )
                {
                    foreach ( IValidater<GraduationPlanInfo> extendValidater in _ExtendValidater )
                    {
                        pass &= extendValidater.Validate(info, responseViewer);
                    }
                }
                else
                {
                    if ( responseViewer != null )
                        responseViewer.SetMessage("課程規畫表：\"" + info.Name + "\"驗證失敗");
                    pass = false;
                }
                _OneTimeOneCheck.Set();
            }
            catch ( Exception ex )
            {
                if ( responseViewer != null )
                    responseViewer.SetMessage("課程規畫表：\"" + info.Name + "\"在驗證過程中發生未預期錯誤");
                BugReporter.ReportException("SmartSchool", CurrentUser.Instance.SystemVersion, ex, false);
                _OneTimeOneCheck.Set();
                return false;
            }
            return pass;
        }

        public List<IValidater<GraduationPlanInfo>> ExtendValidater
        {
            get { return _ExtendValidater; }
        }

        #endregion
    }
}
