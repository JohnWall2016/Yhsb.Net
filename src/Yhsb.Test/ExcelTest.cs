using Yhsb.Util;
using Yhsb.Util.Excel;

public class ExcelTest
{
    public static void TestExcel()
    {
        var workbook = ExcelExtension.LoadExcel(@"D:\精准扶贫\雨湖区精准扶贫底册模板.xlsx");
        var sheet = workbook.GetSheetAt(0);

        var row = sheet.Row(2);
        row.Cell("U").SetValue("属于贫困人员");
        row.Cell("V").SetValue("认定身份");

        workbook.Save($@"D:\精准扶贫\雨湖区精准扶贫底册模板{DateTime.FormatedDate()}.xlsx");
    }
}