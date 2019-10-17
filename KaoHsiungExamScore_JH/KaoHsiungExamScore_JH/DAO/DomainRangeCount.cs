using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaoHsiungExamScore_JH.DAO
{
    public class DomainRangeCount
    {
        /// <summary>
        /// 組距類別
        /// </summary>
        public enum DomainRangeType
        {R100_u, R90_99, R80_89, R70_79, R60_69, R50_59, R40_49, R30_39, R20_29, R10_19, R0_9}

        List<int> _RangeCount;
        public DomainRangeCount()
        {
            _RangeCount = new List<int>();
            for (int i = 0; i <= 10; i++)
            {
                _RangeCount.Add(0);
            }
        }

        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 放入成績
        /// </summary>
        /// <param name="score"></param>
        public void AddScore(decimal? score)
        {
            if (score.HasValue)
            {
                if (score.Value >= 100)
                {
                    _RangeCount[0]++;
                }
                else if (score.Value >= 90 && score.Value < 100)
                {
                    _RangeCount[1]++;
                }
                else if (score.Value >= 80 && score.Value < 90)
                {
                    _RangeCount[2]++;
                }
                else if (score.Value >= 70 && score.Value < 80)
                {
                    _RangeCount[3]++;
                }
                else if (score.Value >= 60 && score.Value < 70)
                {
                    _RangeCount[4]++;
                }
                else if (score.Value >= 50 && score.Value < 60)
                {
                    _RangeCount[5]++;
                }
                else if (score.Value >= 40 && score.Value < 50)
                {
                    _RangeCount[6]++;
                }
                else if (score.Value >= 30 && score.Value < 40)
                {
                    _RangeCount[7]++;
                }
                else if (score.Value >= 20 && score.Value < 30)
                {
                    _RangeCount[8]++;
                }
                else if (score.Value >= 10 && score.Value < 20)
                {
                    _RangeCount[9]++;
                }
                else if (score.Value >= 0 && score.Value < 10)
                {
                    _RangeCount[10]++;
                }
                else
                { }
            }
        }


        /// <summary>
        /// 取得統計數
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetRankCount(DomainRangeType type)
        {
            int retVal = 0;
            switch (type)
            {
                case DomainRangeType.R100_u: retVal = _RangeCount[0]; break;
                case DomainRangeType.R90_99: retVal = _RangeCount[1]; break;
                case DomainRangeType.R80_89: retVal = _RangeCount[2]; break;
                case DomainRangeType.R70_79: retVal = _RangeCount[3]; break;
                case DomainRangeType.R60_69: retVal = _RangeCount[4]; break;
                case DomainRangeType.R50_59: retVal = _RangeCount[5]; break;
                case DomainRangeType.R40_49: retVal = _RangeCount[6]; break;
                case DomainRangeType.R30_39: retVal = _RangeCount[7]; break;
                case DomainRangeType.R20_29: retVal = _RangeCount[8]; break;
                case DomainRangeType.R10_19: retVal = _RangeCount[9]; break;
                case DomainRangeType.R0_9: retVal = _RangeCount[10]; break;
            }
            return retVal;
        }
    }
}
