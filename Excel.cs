using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;

namespace TelegramBotTranslate
{
    public class Excel
    {
        string path = string.Empty;
        _Application excel = new Application();
        Workbook wb;
        Worksheet ws;
        public Excel(string path, int Sheet)
        {
            this.path = path;
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[Sheet];
        }
        public int GetColumn()
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
        public Word? ReadWordFromExcelString(int i)
        {
            var rus = ws.Cells[i + 1, 1].Value2;
            var eng = ws.Cells[i + 1, 3].Value2;

            if (rus == null) return null;

            return new Word { Russian = rus, English = eng };
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
