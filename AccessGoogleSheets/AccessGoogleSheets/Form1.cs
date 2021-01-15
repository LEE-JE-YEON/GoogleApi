using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace AccessGoogleSheets
{
    public partial class Form1 : Form
    {
        #region 전역 변수
        private static readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private static readonly string _applicationName = "SMS Report";
        private static readonly string _spreadSheetsId = "1gNQ0bs0xktkD_54_OCC8QuNRjIASIt05ZE08PgOLO2c";
        private static SheetsService _sheetsService = null;
        #endregion

        public Form1()
        {
            InitializeComponent();

            // google api 접속
            bool result = ConnectGoogleApi();
            if(!result)
            {
                // 프로그램 종료
                Close();
            }
        }

        private static bool ConnectGoogleApi()
        {
            bool connectResult = false;

            try
            {
                UserCredential credential;
                string credentialsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                credentialsPath = Path.Combine(credentialsPath, ".credentials\\sheets.googleapis.com-dotnet-token.json");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets {
                                                                                                ClientId = "780593996999-fq3h61goqgfnevtiit2a11d3ou8kb4q3.apps.googleusercontent.com",
                                                                                                ClientSecret = "DVnoWLARChewH5mY0bsGdC0P"
                },
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credentialsPath, true)).Result;

                //using (var stream = new FileStream("..\\..\\credentials.json", FileMode.Open, FileAccess.Read))
                //{
                //    string credentialsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                //    credentialsPath = Path.Combine(credentialsPath, ".credentials\\sheets.googleapis.com-dotnet-token.json");
                //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                //        _scopes,
                //        "user",
                //        CancellationToken.None,
                //        new FileDataStore(credentialsPath, true)).Result;

                //    Console.WriteLine("Credential file saved to: " + credentialsPath);
                //}

                _sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _applicationName,
                });

                #region 구글 스프레드 시트에서 읽기
                //string readRange = "Sheet1!A1:E5";
                //SpreadsheetsResource.ValuesResource.GetRequest readRequest = _sheetsService.Spreadsheets.Values.Get(_spreadSheetsId, readRange);
                //ValueRange readResponse = readRequest.Execute();
                //IList<IList<object>> values = readResponse.Values;
                //foreach(var rows in values )
                //{
                //    Console.WriteLine();
                //    foreach (var cols in rows)
                //    {
                //        Console.Write(cols + " ");
                //    }
                //    Console.WriteLine();
                //}
                #endregion

                connectResult = true;
            }
            catch (Exception)
            {
                MessageBox.Show("end");
                throw;
            }
            return connectResult;
        }

        private void SendToGoogleSheets(object sender, EventArgs e)
        {
            if(_sheetsService == null)
            {
                // 종료
                MessageBox.Show("Disconnect google sheets");
                Close();
                return;
            }
            try
            {
                // 구글 스프레드 시트에 쓰기
                string appendRange = "Sheet1!A1:ZZ";
                ValueRange appendValueRange = new ValueRange { MajorDimension = "ROWS", Values = new List<IList<object>>() };

                SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = _sheetsService.Spreadsheets.Values.Append(appendValueRange, _spreadSheetsId, appendRange);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                appendValueRange.Values.Clear();

                var dataList = new List<object>();
                appendValueRange.Values.Add(dataList);
                // 수신시간
                dataList.Add(dateTimePicker1.Value.ToShortDateString() + " " + dateTimePicker1.Value.ToShortTimeString());
                // 수신자
                dataList.Add(textBox1.Text);
                // 발신자
                dataList.Add(textBox2.Text);
                // 제목
                dataList.Add(textBox3.Text);
                // 내용
                dataList.Add(textBox4.Text);

                Google.Apis.Sheets.v4.Data.AppendValuesResponse appendResponse = appendRequest.Execute();

                Console.WriteLine(JsonConvert.SerializeObject(appendResponse));

                //MessageBox.Show("Success");
            }
            catch (Exception)
            {

                throw;
            }
            Close();
        }
    }
}
