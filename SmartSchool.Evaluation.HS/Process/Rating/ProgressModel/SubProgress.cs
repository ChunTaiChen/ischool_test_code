using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表整個進度的一個部份。
    /// </summary>
    class SubProgress : IProgressUI
    {
        private IProgressUI _main_progress;
        private float _percentage;
        private int _previous_progress;

        public SubProgress(IProgressUI mainProgress, int percentage)
        {
            _main_progress = mainProgress;
            _percentage = percentage;
            _previous_progress = 0;
        }

        #region IProgressUI 成員

        public void ReportProgress(int progress)
        {
            int current = (int)(Convert.ToSingle(progress) * (_percentage / 100f));
            _main_progress.ReportProgress(current - _previous_progress);
            _previous_progress = current;
        }

        public void ReportMessage(string message)
        {
            _main_progress.ReportMessage(message);
        }

        public void ReportException(Exception ex)
        {
            _main_progress.ReportException(ex);
        }

        public void LogMessage(string message)
        {
            _main_progress.LogMessage(message);
        }

        public void Cancel()
        {
        }

        public bool Cancellation
        {
            get { return false; }
        }

        #endregion
    }
}
