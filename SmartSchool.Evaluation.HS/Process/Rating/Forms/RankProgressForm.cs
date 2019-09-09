using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;
using System.Threading;

namespace SmartSchool.Evaluation.Process.Rating
{
    partial class RankProgressForm : BaseForm
    {
        private BackgroundWorker _main_worker;
        private RatingParameters _parameters;
        private StudentScoreRank _ranker;
        private bool _view;

        public RankProgressForm(RatingParameters parameters, bool view)
        {
            InitializeComponent();
            InitializeBackgroundWorker();

            _parameters = parameters;
            _view = view;
        }

        private void InitializeBackgroundWorker()
        {
            _main_worker = new BackgroundWorker();
            _main_worker.DoWork += new DoWorkEventHandler(MainWorker_DoWork);
            _main_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MainWorker_RunWorkerCompleted);
        }

        protected RatingParameters RatingParams
        {
            get { return _parameters; }
        }

        protected ProgressBarX ProgressUI
        {
            get { return pgUI; }
        }

        protected LabelX MessageLabel
        {
            get { return lblMessage; }
        }

        protected bool IsViewMode
        {
            get { return _view; }
        }

        private void MainWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _ranker = new StudentScoreRank(this, e);
            _ranker.Execute();
        }

        private void MainWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pgUI.Value = pgUI.Maximum;

            if (_view)
            {
                ResultViewForm resultview = new ResultViewForm(_ranker);
                resultview.ShowDialog();
            }

            if (e.Cancelled)
                DialogResult = DialogResult.Cancel;
            else
                DialogResult = DialogResult.OK;
        }

        private void RankProgressForm_Load(object sender, EventArgs e)
        {
            pgUI.Maximum = 100;
            pgUI.Minimum = 0;
            pgUI.Value = 0;

            _main_worker.RunWorkerAsync();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (IsRating()) _ranker.CancelRank();
        }

        private bool IsRating()
        {
            return _main_worker.IsBusy && _ranker != null;
        }

        internal class StudentScoreRank
        {
            private JobWeightTable _weights;
            private IProgressUI _progress;
            private StudentCollection _students;
            private AdapterCollection _adapters;
            private DoWorkEventArgs _work_args;
            private RatingParameters _parameters;
            private Dictionary<ScopeType, RatingScopeCollection> _scopesSet;
            private RatingScopeCollection _allScope;
            private bool _save_required;

            public StudentScoreRank(RankProgressForm rankForm, DoWorkEventArgs workArgs)
            {
                //排名相關參數，由主畫面選項決定。
                _parameters = rankForm.RatingParams;
                //BackgroundWorker 事件物件。
                _work_args = workArgs;
                //工作進度權重管理物件。
                _weights = new JobWeightTable();
                //畫面進度回報物件。
                _progress = new MainProgress(rankForm.ProgressUI, rankForm.MessageLabel);
                //要排名的學生集合。
                _students = new StudentCollection();
                //填入資料的 Adapter 集合。
                _adapters = new AdapterCollection();
                //是否不儲存，只試排看結果。
                _save_required = !rankForm.IsViewMode;
            }

            /// <summary>
            /// 開始進行排名。
            /// </summary>
            public void Execute()
            {
                _adapters.AddAdapter("Student", new StudentAdapter(_students, RatingParams));

                //依參數(RatingParams)建立相對應的 Adapter。
                CreateRequiredAdapter();

                //設定每一個 Adapter 的進度回報介面。
                _adapters.ForEach(new Action<DataAdapter>(SetMainProgress));

                //呼叫每一個 Adapter 的 Fill 動作。
                _adapters.ForEach(new Action<DataAdapter>(FillData));

                int t1 = Environment.TickCount;

                //加一種排名範圍就加一種 Scope。
                CreateScopes();

                //加一種成績就加一種 Target。
                CrateTargets();

                Scopes.ForEach(RankAll);

                //呼叫每一個 Adapter 的 Update 動作。
                _adapters.ForEach(new Action<DataAdapter>(UpdateRatingData));

                _work_args.Cancel = _progress.Cancellation;

                if (_progress.Cancellation)
                    _progress.ReportMessage("排名動作已取消。");
                else
                    _progress.ReportMessage("排名完成。");
            }

            private RatingScopeCollection CreateScopes()
            {
                _allScope = new RatingScopeCollection();
                _scopesSet = new Dictionary<ScopeType, RatingScopeCollection>();

                //將各類 Scope 存於集合中。
                _scopesSet.Add(ScopeType.Class, ScopeFactory.CreateScopes(_students, ScopeType.Class));
                _scopesSet.Add(ScopeType.Dept, ScopeFactory.CreateScopes(_students, ScopeType.Dept));
                _scopesSet.Add(ScopeType.GradeYear, ScopeFactory.CreateScopes(_students, ScopeType.GradeYear));

                //將所有 Scope 統一於一集合中。
                _allScope.AddRange(_scopesSet[ScopeType.Class]);
                _allScope.AddRange(_scopesSet[ScopeType.Dept]);
                _allScope.AddRange(_scopesSet[ScopeType.GradeYear]);

                return _allScope;
            }

            private void CrateTargets()
            {
                //學期科目成績
                if (RequireRating(RatingItems.SemsSubject))
                {
                    foreach (RatingScope eachScope in Scopes)
                    {
                        List<IRatingTarget> targets = SemsSubjectTarget.GroupBy(eachScope);
                        eachScope.RatingTargets.AddRange(targets);
                    }
                }

                //學年科目成績
                if (RequireRating(RatingItems.YearSubject))
                {
                    foreach (RatingScope eachScope in Scopes)
                    {
                        List<IRatingTarget> targets = YearSubjectTarget.GroupBy(eachScope);
                        eachScope.RatingTargets.AddRange(targets);
                    }
                }

                //學期學業成績
                if (RequireRating(RatingItems.SemsScore))
                {
                    SemsScoreTarget target = new SemsScoreTarget();
                    foreach (RatingScope eachScope in Scopes)
                        eachScope.RatingTargets.Add(target);
                }

                //學年學業成績
                if (RequireRating(RatingItems.YearScore))
                {
                    YearScoreTarget target = new YearScoreTarget();
                    foreach (RatingScope eachScope in Scopes)
                        eachScope.RatingTargets.Add(target);
                }

                //學期德行成績
                if (RequireRating(RatingItems.SemsMoral))
                {
                    SemsMoralTarget target = new SemsMoralTarget();

                    foreach (RatingScope eachScope in _scopesSet[ScopeType.Class])
                        eachScope.RatingTargets.Add(target);

                    foreach (RatingScope eachScope in _scopesSet[ScopeType.GradeYear])
                        eachScope.RatingTargets.Add(target);
                }

                //學年德行成績
                if (RequireRating(RatingItems.YearMoral))
                {
                    YearMoralTarget target = new YearMoralTarget();

                    foreach (RatingScope eachScope in _scopesSet[ScopeType.Class])
                        eachScope.RatingTargets.Add(target);

                    foreach (RatingScope eachScope in _scopesSet[ScopeType.GradeYear])
                        eachScope.RatingTargets.Add(target);
                }
            }

            private void CreateRequiredAdapter()
            {
                //學期科目成績
                if (RequireRating(RatingItems.SemsSubject))
                    _adapters.AddAdapter("SemsSubject", new SemsSubjectScoreAdapter(_students, RatingParams));

                //學年科目成績
                if (RequireRating(RatingItems.YearSubject))
                    _adapters.AddAdapter("YearSubject", new YearSubjectScoreAdapter(_students, RatingParams));

                //學期學業成績
                if (RequireRating(RatingItems.SemsScore))
                    _adapters.AddAdapter("SemsScore", new SemsScoreAdapter(_students, RatingParams));

                //學年學業成績
                if (RequireRating(RatingItems.YearScore))
                    _adapters.AddAdapter("YearScore", new YearScoreAdapter(_students, RatingParams));

                //學期德行成績
                if (RequireRating(RatingItems.SemsMoral))
                    _adapters.AddAdapter("SemsMoral", new SemsMoralAdapter(_students, RatingParams));

                //學年德行成績
                if (RequireRating(RatingItems.YearMoral))
                    _adapters.AddAdapter("YearMoral", new YearMoralAdapter(_students, RatingParams));

            }

            private void SetMainProgress(DataAdapter adapter)
            {
                adapter.SetProgressUI(_progress, _weights);
            }

            private void FillData(DataAdapter adapter)
            {
                if (_progress.Cancellation) return;
                if (!adapter.Fill()) _progress.Cancel();
            }

            private void UpdateRatingData(DataAdapter adapter)
            {
                if (_save_required)
                {
                    if (_progress.Cancellation) return;
                    if (!adapter.Update()) _progress.Cancel();
                }
            }

            private void RankAll(RatingScope scope)
            {
                if (_progress.Cancellation) return;
                scope.Rank(RatingParams.RatingMethod);
            }

            private bool RequireRating(RatingItems rating)
            {
                return (RatingParams.RatingItems & rating) == rating;
            }

            public RatingParameters RatingParams
            {
                get { return _parameters; }
            }

            /// <summary>
            /// 由表單呼叫的方法。
            /// </summary>
            public void CancelRank()
            {
                _progress.Cancel();
            }

            public StudentCollection Students
            {
                get { return _students; }
            }

            public RatingScopeCollection Scopes
            {
                get { return _allScope; }
            }
        }
    }
}