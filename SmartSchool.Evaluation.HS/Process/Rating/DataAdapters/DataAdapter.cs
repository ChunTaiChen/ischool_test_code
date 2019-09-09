using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    class DataAdapter
    {
        private StudentCollection _students;
        private RatingParameters _parameters;
        private IProgressUI _progress_ui;
        private JobWeightTable _weight_table;

        public DataAdapter(StudentCollection students, RatingParameters parameters)
        {
            _students = students;
            _parameters = parameters;
        }

        protected StudentCollection Students
        {
            get { return _students; }
        }

        protected RatingParameters Parameters
        {
            get { return _parameters; }
        }

        protected IProgressUI MainProgress
        {
            get { return _progress_ui; }
        }

        protected JobWeightTable WeightTable
        {
            get { return _weight_table; }
        }

        public void SetProgressUI(IProgressUI progress, JobWeightTable weightTable)
        {
            _progress_ui = progress;
            _weight_table = weightTable;
            RegisterJobs();
        }

        protected virtual void RegisterJobs()
        {
        }

        public virtual bool Fill()
        {
            throw new NotImplementedException("此方法未實作。");
        }

        public virtual bool Update()
        {
            throw new NotImplementedException("此方法未實作。");
        }
    }

    class AdapterCollection : Dictionary<string, DataAdapter>
    {
        private List<DataAdapter> _sequence;

        public AdapterCollection()
        {
            _sequence = new List<DataAdapter>();
        }

        public void AddAdapter(string name, DataAdapter adapter)
        {
            if (ContainsKey(name))
                throw new ArgumentException("資料轉換器(DataAdapter)名稱重覆。");

            _sequence.Add(adapter);
            Add(name, adapter);
        }

        public void ForEach(Action<DataAdapter> action)
        {
            _sequence.ForEach(action);
        }
    }
}
