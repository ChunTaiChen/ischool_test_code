using System;
using System.Collections.Generic;
using System.Text;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;
using System.Windows.Forms;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表進度回報的介面，支援多執行緒。
    /// </summary>
    class MainProgress : IProgressUI
    {
        private ProgressBarX _progress;
        private LabelX _message;
        private bool _canceled;

        public MainProgress(ProgressBarX progress, LabelX message)
        {
            _progress = progress;
            _message = message;
            _canceled = false;
        }

        private bool InvokeRequired
        {
            get { return _progress.InvokeRequired; }
        }

        #region IProgressUI 成員

        private delegate void InvokeInt(int progress);
        private delegate void InvokeString(string message);
        private delegate void InvokeException(Exception ex);

        /// <summary>
        /// 回報進度。
        /// </summary>
        /// <param name="progress">與上次進度的差異量。</param>
        public void ReportProgress(int progress)
        {
            if (InvokeRequired)
                _progress.Invoke(new InvokeInt(ReportProgress), progress);
            else
                _progress.Value += ((_progress.Value + progress) > 100 ? 100 : progress);
        }

        public void ReportMessage(string message)
        {
            if (InvokeRequired)
                _progress.Invoke(new InvokeString(ReportMessage), message);
            else
                _message.Text = message;
        }

        public void ReportException(Exception ex)
        {
            if (InvokeRequired)
                _progress.Invoke(new InvokeException(ReportException), ex);
            else
                MsgBox.Show(ex.Message);
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Cancel()
        {
            _canceled = true;
        }

        public bool Cancellation
        {
            get { return _canceled; }
        }

        #endregion
    }
}
