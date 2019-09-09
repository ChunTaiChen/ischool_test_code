using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Common;
using SmartSchool.Evaluation.ScoreCalcRule;
using SmartSchool.Feature.ScoreCalcRule;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class ScoreCalcConfiguration : SmartSchool.Customization.PlugIn.Configure.ConfigurationItem
    {

        private BackgroundWorker _BGWScoreCalcRuleLoader;
        private ButtonItem _SelectItem;
        private bool _DataLoading;

        public ScoreCalcConfiguration()
        {
            this.InitializeComponent();

            _BGWScoreCalcRuleLoader = new BackgroundWorker();
            _BGWScoreCalcRuleLoader.DoWork += new DoWorkEventHandler(_BGWScoreCalcRuleLoader_DoWork);
            _BGWScoreCalcRuleLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWScoreCalcRuleLoader_RunWorkerCompleted);

            ScoreCalcRule.ScoreCalcRule.Instance.ScoreCalcRuleInserted += new EventHandler(Instance_ScoreCalcRuleInserted);
            ScoreCalcRule.ScoreCalcRule.Instance.ScoreCalcRuleUpdated += new EventHandler(Instance_ScoreCalcRuleUpdated);
            ScoreCalcRule.ScoreCalcRule.Instance.ScoreCalcRuleDeleted += new EventHandler<DeleteScoreCalcRuleEventArgs>(Instance_ScoreCalcRuleDeleted);
        }

        private void itemPanel1_SizeChanged(object sender, EventArgs e)
        {
            _WaitingPicture.Location = new Point((itemPanel1.Width - _WaitingPicture.Width) / 2, (itemPanel1.Height - _WaitingPicture.Height) / 2);
        }

        void Instance_ScoreCalcRuleDeleted(object sender, DeleteScoreCalcRuleEventArgs e)
        {
            LoadScoreCalcRule(false);
        }

        void Instance_ScoreCalcRuleUpdated(object sender, EventArgs e)
        {
            LoadScoreCalcRule(false);
        }

        void Instance_ScoreCalcRuleInserted(object sender, EventArgs e)
        {
            LoadScoreCalcRule(false);
        }

        void _BGWScoreCalcRuleLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ScoreCalcRuleInfoCollection resp = (ScoreCalcRuleInfoCollection)e.Result;
            foreach (ScoreCalcRuleInfo scr in resp)
            {
                ButtonItem item = new ButtonItem(scr.ID, scr.Name);
                item.Tag = scr;
                itemPanel1.Items.Add(item);
                item.Click += new EventHandler(item_Click);
            }
            btn_update.Enabled = false;
            btn_delete.Enabled = false;
            scoreCalcRuleEditor1.Visible = false;
            itemPanel1.Refresh();
            resetDataLoading();
        }
        void item_Click(object sender, EventArgs e)
        {
            if (_SelectItem != null)
                _SelectItem.Checked = false;
            btn_update.Enabled = false;
            btn_delete.Enabled = false;
            scoreCalcRuleEditor1.Visible = (sender != null);         
            if (sender != null)
            {
                ButtonItem item = (ButtonItem)sender;
                ScoreCalcRuleInfo info = (ScoreCalcRuleInfo)item.Tag;
                _SelectItem = item;
                item.Checked = true;

                this.scoreCalcRuleEditor1.SetSource(info.ScoreCalcRuleElement);
                this.scoreCalcRuleEditor1.ScoreCalcRuleName = info.Name;
                btn_update.Enabled = true;
                btn_delete.Enabled = true;
                scoreCalcRuleEditor1.Visible = true;
            }
        }
        void _BGWScoreCalcRuleLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            if ( (bool)e.Argument ) ScoreCalcRule.ScoreCalcRule.Instance.Reflash();
            e.Result = ScoreCalcRule.ScoreCalcRule.Instance.Items;
        }

        public void LoadScoreCalcRule(bool reflash)
        {
            if (!_BGWScoreCalcRuleLoader.IsBusy)
            {
                _SelectItem = null;
                itemPanel1.Items.Clear();
                btn_update.Enabled = false;
                btn_delete.Enabled = false;
                setDataLoading();
                _BGWScoreCalcRuleLoader.RunWorkerAsync(reflash);
            }
        }

        protected override void OnActive()
        {
            LoadScoreCalcRule(true);
        }

        private void setDataLoading()
        {
            _DataLoading = true;
            if (_WaitingPicture != null)
                _WaitingPicture.Visible = (_DataLoading);
        }

        private void resetDataLoading()
        {
            _DataLoading = false;
            if (_WaitingPicture != null)
                _WaitingPicture.Visible = (_DataLoading);
        }

        private void btn_create_Click(object sender, EventArgs e)
        {
            new ScoreCalcRuleCreator().ShowDialog();
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            if (this.scoreCalcRuleEditor1.IsValidated)
            {
                EditScoreCalcRule.Update((_SelectItem.Tag as ScoreCalcRuleInfo).ID, (_SelectItem.Tag as ScoreCalcRuleInfo).Name, this.scoreCalcRuleEditor1.GetSource());
                ScoreCalcRule.ScoreCalcRule.Instance.Invok_ScoreCalcRuleUpdated();
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (_SelectItem != null)
            {
                if (MsgBox.Show("確定要刪除 '" + _SelectItem.Text + "' 成績計算規則？", "確定", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    string id = (_SelectItem.Tag as ScoreCalcRuleInfo).ID;
                    RemoveScoreCalcRule.Delete(id);
                    ScoreCalcRule.ScoreCalcRule.Instance.Invok_ScoreCalcRuleDeleted(id);
                }
            }
        }
    }
}
