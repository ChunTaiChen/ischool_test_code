using System;
using System.Collections.Generic;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.Feature.GraduationPlan;

namespace SmartSchool.Evaluation
{

    class EventHub
    {
        private static EventHub _Instance = null;
        public static EventHub Instance 
        {
            get 
            {
                if ( _Instance == null ) _Instance = new EventHub();
                return _Instance;
            }
        }

        public event EventHandler<ScoreChangedEventArgs> ScoreChanged;
        public void InvokScoreChanged(params string[] studentIDList)
        {
            if ( studentIDList.Length == 0 ) return;
            if ( ScoreChanged != null )
            {
                ScoreChangedEventArgs args = new ScoreChangedEventArgs(studentIDList);
                ScoreChanged.Invoke(this, args);
            }
        }


        public event EventHandler CommonPlanUpdated;
        public void InvokCommonPlanUpdated()
        {
            GraduationPlan.GraduationPlan.Instance.LoadCommonPlan();
            if ( CommonPlanUpdated != null )
                CommonPlanUpdated.Invoke(this, new EventArgs());
        }

        public event EventHandler GraduationPlanInserted;
        public void InvokGraduationPlanInserted()
        {
            GraduationPlan.GraduationPlan.Instance.LoadGraduationPlan();
            if ( GraduationPlanInserted != null )
                GraduationPlanInserted.Invoke(this, new EventArgs());
        }

        public event EventHandler<UpdateGraduationPlanEventArgs> GraduationPlanUpdated;
        public void InvokGraduationPlanUpdated(string id)
        {
            GraduationPlanInfo oldInfo = null;
            GraduationPlanInfo newInfo = null;
            if ( GraduationPlan.GraduationPlan.Instance._Items.ContainsKey(id) )
            {
                oldInfo = GraduationPlan.GraduationPlan.Instance._Items[id];
                DSResponse resp = QueryGraduationPlan.GetGraduationPlan(id);
                List<XmlElement> SortList = new List<XmlElement>();
                XmlElement gPlan = resp.GetContent().GetElement("GraduationPlan");
                if ( gPlan != null )
                {
                    newInfo = new GraduationPlanInfo(gPlan);
                    GraduationPlan.GraduationPlan.Instance._Items[id] = newInfo;
                }
                else
                    GraduationPlan.GraduationPlan.Instance._Items.Remove(id);
            }
            if ( GraduationPlanUpdated != null )
                GraduationPlanUpdated.Invoke(this, new UpdateGraduationPlanEventArgs(oldInfo, newInfo));
        }

        public event EventHandler<DeleteGraduationPlanEventArgs> GraduationPlanDeleted;
        public void InvokGraduationPlanDeleted(string id)
        {
            if (GraduationPlan.GraduationPlan.Instance._Items.ContainsKey(id) )
            {
                GraduationPlan.GraduationPlan.Instance._Items.Remove(id);
            }
            if ( GraduationPlanDeleted != null )
                GraduationPlanDeleted.Invoke(this, new DeleteGraduationPlanEventArgs(id));
        }

        public event EventHandler StudentReferenceGranduationPlanChanged;
        public void InvokeStudentReferenceGranduationPlanChanged()
        {
            if ( StudentReferenceGranduationPlanChanged != null )
                StudentReferenceGranduationPlanChanged.Invoke(this, new EventArgs());
        }
        
        public event EventHandler ClassReferenceGranduationPlanChanged;
        public void InvokeClassReferenceGranduationPlanChanged()
        {
            if ( ClassReferenceGranduationPlanChanged != null )
                ClassReferenceGranduationPlanChanged.Invoke(this, new EventArgs());
        }

        public event EventHandler StudentReferenceCaleRuleChanged;
        public void InvokeStudentReferenceCaleRuleChanged()
        {
            if ( StudentReferenceCaleRuleChanged != null )
                StudentReferenceCaleRuleChanged.Invoke(this, new EventArgs());
        }

        public event EventHandler ClassReferenceCaleRuleChanged;
        public void InvokeClassReferenceCaleRuleChanged()
        {
            if ( ClassReferenceCaleRuleChanged != null )
                ClassReferenceCaleRuleChanged.Invoke(this, new EventArgs());
        }
    }
    class ScoreChangedEventArgs : EventArgs
    {
        List<string> _Items;
        public ScoreChangedEventArgs(params string[] items)
        {
            _Items = new List<string>();
            foreach ( string var in items )
            {
                _Items.Add(var);
            }
        }
        public List<string> StudentIds
        { get { return _Items; } }
    }

    public class UpdateGraduationPlanEventArgs : EventArgs
    {
        private GraduationPlanInfo _OldInfo, _NewInfo;
        public UpdateGraduationPlanEventArgs()
        {

        }
        public UpdateGraduationPlanEventArgs(GraduationPlanInfo oldInfo, GraduationPlanInfo newInfo)
        {
            _OldInfo = oldInfo;
            _NewInfo = newInfo;
        }
        public GraduationPlanInfo OldInfo
        {
            get { return _OldInfo; }
            set { _OldInfo = value; }
        }
        public GraduationPlanInfo NewInfo
        {
            get { return _NewInfo; }
            set { _NewInfo = value; }
        }
    }

    public class DeleteGraduationPlanEventArgs : EventArgs
    {
        private string _id;
        public DeleteGraduationPlanEventArgs(string id)
        {
            _id = id;
        }
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }

}
