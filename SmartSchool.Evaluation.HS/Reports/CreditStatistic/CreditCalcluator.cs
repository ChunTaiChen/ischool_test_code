using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;

namespace SmartSchool.Evaluation.Reports.CreditStatistic
{
     internal class CreditCalcluator
     {
          public CreditCalcluator(StudentRecord student)
          {
               foreach (SemesterSubjectScoreInfo eachInfo in student.SemesterSubjectScoreList)
               {
                    //不計學分當沒看到
                    if (eachInfo.Detail.GetAttribute("不計學分") == "是")
                         continue;
                    _total_credit += eachInfo.CreditDec();

                    if (eachInfo.Require)
                         _total_require_credit += eachInfo.CreditDec();

                    if (eachInfo.Pass)
                    {
                         if (eachInfo.Require)
                              _passed_required_credit += eachInfo.CreditDec();
                         else
                              _passed_select_credit += eachInfo.CreditDec();
                    }
               }
          }

          private decimal _total_credit;
          public decimal TotalCredit
          {
               get { return _total_credit; }
          }

          private decimal _total_require_credit;
          public decimal TotalRequireCredit
          {
               get { return _total_require_credit; }
          }

          private decimal _passed_required_credit;
          public decimal PassedRequiredCredit
          {
               get { return _passed_required_credit; }
          }

          private decimal _passed_select_credit;
          public decimal PassedSelectCredit
          {
               get { return _passed_select_credit; }
          }

          public decimal TotalPassedCredit
          {
               get { return PassedRequiredCredit + PassedSelectCredit; }
          }

          public decimal RequiredRestCredit
          {
               get { return TotalRequireCredit - PassedRequiredCredit; }
          }

     }
}
