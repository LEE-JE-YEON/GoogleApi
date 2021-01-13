using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AccessGoogleSheets
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitGoogleApi();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void InitGoogleApi()
        {
            UserCredential credential;

            using (var stream = new FileStream("..\\..\\credentials.json", FileMode.Open, FileAccess.Read))
            {
                //string credentialsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                //credentialsPath = Path.Combine(credentialsPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                //credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                //    Scopes,
                //    "user",
                //    CancellationToken.None,
                //    new FileDataStore(credentialsPath, true)).Result;

                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });

            
            // 구글 스프레드 시트에서 읽기
            string readRange = "Sheet1!A1:E5";
            SpreadsheetsResource.ValuesResource.GetRequest readRequest = _sheetsService.Spreadsheets.Values.Get(_spreadSheetsId, readRange);
            ValueRange readResponse = readRequest.Execute();
            IList<IList<object>> values = readResponse.Values;
            foreach(var rows in values )
            {
                Console.WriteLine();
                foreach (var cols in rows)
                {
                    Console.Write(cols + " ");
                }
                Console.WriteLine();
            }

            // 구글 스프레드 시트에 쓰기
            string appendRange = "";
            Google.Apis.Sheets.v4.Data.ValueRange requestBody = new Google.Apis.Sheets.v4.Data.ValueRange();
            SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = _sheetsService.Spreadsheets.Values.Append(requestBody, _spreadSheetsId, appendRange);

            SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum valueInputOption = (SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum)0;  // TODO: Update placeholder value.
            SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum insertDataOption = (SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum)0;  // TODO: Update placeholder value.
            appendRequest.ValueInputOption = valueInputOption;
            appendRequest.InsertDataOption = insertDataOption;

            Google.Apis.Sheets.v4.Data.AppendValuesResponse appendResponse = appendRequest.Execute();

            Console.WriteLine(JsonConvert.SerializeObject(appendResponse));
        }

        private static readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private static readonly string _applicationName = "SMS Report";
        private static readonly string _spreadSheetsId = "1gNQ0bs0xktkD_54_OCC8QuNRjIASIt05ZE08PgOLO2c";
        static SheetsService _sheetsService;
    }
}
