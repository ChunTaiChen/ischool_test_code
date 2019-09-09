using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using System.Collections;

namespace SmartSchool.Evaluation.Process.Rating
{
    partial class ResultViewForm : BaseForm
    {
        private RankProgressForm.StudentScoreRank _rankResult;

        public ResultViewForm(RankProgressForm.StudentScoreRank rankResult)
        {
            InitializeComponent();
            _rankResult = rankResult;
        }

        private void ResultViewForm_Load(object sender, EventArgs e)
        {
            foreach (string each in Enum.GetNames(typeof(ScopeType)))
                cboScopeType.Items.Add(each);
        }

        private void cboScopeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScopeType listType = ScopeType.Class;

            if (cboScopeType.Text == Enum.GetName(typeof(ScopeType), ScopeType.Class))
                listType = ScopeType.Class;
            else if (cboScopeType.Text == Enum.GetName(typeof(ScopeType), ScopeType.Dept))
                listType = ScopeType.Dept;
            else if (cboScopeType.Text == Enum.GetName(typeof(ScopeType), ScopeType.GradeYear))
                listType = ScopeType.GradeYear;

            lvScopes.Items.Clear();
            foreach (RatingScope eachScope in _rankResult.Scopes)
            {
                if (eachScope.ScopeType == listType)
                {
                    ListViewItem item = new ListViewItem(eachScope.Name + "(" + eachScope.Count + ")");
                    item.Tag = eachScope;
                    lvScopes.Items.Add(item);
                }
            }
        }

        private void lvScopes_Click(object sender, EventArgs e)
        {
            if (lvScopes.FocusedItem == null) return;
            HighlighSelected(sender as ListView);

            RatingScope scope = lvScopes.FocusedItem.Tag as RatingScope;

            lvTargets.Items.Clear();
            foreach (IRatingTarget eachTarget in scope.RatingTargets)
            {
                ListViewItem item = new ListViewItem(string.Format("{0}", eachTarget.Name));
                item.Tag = eachTarget;
                lvTargets.Items.Add(item);
            }
        }

        private void lvTargets_Click(object sender, EventArgs e)
        {
            if (lvScopes.FocusedItem == null) return;
            if (lvTargets.FocusedItem == null) return;
            HighlighSelected(sender as ListView);

            IRatingTarget target = lvTargets.FocusedItem.Tag as IRatingTarget;
            RatingScope scope = lvScopes.FocusedItem.Tag as RatingScope;

            lvPlace.SuspendLayout();
            lvPlace.Items.Clear();
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (Student eachStu in scope)
            {
                ListViewItem item = new ListViewItem();
                item.Text = eachStu.ClassName;
                item.SubItems.Add(eachStu.SeatNumber);
                item.SubItems.Add(eachStu.Name);

                decimal score = target.GetScore(eachStu);
                if (score == decimal.MinValue)
                    item.SubItems.Add("無");
                else
                    item.SubItems.Add(score.ToString());

                ResultPlace place = target.GetPlace(eachStu, scope.ScopeType);
                if (place == null)
                    item.SubItems.Add("無");
                else
                    item.SubItems.Add(place.Place.ToString());

                item.Tag = eachStu;

                items.Add(item);
            }
            lvPlace.Items.AddRange(items.ToArray());
            lvPlace.ResumeLayout();
        }

        private void lvPlace_Click(object sender, EventArgs e)
        {
            if (lvPlace.FocusedItem == null) return;
            HighlighSelected(sender as ListView);

            Student student = lvPlace.FocusedItem.Tag as Student;

            string info = "GradeYear:{0} Dept:{1} Class:{2} Name:{3}";
            lblStudent.Text = string.Format(info, student.GradeYear, student.DeptName, student.ClassName, student.Name);

            lvStudentPlace.Items.Clear();

            foreach (SemsSubjectScore eachScore in student.SemsSubjects.Values)
                ShowScoreItem(eachScore);

            foreach (YearSubjectScore eachScore in student.YearSubjects.Values)
                ShowScoreItem(eachScore);

            ShowScoreItem(student.SemsScore);

            ShowScoreItem(student.YearScore);

            ShowScoreItem(student.SemsMoral);

            ShowScoreItem(student.YearMoral);

        }

        private void ShowScoreItem(RatableScore eachScore)
        {
            if (eachScore == null) return;

            foreach (ResultPlace eachPlace in eachScore.RatingResults.Values)
            {
                ListViewItem item = new ListViewItem(eachPlace.Scope.Name + "(" + eachPlace.RatingBase + ")");
                item.SubItems.Add(eachPlace.RatingTarget.Name + "(" + eachPlace.Scope.GetContainTargetList(eachPlace.RatingTarget).Count + ")");

                if (eachScore.Score == decimal.MinValue)
                    item.SubItems.Add("無");
                else
                    item.SubItems.Add(eachScore.Score.ToString());

                item.SubItems.Add(eachPlace.Place.ToString());
                lvStudentPlace.Items.Add(item);
            }
        }

        private void HighlighSelected(ListView listView)
        {
            if (listView.FocusedItem == null) return;

            foreach (ListViewItem eachItem in listView.Items)
            {
                eachItem.ForeColor = Color.Black;
                eachItem.BackColor = Color.White;
            }

            listView.FocusedItem.BackColor = Color.Blue;
            listView.FocusedItem.ForeColor = Color.White;
        }

        private void lvPlace_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            lvPlace.ListViewItemSorter = new PlaceSorter(e.Column);
        }

        private void lvStudentPlace_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            lvStudentPlace.ListViewItemSorter = new PlaceSorter(e.Column);
        }

        class PlaceSorter : IComparer
        {
            private int _column;

            public PlaceSorter(int column)
            {
                _column = column;
            }

            #region IComparer 成員

            public int Compare(object x, object y)
            {
                bool stringCompare = false;
                ListViewItem xItem = x as ListViewItem;
                ListViewItem yItem = y as ListViewItem;
                decimal xValue, yValue;

                stringCompare = (!decimal.TryParse(xItem.SubItems[_column].Text, out xValue));
                stringCompare |= (!decimal.TryParse(yItem.SubItems[_column].Text, out yValue));

                if (stringCompare)
                    return xItem.SubItems[_column].Text.CompareTo(yItem.SubItems[_column].Text);
                else
                    return xValue.CompareTo(yValue);
            }

            #endregion

        }
    }
}