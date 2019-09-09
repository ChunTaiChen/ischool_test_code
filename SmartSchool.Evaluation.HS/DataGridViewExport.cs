using System;
using System.Collections.Generic;
using System.Text;
using Aspose.Cells;
using System.Windows.Forms;

namespace SmartSchool.Evaluation.Process
{
    //匯出DataGridView內容的小工具
    internal class DataGridViewExport
    {
        Workbook _workbook;
        Worksheet _worksheet;
        List<int> _colIndexes;

        public DataGridViewExport(DataGridView dgv)
        {
            _workbook = new Workbook();
            _workbook.Worksheets.Clear();
            _worksheet = _workbook.Worksheets[_workbook.Worksheets.Add()];
            _worksheet.Name = "Sheet1";
            _colIndexes = new List<int>();

            int sheetRowIndex = 0;
            int sheetColIndex = 0;
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible == false) continue;
                _colIndexes.Add(col.Index);
                _worksheet.Cells[sheetRowIndex, sheetColIndex++].PutValue(col.HeaderText);
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                sheetRowIndex++;
                sheetColIndex = 0;
                foreach (int colIndex in _colIndexes)
                    _worksheet.Cells[sheetRowIndex, sheetColIndex++].PutValue("" + row.Cells[colIndex].Value);
            }

            _worksheet.AutoFitColumns();
        }

        public void Save(string path)
        {
            try
            {
                _workbook.Save(path, FileFormatType.Excel2003);

            }
            catch (Exception ex)
            {
                MessageBox.Show("匯出失敗：" + ex.Message);
            }
        }
    }
}
