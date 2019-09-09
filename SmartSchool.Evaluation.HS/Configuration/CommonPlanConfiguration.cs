using System;
using System.ComponentModel;
using System.Xml;
using SmartSchool.Common;
using SmartSchool.Evaluation.GraduationPlan.Editor;
using SmartSchool.Feature.GraduationPlan;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class CommonPlanConfiguration : SmartSchool.Customization.PlugIn.Configure.ConfigurationItem
    {
        private IGraduationPlanEditor _CommonPlanEditor;

        private BackgroundWorker _BKWCommonPlanLoader;

        public CommonPlanConfiguration()
        {
            InitializeComponent();
            
            _CommonPlanEditor = commonPlanEditor1;
            _CommonPlanEditor.IsDirtyChanged += new EventHandler(_CommonPlanEditor_IsDirtyChanged);
            _BKWCommonPlanLoader = new BackgroundWorker();
            _BKWCommonPlanLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BKWCommonPlanLoader_RunWorkerCompleted);
            _BKWCommonPlanLoader.DoWork += new DoWorkEventHandler(_BKWCommonPlanLoader_DoWork);
        }

        protected override void OnActive()
        {
            LoadGraduationPlan();
        }

        void _CommonPlanEditor_IsDirtyChanged(object sender, EventArgs e)
        {
            buttonX4.Enabled = _CommonPlanEditor.IsDirty;
        }

        public void LoadGraduationPlan()
        {
            if (!_BKWCommonPlanLoader.IsBusy)
            {
                _BKWCommonPlanLoader.RunWorkerAsync();
            }
        }

        void _BKWCommonPlanLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.CommonPlan;
        }

        void _BKWCommonPlanLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _CommonPlanEditor.SetSource((XmlElement)e.Result);
        }
        private void buttonX4_Click(object sender, EventArgs e)
        {

            if (!_CommonPlanEditor.IsValidated)
            {
                MsgBox.Show("課程資料表內容輸入錯誤，請檢查輸入資料。");
                return;
            }

            EditGraduationPlan.SetCommon(_CommonPlanEditor.GetSource());
            EventHub.Instance.InvokCommonPlanUpdated();
            LoadGraduationPlan();
        }    
    }
}
