using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DevComponents.DotNetBar.Controls;
using FISCA.DSAUtil;
using SmartSchool.ApplicationLog;
using SmartSchool.Common;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.Feature.Score;
using SmartSchool.StudentRelated;

namespace SmartSchool.Evaluation.Content.ScoreEditor
{
     public partial class SemesterScoreEditor : BaseForm
     {
          string _SubjectScoreID = "";
          string _EntryScoreID1 = "";
          string _EntryScoreID2 = "";
          string _StudentID;
          private Dictionary<Control, string> _entryScoreBase;
          private bool _scoreUpdating = false;
          private Dictionary<Control, ErrorProvider> _errorProviderDictionary = new Dictionary<Control, ErrorProvider>();

          //Log用來比較前後資料差異的變數
          private Dictionary<string, string> _beforeData = new Dictionary<string, string>();
          private Dictionary<string, string> _afterData = new Dictionary<string, string>();
          private DSXmlHelper _beforeXml = new DSXmlHelper("BeforeXml");
          private DSXmlHelper _afterXml = new DSXmlHelper("AfterXml");
          private EntityAction _entityAction = EntityAction.Insert;

          //排名相關
          private SubjectScoreToolTipProvider _subject_rating;
          private EntryScoreToolTipProvider _score_rating;
          private EntryScoreToolTipProvider _moral_rating;
          private const int SubjectColumn = 1, SubjectLevel = 2;

          public SemesterScoreEditor(string refStudentID)
          {
               InitializeComponent();
               _StudentID = refStudentID;
               _beforeXml.AddElement("SubjectGradeYear");
               _beforeXml.AddElement("SubjectCollection");
               _beforeXml.AddElement("EntryCollection");
               _beforeXml.AddElement("LockScore");
               _afterXml.AddElement("SubjectGradeYear");
               _afterXml.AddElement("SubjectCollection");
               _afterXml.AddElement("EntryCollection");
               _afterXml.AddElement("LockScore");
               labelX10.Text = Student.Instance.Items[_StudentID].ClassName + (Student.Instance.Items[_StudentID].ClassName != "" && Student.Instance.Items[_StudentID].SeatNo != "" ? " " + Student.Instance.Items[_StudentID].SeatNo : "") +
                   " " + Student.Instance.Items[_StudentID].Name + (Student.Instance.Items[_StudentID].StudentNumber == "" ? "" : " (" + Student.Instance.Items[_StudentID].StudentNumber + ")");
               for (int s = 3; s > 0; s--)
               {
                    comboBoxEx1.Items.Add(CurrentUser.Instance.SchoolYear - s);
               }
               comboBoxEx2.Items.AddRange(new object[] { "1", "2" });
               cboAttendGradeYear.Items.AddRange(new object[] { "1", "2", "3", "4" });
               ValidateAll();
          }

          public SemesterScoreEditor(string schoolYear, string semester, string refStudentID)
          {
               InitializeComponent();
               _StudentID = refStudentID;
               _beforeXml.AddElement("SubjectGradeYear");
               _beforeXml.AddElement("SubjectCollection");
               _beforeXml.AddElement("EntryCollection");
               _beforeXml.AddElement("LockScore");
               _afterXml.AddElement("SubjectGradeYear");
               _afterXml.AddElement("SubjectCollection");
               _afterXml.AddElement("EntryCollection");
               _afterXml.AddElement("LockScore");
               _entityAction = EntityAction.Update;
               labelX10.Text = Student.Instance.Items[_StudentID].ClassName + (Student.Instance.Items[_StudentID].ClassName != "" && Student.Instance.Items[_StudentID].SeatNo != "" ? " " + Student.Instance.Items[_StudentID].SeatNo : "") +
                   " " + Student.Instance.Items[_StudentID].Name + (Student.Instance.Items[_StudentID].StudentNumber == "" ? "" : " (" + Student.Instance.Items[_StudentID].StudentNumber + ")");
               this.comboBoxEx1.Text = schoolYear;
               this.comboBoxEx2.Text = semester;
               comboBoxEx1.Enabled = comboBoxEx2.Enabled = false;
               ReLoad(null, null);
               cboAttendGradeYear.Items.AddRange(new object[] { "1", "2", "3", "4" });
               ValidateAll();

               btnSave.Visible = CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               dataGridViewX1.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               cboAttendGradeYear.Enabled = CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               btnCalcScore.Enabled = CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX1.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX2.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX3.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX4.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX5.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX6.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX7.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX8.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX9.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX10.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX11.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX12.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
               textBoxX13.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
          }

          private void buttonX2_Click(object sender, EventArgs e)
          {
               this.Close();
          }

          private void ReLoad(object sender, EventArgs e)
          {
               _scoreUpdating = true;

               _SubjectScoreID = "";
               _EntryScoreID1 = "";
               _EntryScoreID2 = "";

               if (!ValidateSchoolYearSemester())
                    return;

               string gradeYear = "";
               #region 科目成績
               dataGridViewX1.Rows.Clear();
               DSXmlHelper _SubjectResponse = QueryScore.GetSemesterSubjectScoreBySemester(true, (comboBoxEx1.Text), (comboBoxEx2.Text), _StudentID);
               if (_SubjectResponse.GetElement("SemesterSubjectScore") != null)
               {
                    _SubjectScoreID = _SubjectResponse.GetElement("SemesterSubjectScore").GetAttribute("ID");
                    int t;
                    if (int.TryParse(_SubjectResponse.GetText("SemesterSubjectScore/GradeYear"), out t))
                         gradeYear = "" + t;

                    #region 建立排名相關物件
                    DSXmlHelper rating = _SubjectResponse;
                    _subject_rating = new SubjectScoreToolTipProvider(rating.GetElement("SemesterSubjectScore/ClassRating"),
                        rating.GetElement("SemesterSubjectScore/DeptRating"),
                        rating.GetElement("SemesterSubjectScore/YearRating"));
                    #endregion

                    foreach (XmlElement var in _SubjectResponse.GetElements("SemesterSubjectScore/ScoreInfo/SemesterSubjectScoreInfo/Subject"))
                    {
                         _beforeXml.AddElement("SubjectCollection", var);
                         DataGridViewRow row = new DataGridViewRow();
                         row.CreateCells(dataGridViewX1,
                             var.GetAttribute("開課分項類別"),
                             var.GetAttribute("科目"),
                             var.GetAttribute("科目級別"),
                             var.GetAttribute("開課學分數"),
                             var.GetAttribute("修課校部訂"),
                             var.GetAttribute("修課必選修"),
                             var.GetAttribute("是否取得學分") == "是",
                             var.GetAttribute("原始成績"),
                             var.GetAttribute("補考成績"),
                             var.GetAttribute("重修成績"),
                             var.GetAttribute("擇優採計成績"),
                             var.GetAttribute("學年調整成績"),
                             var.GetAttribute("不計學分") == "是",
                             var.GetAttribute("不需評分") == "是",
                             var.GetAttribute("註記")
                             );
                         row.Cells[SubjectColumn].ToolTipText = GetSubjectScorePlace(row);
                         dataGridViewX1.Rows.Add(row);
                    }
                    foreach (DataGridViewRow row in dataGridViewX1.Rows)
                    {
                         CompareSubjectInfo(row);
                    }
               }
               #endregion
               #region 分項成績
               textBoxX1.Text = "";
               textBoxX2.Text = "";
               textBoxX3.Text = "";
               textBoxX4.Text = "";
               textBoxX5.Text = "";
               textBoxX6.Text = "";
               textBoxX7.Text = "";
               textBoxX8.Text = "";
               textBoxX9.Text = "";
               textBoxX10.Text = "";
               textBoxX11.Text = "";
               textBoxX12.Text = "";
               textBoxX13.Text = "";
               DSXmlHelper _EntryResponse = QueryScore.GetSemesterEntryScoreBySemester(true, (comboBoxEx1.Text), (comboBoxEx2.Text), _StudentID);
               if (_EntryResponse.GetElement("SemesterEntryScore") != null)
               {
                    int t;
                    if (int.TryParse(_EntryResponse.GetText("SemesterEntryScore/GradeYear"), out t))
                         gradeYear = "" + t;
                    foreach (XmlElement var in _EntryResponse.GetElements("SemesterEntryScore"))
                    {
                         string id = var.GetAttribute("ID");
                         switch (var.SelectSingleNode("EntryGroup").InnerText)
                         {
                              case "學習":
                                   _EntryScoreID1 = id;
                                   _score_rating = new EntryScoreToolTipProvider("學業", var); //排名資訊
                                   break;
                              case "行為":
                                   _EntryScoreID2 = id;
                                   _moral_rating = new EntryScoreToolTipProvider("德行", var); //排名資訊
                                   break;
                              default:
                                   throw new Exception("拎唄謀洗鰓機雷EntryGroup： 　\"" + var.SelectSingleNode("EntryGroup").InnerText + "\"");
                         }
                         foreach (XmlNode score in var.SelectNodes("ScoreInfo/SemesterEntryScore/Entry"))
                         {
                              XmlElement element = (XmlElement)score;
                              _beforeXml.AddElement("EntryCollection", element);

                              #region 依分項填入格子
                              switch (element.GetAttribute("分項"))
                              {
                                   case "學業":
                                        textBoxX1.Text = element.GetAttribute("成績");

                                        _score_rating.SetTooltip(textBoxX1);
                                        _score_rating.SetTooltip(labelX3);
                                        break;
                                   case "體育":
                                        textBoxX3.Text = element.GetAttribute("成績");
                                        break;
                                   case "國防通識":
                                        textBoxX4.Text = element.GetAttribute("成績");
                                        break;
                                   case "健康與護理":
                                        textBoxX5.Text = element.GetAttribute("成績");
                                        break;
                                   case "實習科目":
                                        textBoxX6.Text = element.GetAttribute("成績");
                                        break;
                                   case "專業科目":
                                        textBoxX7.Text = element.GetAttribute("成績");
                                        break;
                                   case "學業(原始)":
                                        textBoxX13.Text = element.GetAttribute("成績");
                                        break;
                                   case "體育(原始)":
                                        textBoxX12.Text = element.GetAttribute("成績");
                                        break;
                                   case "國防通識(原始)":
                                        textBoxX10.Text = element.GetAttribute("成績");
                                        break;
                                   case "健康與護理(原始)":
                                        textBoxX11.Text = element.GetAttribute("成績");
                                        break;
                                   case "實習科目(原始)":
                                        textBoxX9.Text = element.GetAttribute("成績");
                                        break;
                                   case "專業科目(原始)":
                                        textBoxX8.Text = element.GetAttribute("成績");
                                        break;
                                   case "德行":
                                        textBoxX2.Text = element.GetAttribute("成績");

                                        _moral_rating.SetTooltip(textBoxX2);
                                        _moral_rating.SetTooltip(labelX4);

                                        bool lockScore = false;
                                        bool.TryParse(element.GetAttribute("鎖定"), out lockScore);
                                        if (lockScore)
                                        {
                                             buttonItem4.Checked = true;
                                        }
                                        else
                                        {
                                             buttonItem3.Checked = true;
                                        }
                                        _beforeXml.AddElement("LockScore", "Lock", element.GetAttribute("鎖定"));
                                        break;
                                   default:
                                        //throw new Exception("拎唄謀洗鰓機雷分項： " + element.GetAttribute("分項"));
                                        break;
                              }
                              #endregion
                         }
                    }
               }
               _entryScoreBase = new Dictionary<Control, string>();
               _entryScoreBase.Add(textBoxX1, textBoxX1.Text);
               _entryScoreBase.Add(textBoxX2, textBoxX2.Text);
               _entryScoreBase.Add(textBoxX3, textBoxX3.Text);
               _entryScoreBase.Add(textBoxX4, textBoxX4.Text);
               _entryScoreBase.Add(textBoxX5, textBoxX5.Text);
               _entryScoreBase.Add(textBoxX6, textBoxX6.Text);
               _entryScoreBase.Add(textBoxX7, textBoxX7.Text);
               _entryScoreBase.Add(textBoxX8, textBoxX8.Text);
               _entryScoreBase.Add(textBoxX9, textBoxX9.Text);
               _entryScoreBase.Add(textBoxX10, textBoxX10.Text);
               _entryScoreBase.Add(textBoxX11, textBoxX11.Text);
               _entryScoreBase.Add(textBoxX12, textBoxX12.Text);
               _entryScoreBase.Add(textBoxX13, textBoxX13.Text);

               _scoreUpdating = false;
               #endregion
               int tryint;
               if (int.TryParse(gradeYear, out tryint))
               {
                    cboAttendGradeYear.Text = gradeYear;
                    _beforeXml.AddElement("SubjectGradeYear", "GradeYear", cboAttendGradeYear.Text);
               }
          }

          private void CompareSubjectInfo(DataGridViewRow row)
          {
               if (!row.IsNewRow)
               {
                    #region 當課程資訊定義取自課程規畫表時
                    //將自課程規畫表取得的資料顯示於TOOLTIP
                    if (GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(_StudentID) != null)
                    {
                         #region 比對各項目資料
                         GraduationPlanInfo gplan = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(_StudentID);
                         GraduationPlanSubject subject = gplan.GetSubjectInfo("" + row.Cells[1].Value, "" + row.Cells[2].Value);
                         int index = 0;
                         if ("" + row.Cells[index].Value != subject.Entry)
                         {
                              row.Cells[index].ToolTipText = "在課程規劃表 \"" + gplan.Name + "\"中\n值為: " + subject.Entry;
                              row.Cells[index].Style.BackColor = Color.Gainsboro;
                              row.Cells[index].Style.ForeColor = Color.DimGray;
                         }
                         else
                         {
                              row.Cells[index].ToolTipText = "";
                              row.Cells[index].Style.BackColor = dataGridViewX1.DefaultCellStyle.BackColor; ;
                              row.Cells[index].Style.ForeColor = dataGridViewX1.DefaultCellStyle.ForeColor;
                         }
                         index = 3;
                         if ("" + row.Cells[index].Value != subject.Credit)
                         {
                              row.Cells[index].ToolTipText = "在課程規劃表 \"" + gplan.Name + "\"中\n值為: " + subject.Credit;
                              row.Cells[index].Style.BackColor = Color.Gainsboro;
                              row.Cells[index].Style.ForeColor = Color.DimGray;
                         }
                         else
                         {
                              row.Cells[index].ToolTipText = "";
                              row.Cells[index].Style.BackColor = dataGridViewX1.DefaultCellStyle.BackColor; ;
                              row.Cells[index].Style.ForeColor = dataGridViewX1.DefaultCellStyle.ForeColor;
                         }
                         index = 4;
                         if ("" + row.Cells[index].Value != subject.RequiredBy)
                         {
                              row.Cells[index].ToolTipText = "在課程規劃表 \"" + gplan.Name + "\"中\n值為: " + subject.RequiredBy;
                              row.Cells[index].Style.BackColor = Color.Gainsboro;
                              row.Cells[index].Style.ForeColor = Color.DimGray;
                         }
                         else
                         {
                              row.Cells[index].ToolTipText = "";
                              row.Cells[index].Style.BackColor = dataGridViewX1.DefaultCellStyle.BackColor; ;
                              row.Cells[index].Style.ForeColor = dataGridViewX1.DefaultCellStyle.ForeColor;
                         }
                         index = 5;
                         if ("" + row.Cells[index].Value != subject.Required)
                         {
                              row.Cells[index].ToolTipText = "在課程規劃表 \"" + gplan.Name + "\"中\n值為: " + subject.Required;
                              row.Cells[index].Style.BackColor = Color.Gainsboro;
                              row.Cells[index].Style.ForeColor = Color.DimGray;
                         }
                         else
                         {
                              row.Cells[index].ToolTipText = "";
                              row.Cells[index].Style.BackColor = dataGridViewX1.DefaultCellStyle.BackColor; ;
                              row.Cells[index].Style.ForeColor = dataGridViewX1.DefaultCellStyle.ForeColor;
                         }
                         index = 12;
                         if ((row.Cells[index].Value != null && (bool)row.Cells[index].Value) != subject.NotIncludedInCredit)
                         {
                              row.Cells[index].ToolTipText = "在課程規劃表 \"" + gplan.Name + "\"中\n值為: " + (subject.NotIncludedInCredit ? "是" : "否");
                              row.Cells[index].Style.BackColor = Color.Gainsboro;
                              row.Cells[index].Style.ForeColor = Color.DimGray;
                         }
                         else
                         {
                              row.Cells[index].ToolTipText = "";
                              row.Cells[index].Style.BackColor = dataGridViewX1.DefaultCellStyle.BackColor; ;
                              row.Cells[index].Style.ForeColor = dataGridViewX1.DefaultCellStyle.ForeColor;
                         }
                         index = 13;
                         if ((row.Cells[index].Value != null && (bool)row.Cells[index].Value) != subject.NotIncludedInCalc)
                         {
                              row.Cells[index].ToolTipText = "在課程規劃表 \"" + gplan.Name + "\"中\n值為: " + (subject.NotIncludedInCalc ? "是" : "否");
                              row.Cells[index].Style.BackColor = Color.Gainsboro;
                              row.Cells[index].Style.ForeColor = Color.DimGray;
                         }
                         else
                         {
                              row.Cells[index].ToolTipText = "";
                              row.Cells[index].Style.BackColor = dataGridViewX1.DefaultCellStyle.BackColor; ;
                              row.Cells[index].Style.ForeColor = dataGridViewX1.DefaultCellStyle.ForeColor;
                         }
                         #endregion
                    }
                    else
                    {
                         //改變顏色
                         foreach (int index in new int[] { 0, 3, 4, 5, 12, 13 })
                         {
                              row.Cells[index].ToolTipText = "學生課程規劃表未設定，無法比較與課程規畫表差異。";
                              row.Cells[index].Style.BackColor = Color.Gainsboro;
                              row.Cells[index].Style.ForeColor = Color.DimGray;
                         }
                    }
                    #endregion
               }
               else
               {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                         cell.ToolTipText = "";
                         cell.Style.BackColor = dataGridViewX1.DefaultCellStyle.BackColor; ;
                         cell.Style.ForeColor = dataGridViewX1.DefaultCellStyle.ForeColor;
                    }
               }
          }

          private void comboBoxEx3_TextChanged(object sender, EventArgs e)
          {
               int s = 0;
               if (!int.TryParse(cboAttendGradeYear.Text, out s))
               {
                    errorGradeYear.Icon = Properties.Resources.error;
                    errorGradeYear.SetError(cboAttendGradeYear, "必須輸入數字");
               }
               else
                    errorGradeYear.Clear();
               if (!int.TryParse(cboAttendGradeYear.Text, out s))
               {
                    errorGradeYear.Icon = Properties.Resources.error;
                    errorGradeYear.SetError(cboAttendGradeYear, "修課年級必須填寫");
               }
               else
                    errorGradeYear.Clear();
          }

          private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
          {
               e.Cancel = true;
               DataGridViewCell cell = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex];
               string message = "儲存格值：" + cell.Value + "。\n發生錯誤： " + e.Exception.Message + "。";
               if (cell.ErrorText != message)
               {
                    cell.ErrorText = message;
                    dataGridViewX1.UpdateCellErrorText(e.ColumnIndex, e.RowIndex);
               }
          }

          private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
          {
               if (dataGridViewX1.SelectedCells.Count == 1)
                    dataGridViewX1.BeginEdit(true);
          }

          private void buttonX1_Click(object sender, EventArgs e)
          {
               dataGridViewX1.EndEdit();
               if (!ValidateAll()) return;
               #region 新增修改科目成績資料
               XmlElement subjectScoreInfo = CreateSubjectScoreElement();
               if (_SubjectScoreID != "")
               {
                    if (subjectScoreInfo.SelectNodes("Subject").Count > 0)
                         EditScore.UpdateSemesterSubjectScore(_SubjectScoreID, cboAttendGradeYear.Text, subjectScoreInfo);
                    else
                         RemoveScore.DeleteSemesterSubjectScore(_SubjectScoreID);
               }
               else
               {
                    if (subjectScoreInfo.SelectNodes("Subject").Count > 0)
                         AddScore.InsertSemesterSubjectScore(_StudentID, comboBoxEx1.Text, comboBoxEx2.Text, cboAttendGradeYear.Text, subjectScoreInfo);
               }
               #endregion

               _afterXml.AddElement("LockScore", "Lock", "" + buttonItem4.Checked);
               #region 新增修改分項成績資料

               XmlDocument doc = new XmlDocument();
               XmlElement entryScoreInfo1 = doc.CreateElement("SemesterEntryScore");
               XmlElement entryScoreInfo2 = doc.CreateElement("SemesterEntryScore");
               XmlElement entryElement;
               double test = 0;
               #region 行為類
               if (double.TryParse(textBoxX2.Text, out test))
               {
                    entryElement = doc.CreateElement("Entry");
                    entryElement.SetAttribute("分項", "德行");
                    entryElement.SetAttribute("成績", textBoxX2.Text);
                    entryElement.SetAttribute("鎖定", "" + buttonItem4.Checked);
                    entryScoreInfo1.AppendChild(entryElement);
                    _afterXml.AddElement("EntryCollection", entryElement);
                    if (_EntryScoreID2 != "")
                    {
                         EditScore.UpdateSemesterEntryScore(_EntryScoreID2, cboAttendGradeYear.Text, entryScoreInfo1);
                    }
                    else
                         AddScore.InsertSemesterEntryScore(_StudentID, comboBoxEx1.Text, comboBoxEx2.Text, cboAttendGradeYear.Text, "行為", entryScoreInfo1);
               }
               else
               {
                    if (_EntryScoreID2 != "")
                         RemoveScore.DeleteSemesterEntityScore(_EntryScoreID2);
               }
               #endregion
               #region 學習類
               if (double.TryParse(textBoxX1.Text, out test)
                   || double.TryParse(textBoxX3.Text, out test)
                   || double.TryParse(textBoxX5.Text, out test)
                   || double.TryParse(textBoxX4.Text, out test)
                   || double.TryParse(textBoxX6.Text, out test)
                   || double.TryParse(textBoxX7.Text, out test)
                   || double.TryParse(textBoxX8.Text, out test)
                   || double.TryParse(textBoxX9.Text, out test)
                   || double.TryParse(textBoxX10.Text, out test)
                   || double.TryParse(textBoxX11.Text, out test)
                   || double.TryParse(textBoxX12.Text, out test)
                   || double.TryParse(textBoxX13.Text, out test)
                   )
               {
                    if (double.TryParse(textBoxX1.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "學業");
                         entryElement.SetAttribute("成績", textBoxX1.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX3.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "體育");
                         entryElement.SetAttribute("成績", textBoxX3.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX5.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "健康與護理");
                         entryElement.SetAttribute("成績", textBoxX5.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX4.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "國防通識");
                         entryElement.SetAttribute("成績", textBoxX4.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX6.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "實習科目");
                         entryElement.SetAttribute("成績", textBoxX6.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX7.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "專業科目");
                         entryElement.SetAttribute("成績", textBoxX7.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX13.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "學業(原始)");
                         entryElement.SetAttribute("成績", textBoxX13.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX12.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "體育(原始)");
                         entryElement.SetAttribute("成績", textBoxX12.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX11.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "健康與護理(原始)");
                         entryElement.SetAttribute("成績", textBoxX11.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX10.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "國防通識(原始)");
                         entryElement.SetAttribute("成績", textBoxX10.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX9.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "實習科目(原始)");
                         entryElement.SetAttribute("成績", textBoxX9.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (double.TryParse(textBoxX8.Text, out test))
                    {
                         entryElement = doc.CreateElement("Entry");
                         entryElement.SetAttribute("分項", "專業科目(原始)");
                         entryElement.SetAttribute("成績", textBoxX8.Text);
                         entryScoreInfo2.AppendChild(entryElement);
                         _afterXml.AddElement("EntryCollection", entryElement);
                    }
                    if (_EntryScoreID1 != "")
                    {
                         EditScore.UpdateSemesterEntryScore(_EntryScoreID1, cboAttendGradeYear.Text, entryScoreInfo2);
                    }
                    else
                         AddScore.InsertSemesterEntryScore(_StudentID, comboBoxEx1.Text, comboBoxEx2.Text, cboAttendGradeYear.Text, "學習", entryScoreInfo2);
               }
               else
               {
                    if (_EntryScoreID1 != "")
                         RemoveScore.DeleteSemesterEntityScore(_EntryScoreID1);
               }
               #endregion
               #endregion

               #region 處理Log

               StringBuilder desc = new StringBuilder("");
               desc.AppendLine("學生姓名：" + Student.Instance.Items[_StudentID].Name + " ");
               desc.AppendLine(comboBoxEx1.Text + " 學年度 第" + comboBoxEx2.Text + "學期 ");

               #region 處理修課年級log
               if (_beforeXml.GetText("SubjectGradeYear/GradeYear") != _afterXml.GetText("SubjectGradeYear/GradeYear"))
                    desc.AppendLine("修課年級由「" + _beforeXml.GetText("SubjectGradeYear/GradeYear") + "」變更為「" + _afterXml.GetText("SubjectGradeYear/GradeYear") + "」");
               #endregion

               #region 處理鎖定成績
               if (_beforeXml.GetText("LockScore/Lock") != _afterXml.GetText("LockScore/Lock"))
                    desc.AppendLine("鎖定德行分項成績由「" + _beforeXml.GetText("LockScore/Lock") + "」變更為「" + _afterXml.GetText("LockScore/Lock") + "」");
               #endregion

               #region 處理科目成績log

               //設定分隔符號
               string splitSign = "#$@%#";

               foreach (XmlElement var in _beforeXml.GetElements("SubjectCollection/Subject"))
               {
                    string prefix = var.GetAttribute("科目") + splitSign + var.GetAttribute("科目級別") + splitSign;

                    foreach (XmlAttribute attr in var.Attributes)
                    {
                         if (!_beforeData.ContainsKey(prefix + attr.Name))
                              _beforeData.Add(prefix + attr.Name, attr.InnerText);
                    }
               }

               foreach (XmlElement var in _afterXml.GetElements("SubjectCollection/Subject"))
               {
                    string prefix = var.GetAttribute("科目") + splitSign + var.GetAttribute("科目級別") + splitSign;

                    foreach (XmlAttribute attr in var.Attributes)
                    {
                         if (!_afterData.ContainsKey(prefix + attr.Name))
                              _afterData.Add(prefix + attr.Name, attr.InnerText);
                    }
               }

               desc.AppendLine("科目成績：");

               string delItem = "";
               foreach (string var in _beforeData.Keys)
               {
                    string[] splitWord = var.Split(new string[] { splitSign }, StringSplitOptions.None);

                    if (!_afterData.ContainsKey(var))
                    {
                         if (splitWord[0] + splitSign + splitWord[1] != delItem)
                              desc.AppendLine("刪除科目「" + splitWord[0] + ((splitWord[1] == "") ? "" : " " + GetNumber(int.Parse(splitWord[1]))) + "」");
                         delItem = splitWord[0] + splitSign + splitWord[1];
                    }
                    else
                    {
                         if (_beforeData[var] != _afterData[var])
                         {
                              desc.AppendLine("科目「" + splitWord[0] + ((splitWord[1] == "") ? "" : " " + GetNumber(int.Parse(splitWord[1]))) + "」的欄位「" + splitWord[2] + "」由「" + _beforeData[var] + "」變更為「" + _afterData[var] + "」");
                         }
                         _afterData.Remove(var);
                    }
               }

               string newItem = "";
               foreach (string var in _afterData.Keys)
               {
                    string[] splitWord = var.Split(new string[] { splitSign }, StringSplitOptions.None);

                    if (splitWord[0] + splitSign + splitWord[1] != newItem)
                         desc.AppendLine("新增科目「" + splitWord[0] + ((splitWord[1] == "") ? "" : " " + GetNumber(int.Parse(splitWord[1]))) + "」");
                    newItem = splitWord[0] + splitSign + splitWord[1];
                    if (_afterData[var] != "")
                         desc.AppendLine("科目「" + splitWord[0] + ((splitWord[1] == "") ? "" : " " + GetNumber(int.Parse(splitWord[1]))) + "」的欄位「" + splitWord[2] + "」為「" + _afterData[var] + "」");
               }

               _beforeData.Clear();
               _afterData.Clear();

               #endregion

               #region 處理分項成績log

               foreach (XmlElement var in _beforeXml.GetElements("EntryCollection/Entry"))
               {
                    if (!_beforeData.ContainsKey(var.GetAttribute("分項")))
                         _beforeData.Add(var.GetAttribute("分項"), var.GetAttribute("成績"));
                    if (!_afterData.ContainsKey(var.GetAttribute("分項")))
                         _afterData.Add(var.GetAttribute("分項"), "");
               }

               foreach (XmlElement var in _afterXml.GetElements("EntryCollection/Entry"))
               {
                    if (!_afterData.ContainsKey(var.GetAttribute("分項")))
                         _afterData.Add(var.GetAttribute("分項"), var.GetAttribute("成績"));
                    else
                         _afterData[var.GetAttribute("分項")] = var.GetAttribute("成績");
                    if (!_beforeData.ContainsKey(var.GetAttribute("分項")))
                         _beforeData.Add(var.GetAttribute("分項"), "");
               }

               desc.AppendLine("分項成績：");

               foreach (string var in _afterData.Keys)
               {
                    if (_beforeData[var] != _afterData[var])
                         desc.AppendLine("「" + var + "成績」由「" + _beforeData[var] + "」變更為「" + _afterData[var] + "」");
               }

               #endregion

               CurrentUser.Instance.AppLog.Write(EntityType.Student, _entityAction, _StudentID, desc.ToString(), Text, _afterXml.GetRawXml());

               #endregion

               EventHub.Instance.InvokScoreChanged(_StudentID);
               this.Close();
          }

          private XmlElement CreateSubjectScoreElement()
          {
               XmlDocument doc = new XmlDocument();
               XmlElement subjectScoreInfo = doc.CreateElement("SemesterSubjectScoreInfo");

               _afterXml.AddElement("SubjectGradeYear", "GradeYear", cboAttendGradeYear.Text);

               foreach (DataGridViewRow row in dataGridViewX1.Rows)
               {
                    if (row.IsNewRow)
                         break;
                    XmlElement subjectElement = doc.CreateElement("Subject");
                    subjectElement.SetAttribute("開課分項類別", "" + row.Cells[0].Value);
                    subjectElement.SetAttribute("科目", "" + row.Cells[1].Value);
                    subjectElement.SetAttribute("科目級別", "" + row.Cells[2].Value);
                    subjectElement.SetAttribute("開課學分數", "" + row.Cells[3].Value);
                    subjectElement.SetAttribute("修課校部訂", "" + row.Cells[4].Value);
                    subjectElement.SetAttribute("修課必選修", "" + row.Cells[5].Value);
                    subjectElement.SetAttribute("是否取得學分", (row.Cells[6].Value != null && (bool)row.Cells[6].Value) ? "是" : "否");
                    subjectElement.SetAttribute("原始成績", "" + row.Cells[7].Value);
                    subjectElement.SetAttribute("補考成績", "" + row.Cells[8].Value);
                    subjectElement.SetAttribute("重修成績", "" + row.Cells[9].Value);
                    subjectElement.SetAttribute("擇優採計成績", "" + row.Cells[10].Value);
                    subjectElement.SetAttribute("學年調整成績", "" + row.Cells[11].Value);
                    subjectElement.SetAttribute("不計學分", (row.Cells[12].Value != null && (bool)row.Cells[12].Value) ? "是" : "否");
                    subjectElement.SetAttribute("不需評分", (row.Cells[13].Value != null && (bool)row.Cells[13].Value) ? "是" : "否");
                    subjectElement.SetAttribute("註記", "" + row.Cells[14].Value);
                    subjectScoreInfo.AppendChild(subjectElement);

                    _afterXml.AddElement("SubjectCollection", subjectElement);
               }
               return subjectScoreInfo;
          }

          #region 資料驗證相關
          private bool ValidateAll()
          {
               errorSchoolYear.Clear();
               errorSemester.Clear();
               errorGradeYear.Clear();
               bool validatePass = true;
               #region 檢查輸入欄位值
               int s = 0;
               if (!int.TryParse(comboBoxEx1.Text, out s))
               {
                    validatePass &= false;
                    errorSchoolYear.Icon = Properties.Resources.error;
                    errorSchoolYear.SetError(comboBoxEx1, "必須輸入數字");
               }
               if (!int.TryParse(comboBoxEx2.Text, out s))
               {
                    validatePass &= false;
                    errorSemester.Icon = Properties.Resources.error;
                    errorSemester.SetError(comboBoxEx2, "必須輸入數字");
               }
               if (!int.TryParse(cboAttendGradeYear.Text, out s))
               {
                    validatePass &= false;
                    errorGradeYear.Icon = Properties.Resources.error;
                    errorGradeYear.SetError(cboAttendGradeYear, "必須輸入數字");
               }
               #endregion
               #region 檢查空值
               if (comboBoxEx1.Text == "")
               {
                    validatePass &= false;
                    errorSchoolYear.Icon = Properties.Resources.error;
                    errorSchoolYear.SetError(comboBoxEx1, "必須填寫");
               }
               if (comboBoxEx2.Text == "")
               {
                    validatePass &= false;
                    errorSemester.Icon = Properties.Resources.error;
                    errorSemester.SetError(comboBoxEx2, "必須填寫");
               }
               if (!int.TryParse(cboAttendGradeYear.Text, out s))
               {
                    validatePass &= false;
                    errorGradeYear.Icon = Properties.Resources.error;
                    errorGradeYear.SetError(cboAttendGradeYear, "修課年級必須填寫");
               }
               #endregion
               #region 檢查DataGridView資料正確
               foreach (DataGridViewRow row in dataGridViewX1.Rows)
               {
                    if (row.IsNewRow)
                         break;
                    validatePass &= ValidateRow(row.Index);
               }
               #endregion
               return validatePass;
          }

          private bool ValidateRow(int p)
          {
               bool validatePass = true;
               DataGridViewRow row = dataGridViewX1.Rows[p];
               if (row.IsNewRow) return true;
               CompareSubjectInfo(row);

               foreach (int i in new int[] { 0, 1, 3, 4, 5 })
               {
                    row.Cells[i].ErrorText = "";
                    if ("" + row.Cells[i].Value == "")
                    {
                         validatePass &= false;
                         row.Cells[i].ErrorText = "此為必填欄位";
                    }
                    dataGridViewX1.UpdateCellErrorText(i, row.Index);
               }

               foreach (int i in new int[] { 2 })
               {
                    row.Cells[i].ErrorText = "";
                    int x = 0;
                    if ("" + row.Cells[i].Value != "" && !int.TryParse("" + row.Cells[i].Value, out x))
                    {
                         validatePass &= false;
                         row.Cells[i].ErrorText = "必須輸入整數";
                    }
                    dataGridViewX1.UpdateCellErrorText(i, row.Index);
               }
               foreach (int i in new int[] { 3 })
               {
                    row.Cells[i].ErrorText = "";
                    decimal x = 0;
                    if ("" + row.Cells[i].Value != "" && !decimal.TryParse("" + row.Cells[i].Value, out x))
                    {
                         validatePass &= false;
                         row.Cells[i].ErrorText = "必須輸入數字";
                    }
                    dataGridViewX1.UpdateCellErrorText(i, row.Index);
               }
               foreach (int i in new int[] { 7, 8, 9, 10, 11 })
               {
                    row.Cells[i].ErrorText = "";
                    double x = 0;
                    if ("" + row.Cells[i].Value != "" && !double.TryParse("" + row.Cells[i].Value, out x))
                    {
                         validatePass &= false;
                         row.Cells[i].ErrorText = "必須輸入數字";
                    }
                    dataGridViewX1.UpdateCellErrorText(i, row.Index);
               }
               foreach (DataGridViewCell cell in row.Cells)
               {
                    validatePass &= (cell.ErrorText == "");
               }
               return validatePass;
          }

          private bool ValidateSchoolYearSemester()
          {
               errorSchoolYear.Clear();
               errorSemester.Clear();
               bool validatePass = true;
               #region 檢查輸入欄位值
               int s = 0;
               if (!int.TryParse(comboBoxEx1.Text, out s))
               {
                    validatePass &= false;
                    errorSchoolYear.Icon = Properties.Resources.error;
                    errorSchoolYear.SetError(comboBoxEx1, "必須輸入數字");
               }
               if (!int.TryParse(comboBoxEx2.Text, out s))
               {
                    validatePass &= false;
                    errorSemester.Icon = Properties.Resources.error;
                    errorSemester.SetError(comboBoxEx2, "必須輸入數字");
               }
               #endregion
               #region 檢查空值
               if (comboBoxEx1.Text == "")
               {
                    validatePass &= false;
                    errorSchoolYear.Icon = Properties.Resources.error;
                    errorSchoolYear.SetError(comboBoxEx1, "必須填寫");
               }
               if (comboBoxEx2.Text == "")
               {
                    validatePass &= false;
                    errorSemester.Icon = Properties.Resources.error;
                    errorSemester.SetError(comboBoxEx2, "必須填寫");
               }
               #endregion
               return validatePass;
          }

          private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
          {
               ValidateRow(e.RowIndex);

               try
               {
                    DataGridViewRow row = dataGridViewX1.Rows[e.RowIndex];
                    row.Cells[SubjectColumn].ToolTipText = GetSubjectScorePlace(row);
               }
               catch { }
          }
          #endregion

          private string GetNumber(int p)
          {
               string levelNumber;
               switch (p)
               {
                    #region 對應levelNumber
                    case 1:
                         levelNumber = "I";
                         break;
                    case 2:
                         levelNumber = "II";
                         break;
                    case 3:
                         levelNumber = "III";
                         break;
                    case 4:
                         levelNumber = "IV";
                         break;
                    case 5:
                         levelNumber = "V";
                         break;
                    case 6:
                         levelNumber = "VI";
                         break;
                    case 7:
                         levelNumber = "VII";
                         break;
                    case 8:
                         levelNumber = "VIII";
                         break;
                    case 9:
                         levelNumber = "IX";
                         break;
                    case 10:
                         levelNumber = "X";
                         break;
                    default:
                         levelNumber = "" + (p);
                         break;
                    #endregion
               }
               return levelNumber;
          }

          private void buttonX3_Click(object sender, EventArgs e)
          {
               if (ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(_StudentID) == null)
               {
                    MsgBox.Show("學生尚未設定計算標準");
                    return;
               }
               XmlElement newEntryScoreElement = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(_StudentID).CalculateSemesterEntryScore(CreateSubjectScoreElement());
               foreach (XmlNode score in newEntryScoreElement.SelectNodes("Entry"))
               {
                    XmlElement element = (XmlElement)score;
                    _beforeXml.AddElement("EntryCollection", element);

                    #region 依分項填入格子
                    switch (element.GetAttribute("分項"))
                    {
                         case "學業":
                              textBoxX1.Text = element.GetAttribute("成績");
                              break;
                         case "體育":
                              textBoxX3.Text = element.GetAttribute("成績");
                              break;
                         case "國防通識":
                              textBoxX4.Text = element.GetAttribute("成績");
                              break;
                         case "健康與護理":
                              textBoxX5.Text = element.GetAttribute("成績");
                              break;
                         case "實習科目":
                              textBoxX6.Text = element.GetAttribute("成績");
                              break;
                         case "專業科目":
                              textBoxX7.Text = element.GetAttribute("成績");
                              break;
                         case "學業(原始)":
                              textBoxX13.Text = element.GetAttribute("成績");
                              break;
                         case "體育(原始)":
                              textBoxX12.Text = element.GetAttribute("成績");
                              break;
                         case "國防通識(原始)":
                              textBoxX10.Text = element.GetAttribute("成績");
                              break;
                         case "健康與護理(原始)":
                              textBoxX11.Text = element.GetAttribute("成績");
                              break;
                         case "實習科目(原始)":
                              textBoxX9.Text = element.GetAttribute("成績");
                              break;
                         case "專業科目(原始)":
                              textBoxX8.Text = element.GetAttribute("成績");
                              break;
                         case "德行":
                              textBoxX2.Text = element.GetAttribute("成績");
                              break;
                         default:
                              //throw new Exception("拎唄謀洗鰓機雷分項： " + element.GetAttribute("分項"));
                              break;
                    }
                    #endregion
               }
               //newEntryScoreElement.OwnerDocument.AppendChild(newEntryScoreElement);
               //newEntryScoreElement.OwnerDocument.Save("D:/1234.xml");
          }

          private void entryScoreChanged(object sender, EventArgs e)
          {
               TextBoxX control = (TextBoxX)sender;
               ResetErrorProvider(control);
               if (control == textBoxX2)
               {
                    if (!_scoreUpdating && _entryScoreBase.ContainsKey(control) && control.Text != _entryScoreBase[control])
                    {
                         SetErrorProvider(control, "由\"" + _entryScoreBase[control] + "\" 修改為 \"" + control.Text + "\"");
                         buttonItem4.Checked = true;
                    }
                    else
                    {
                         buttonItem3.Checked = true;
                    }
               }
               else if (!_scoreUpdating && _entryScoreBase.ContainsKey(control) && control.Text != _entryScoreBase[control])
               {
                    SetErrorProvider(control, "由\"" + _entryScoreBase[control] + "\" 修改為 \"" + control.Text + "\"");
               }
          }
          private void SetErrorProvider(Control control, string p)
          {
               if (!_errorProviderDictionary.ContainsKey(control))
               {
                    ErrorProvider ep = new ErrorProvider();
                    ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                    ep.SetIconAlignment(control, ErrorIconAlignment.MiddleRight);
                    ep.Icon = Properties.Resources.Info3D;
                    ep.SetError(control, p);
                    _errorProviderDictionary.Add(control, ep);
               }
          }

          private void ResetErrorProvider(Control control)
          {
               if (_errorProviderDictionary.ContainsKey(control))
               {
                    _errorProviderDictionary[control].Clear();
                    _errorProviderDictionary.Remove(control);
               }
          }

          private void dataGridViewX1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
          {
               dataGridViewX1.EndEdit();
          }

          private void textBoxX2_ButtonCustomClick(object sender, EventArgs e)
          {
               buttonX4.Popup(groupPanel2.PointToScreen(buttonX4.Location));
          }

          private void buttonItem4_CheckedChanged(object sender, EventArgs e)
          {
               //textBoxX2.BackColor = buttonItem4.Checked ? Color.PaleGoldenrod : textBoxX3.BackColor;
               textBoxX2.ButtonCustom2.Visible = buttonItem4.Checked;
               textBoxX2.ButtonCustom.Visible = !textBoxX2.ButtonCustom2.Visible;
          }

          private string GetSubjectScorePlace(DataGridViewRow row)
          {
               if (_subject_rating != null)
                    return _subject_rating.GetTooltip(row);
               else
                    return string.Empty;
          }

          class SubjectScoreToolTipProvider
          {
               private Dictionary<string, ScorePlace> _class_rating;
               private Dictionary<string, ScorePlace> _dept_rating;
               private Dictionary<string, ScorePlace> _year_rating;

               public SubjectScoreToolTipProvider(XmlElement classRating, XmlElement deptRating, XmlElement yearRating)
               {
                    _class_rating = new Dictionary<string, ScorePlace>();
                    _dept_rating = new Dictionary<string, ScorePlace>();
                    _year_rating = new Dictionary<string, ScorePlace>();

                    CreatePlaceObjects(classRating, _class_rating);
                    CreatePlaceObjects(deptRating, _dept_rating);
                    CreatePlaceObjects(yearRating, _year_rating);
               }

               private void CreatePlaceObjects(XmlElement ratingData, Dictionary<string, ScorePlace> target)
               {
                    DSXmlHelper temp;
                    foreach (XmlElement eachPlace in ratingData.SelectNodes("Rating/Item"))
                    {
                         temp = new DSXmlHelper(eachPlace);
                         string key = temp.GetText("@科目") + temp.GetText("@科目級別");

                         if (target.ContainsKey(key)) continue;

                         target.Add(key, new ScorePlace(eachPlace, temp.GetText("../@範圍人數")));
                    }
               }

               public string GetTooltip(DataGridViewRow row)
               {
                    //第1欄是科目名稱，第2欄是級別，如果改了的話....。
                    string key = row.Cells[SubjectColumn].Value + string.Empty + row.Cells[SubjectLevel].Value;
                    ScorePlace temp;
                    StringBuilder tooltip = new StringBuilder();

                    if (_class_rating.TryGetValue(key, out temp))
                         tooltip.AppendFormat("班排名：{0}\n", temp.Place);

                    if (_dept_rating.TryGetValue(key, out temp))
                         tooltip.AppendFormat("科排名：{0}\n", temp.Place);

                    if (_year_rating.TryGetValue(key, out temp))
                         tooltip.AppendFormat("年排名：{0}\n", temp.Place);

                    return tooltip.ToString();
               }

               class ScorePlace
               {
                    public ScorePlace(XmlElement placeData, string ratingBase)
                    {
                         DSXmlHelper hlpData = new DSXmlHelper(placeData);

                         _score = hlpData.GetText("@成績");
                         _rating_base = ratingBase;
                         _actual_base = hlpData.GetText("@成績人數");
                         _place = hlpData.GetText("@排名");
                    }

                    private string _score;
                    public string Score
                    {
                         get { return _score; }
                    }

                    private string _actual_base;
                    public string ActualBase
                    {
                         get { return _actual_base; }
                    }

                    private string _rating_base;
                    public string RatingBase
                    {
                         get { return _rating_base; }
                    }

                    private string _place;
                    public string Place
                    {
                         get { return _place; }
                    }
               }
          }

          class EntryScoreToolTipProvider
          {
               private System.Windows.Forms.ToolTip _obj_tooltip = new System.Windows.Forms.ToolTip();

               public EntryScoreToolTipProvider(string entryGroup, XmlElement semesterEntryScore)
               {
                    XmlElement classRating, deptRating, yearRating;
                    DSXmlHelper hlpEntryScore = new DSXmlHelper(semesterEntryScore);
                    classRating = hlpEntryScore.GetElement("ClassRating");
                    deptRating = hlpEntryScore.GetElement("DeptRating");
                    yearRating = hlpEntryScore.GetElement("YearRating");

                    StringBuilder tooltip = new StringBuilder();
                    string path = string.Format("Rating/Item[@分項='{0}']", entryGroup);

                    DSXmlHelper temp = new DSXmlHelper(classRating);
                    if (temp.PathExist(path))
                         tooltip.AppendFormat("班排名：{0}\n", temp.GetText(path + "/@排名"));

                    temp = new DSXmlHelper(deptRating);
                    if (temp.PathExist(path))
                         tooltip.AppendFormat("科排名：{0}\n", temp.GetText(path + "/@排名"));

                    temp = new DSXmlHelper(yearRating);
                    if (temp.PathExist(path))
                         tooltip.AppendFormat("年排名：{0}\n", temp.GetText(path + "/@排名"));

                    _score_tooltip = tooltip.ToString();
               }

               private string _score_tooltip;
               public string ScoreToolTip
               {
                    get { return _score_tooltip; }
               }

               public void SetTooltip(Control ctl)
               {
                    if (_obj_tooltip.CanExtend(ctl))
                         _obj_tooltip.SetToolTip(ctl, ScoreToolTip);
               }
          }
     }
}