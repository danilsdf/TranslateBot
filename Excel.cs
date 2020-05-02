using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;

namespace TelegramBotTranslate
{
    public class Excel
    {
        string path = string.Empty;
        _Application excel = new _Excel.Application();
        Workbook wb;
        Worksheet ws;
        public Excel(string path, int Sheet)
        {
            this.path = path;
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[Sheet];
        }
        public int GetColump()
        {
            int CountOfColumbs = 0;
            for (int i = 1; ws.Cells[i, 1].Value2 != null; i++)
            { CountOfColumbs++; }
            return --CountOfColumbs;
        }
        public int GetSheet()
        { 
            return wb.Worksheets.Count;
        }
        public string[] GetNames()
        {
            string[] names = new string[this.GetSheet()];
            for (int i = 1; i <= this.GetSheet(); i++)
            {
                this.SelectWorkSheet(i);
                names[i - 1] = ws.Name;
            }
            return names;

        }
        public string ReadExcelString(int i, int j)
        {
            //i++;j++;
            if (ws.Cells[i + 1, j + 1].Value2 != null)
            {
                return ws.Cells[i + 1, j + 1].Value2;
            }
            else return string.Empty;
        }
        public void SelectWorkSheet(int n)
        {
            ws = wb.Worksheets[n];
        }
        public void Close()
        {
            wb.Close();
            excel.Quit();
        }
    }
}
