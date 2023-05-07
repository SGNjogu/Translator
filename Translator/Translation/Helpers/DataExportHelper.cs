using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translation.DataService.Interfaces;
using Translation.Utils;

namespace Translation.Helpers
{
    public interface IDataExportHelper
    {
        Task<MemoryStream> GenerateSessionData(int sessionId);
    }

    public class DataExportHelper : IDataExportHelper
    {
        private readonly IDataService _dataService;

        public DataExportHelper(IDataService dataService)
        {
            _dataService = dataService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<MemoryStream> GenerateSessionData(int sessionId)
        {
            try
            {
                var session = await _dataService.GetOneSessionAsync(sessionId);
                var transcriptions = await _dataService.GetSessionTranscriptions(sessionId);

                if (!transcriptions.Any())
                    throw new Exception($"The Session has no transcriptions. Export is not possible.");

                ExcelPackage package = new ExcelPackage();
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add($"SessionId {sessionId}");
                int totalRows = transcriptions.Count;

                worksheet.Cells[1, 1].Value = "Session Date";
                worksheet.Cells[1, 2].Value = "Session Id";
                worksheet.Cells[1, 3].Value = "Session Duration (mm:ss)";
                worksheet.Cells[1, 4].Value = "Original Language";
                worksheet.Cells[1, 5].Value = "Translated Language";
                worksheet.Cells[1, 6].Value = "User";
                worksheet.Cells[1, 7].Value = "Original Text";
                worksheet.Cells[1, 8].Value = "Translated Text";
                worksheet.Cells[1, 9].Value = "Transcription Timestamp";
                worksheet.Cells[1, 10].Value = "Text Sentiment";

                int i = 0;
                for (int row = 2; row <= totalRows + 1; row++)
                {
                    worksheet.Cells[row, 1].Value = DateTimeUtility.ReturnDateStringFromLong(DateTimeUtility.Format.FullDayMonthYear, session.StartTime);
                    worksheet.Cells[row, 2].Value = transcriptions[i].SessionId;
                    worksheet.Cells[row, 3].Value = session.Duration;
                    worksheet.Cells[row, 4].Value = session.SourceLangISO;
                    worksheet.Cells[row, 5].Value = session.TargetLangIso;
                    worksheet.Cells[row, 6].Value = transcriptions[i].ChatUser;
                    worksheet.Cells[row, 7].Value = transcriptions[i].OriginalText;
                    worksheet.Cells[row, 8].Value = transcriptions[i].TranslatedText;
                    worksheet.Cells[row, 9].Value = transcriptions[i].ChatTime;
                    worksheet.Cells[row, 10].Value = transcriptions[i].Sentiment;
                    i++;
                }

                var memoryStream = new MemoryStream();
                package.SaveAs(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
