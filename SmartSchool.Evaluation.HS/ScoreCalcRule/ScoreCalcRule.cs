using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Customization.Data;
using SmartSchool.Feature.ScoreCalcRule;

namespace SmartSchool.Evaluation.ScoreCalcRule
{
    public class ScoreCalcRule
    {
        private static ScoreCalcRule _Instance;
        public static ScoreCalcRule Instance
        {
            get
            {
                if (_Instance == null)
                    throw new Exception("未建構");
                return _Instance;
            }
        }

        private AccessHelper accessHelper = new AccessHelper();

        private Dictionary<string, ScoreCalcRuleInfo> _Items;
        public ScoreCalcRuleInfoCollection Items
        {
            get
            {
                _LoadingEvent.WaitOne();
                return new ScoreCalcRuleInfoCollection(_Items);
            }
        }

        public bool IsStudentOverrided(string id)
        {
            _LoadingStudentReferenceEvent.WaitOne();
            return _StudentReference.ContainsKey(id);
        }

        public ScoreCalcRuleInfo GetStudentScoreCalcRuleInfo(string id)
        {
            _LoadingStudentReferenceEvent.WaitOne();
            if (_StudentReference.ContainsKey(id))//自己有救回傳
                return Items[_StudentReference[id]];
            else
            {
                if (accessHelper.StudentHelper.GetStudents(id)[0].RefClass != null)//有所屬班級就抓所屬班級的
                    return GetClassScoreCalcRuleInfo(accessHelper.StudentHelper.GetStudents(id)[0].RefClass.ClassID);
                else
                    return null;//自己沒有也沒有所屬班級就回傳nulll
            }
            return null;
        }

        public ScoreCalcRuleInfo GetClassScoreCalcRuleInfo(string id)
        {
            _LoadingClassReferenceEvent.WaitOne();
            if (_ClassReference.ContainsKey(id))//有救回傳
                return Items[_ClassReference[id]];
            else
                return null;//沒有拉倒
        }

        private bool _LoadScoreCalcRuleAgain = false;
        private BackgroundWorker _BGWScoreCalcRuleLoader;
        private ManualResetEvent _LoadingEvent;

        private bool _LoadStudentReferenceAgain = false;
        private BackgroundWorker _BGWStudentReferenceLoader;
        private ManualResetEvent _LoadingStudentReferenceEvent;

        private bool _LoadClassReferenceAgain = false;
        private BackgroundWorker _BGWClassReferenceLoader;
        private ManualResetEvent _LoadingClassReferenceEvent;

        private Dictionary<string, string> _StudentReference;

        private Dictionary<string, string> _ClassReference;

        public static void CreateInstance()
        {
            _Instance = new ScoreCalcRule();
        }

        private ScoreCalcRule()
        {
            _LoadingEvent = new ManualResetEvent(true);
            _LoadingStudentReferenceEvent = new ManualResetEvent(true);
            _LoadingClassReferenceEvent = new ManualResetEvent(true);

            _BGWScoreCalcRuleLoader = new BackgroundWorker();
            _BGWScoreCalcRuleLoader.DoWork += new DoWorkEventHandler(_BGWScoreCalcRuleLoader_DoWork);

            _BGWStudentReferenceLoader = new BackgroundWorker();
            _BGWStudentReferenceLoader.DoWork += new DoWorkEventHandler(_BGWStudentReferenceLoader_DoWork);

            _BGWClassReferenceLoader = new BackgroundWorker();
            _BGWClassReferenceLoader.DoWork += new DoWorkEventHandler(_BGWClassReferenceLoader_DoWork);


            LoadScoreCalcRule();
            LoadClassReference();
            LoadStudentReference();
        }

        void _BGWClassReferenceLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            _LoadClassReferenceAgain = true;
            while (_LoadClassReferenceAgain)
            {
                try
                {
                    _LoadClassReferenceAgain = false;
                    _ClassReference = new Dictionary<string, string>();
                    _ClassReference = QueryScoreCalcRule.GetClassReference();
                }
                catch (Exception exc)
                {
                    SmartSchool.ExceptionHandler.BugReporter.ReportException(new Exception("取得學生採用成績計算規則資料時發生錯誤。", exc), false);
                }
            }
            _LoadingClassReferenceEvent.Set();
        }

        void _BGWStudentReferenceLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            _LoadStudentReferenceAgain = true;
            while (_LoadStudentReferenceAgain)
            {
                try
                {
                    _LoadStudentReferenceAgain = false;
                    _StudentReference = new Dictionary<string, string>();
                    _StudentReference = QueryScoreCalcRule.GetStudentReference();
                }
                catch (Exception exc)
                {
                    SmartSchool.ExceptionHandler.BugReporter.ReportException(new Exception("取得學生採用成績計算規則資料時發生錯誤。", exc), false);
                }
            }
            _LoadingStudentReferenceEvent.Set();
        }

        void _BGWScoreCalcRuleLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            _LoadScoreCalcRuleAgain = true;
            while (_LoadScoreCalcRuleAgain)
            {
                try
                {
                    _LoadScoreCalcRuleAgain = false;
                    _Items = new Dictionary<string, ScoreCalcRuleInfo>();
                    DSResponse resp = QueryScoreCalcRule.GetList();
                    foreach (XmlElement scr in resp.GetContent().GetElements("ScoreCalcRule"))
                    {
                        _Items.Add(scr.SelectSingleNode("@ID").InnerText, new ScoreCalcRuleInfo(scr));
                    }
                }
                catch(Exception exc)
                {
                    SmartSchool.ExceptionHandler.BugReporter.ReportException(new Exception("取得成績計算規則資料時發生錯誤。", exc), false);
                }
            }
            _LoadingEvent.Set();
        }

        public void LoadClassReference()
        {
            if (!_BGWClassReferenceLoader.IsBusy)
            {
                _LoadingClassReferenceEvent.Reset();
                _BGWClassReferenceLoader.RunWorkerAsync();
            }
            else
            {
                _LoadClassReferenceAgain = true;
            }
        }

        public void LoadStudentReference()
        {
            if (!_BGWStudentReferenceLoader.IsBusy)
            {
                _LoadingStudentReferenceEvent.Reset();
                _BGWStudentReferenceLoader.RunWorkerAsync();
            }
            else
            {
                _LoadStudentReferenceAgain = true;
            }
        }

        public void LoadScoreCalcRule()
        {
            if (!_BGWScoreCalcRuleLoader.IsBusy)
            {
                _LoadingEvent.Reset();
                _BGWScoreCalcRuleLoader.RunWorkerAsync();
            }
            else
            {
                _LoadScoreCalcRuleAgain = true;
            }
        }

        public void Reflash()
        {
            LoadScoreCalcRule();
        }

        public event EventHandler ScoreCalcRuleInserted;
        public void Invok_ScoreCalcRuleInserted()
        {
            Reflash();

            if (ScoreCalcRuleInserted != null)
                ScoreCalcRuleInserted.Invoke(this, new EventArgs());
        }

        public event EventHandler ScoreCalcRuleUpdated;
        public void Invok_ScoreCalcRuleUpdated()
        {
            Reflash();

            if (ScoreCalcRuleUpdated != null)
                ScoreCalcRuleUpdated.Invoke(this, new EventArgs());
        }

        public event EventHandler<DeleteScoreCalcRuleEventArgs> ScoreCalcRuleDeleted;
        public void Invok_ScoreCalcRuleDeleted(string id)
        {
            Reflash();

            DeleteScoreCalcRuleEventArgs e = new DeleteScoreCalcRuleEventArgs();
            e.DeleteID = id;

            if (ScoreCalcRuleDeleted != null)
                ScoreCalcRuleDeleted.Invoke(this, e);
        }
    }

    public class DeleteScoreCalcRuleEventArgs : EventArgs
    {
        private string _id;

        public string DeleteID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
