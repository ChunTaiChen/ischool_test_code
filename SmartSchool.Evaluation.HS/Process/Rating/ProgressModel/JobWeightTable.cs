using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表一組工作項目的時間花費比重。
    /// </summary>
    class JobWeightTable
    {
        //代表各種工作的進度比重。
        private Dictionary<string, float> _progress_weight;

        public JobWeightTable()
        {
            _progress_weight = new Dictionary<string, float>();
        }

        /// <summary>
        /// 取得工作的比重，使用百分數。
        /// </summary>
        /// <param name="jobName">工作名稱。</param>
        /// <returns>回傳工作比重的百分數，如果該工作不存在則會回傳 0。</returns>
        public int GetJobWeight(string jobName)
        {
            if (_progress_weight.ContainsKey(jobName))
                return (int)((_progress_weight[jobName] / (float)TotalWeight) * 100);
            else
                return 0;
        }

        public void RegisterJob(string name, float weight)
        {
            if (_progress_weight.ContainsKey(name))
                throw new ArgumentException("工作(job)名稱不可重覆。");

            _progress_weight.Add(name, weight);
        }

        public float TotalWeight
        {
            get
            {
                float total = 0;
                foreach (float each in _progress_weight.Values)
                    total += each;
                
                return total;
            }
        }
    }
}
