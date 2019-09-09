using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    interface IProgressUI
    {
        void ReportProgress(int progress);

        void ReportMessage(string message);

        void ReportException(Exception ex);

        void LogMessage(string message);

        void Cancel();

        bool Cancellation { get;}
    }
}