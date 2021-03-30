using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Xml.Linq;
using WpfApp1.Data;
using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        List<WeatherData> m_weatherData = new List<WeatherData>();
        List<CalendarEvent> m_CalendarEvent = new List<CalendarEvent>();
        public MainWindow()
        {
            InitializeComponent();

            _initWeather();

            _initCalendar();

            CAsyncServer asyncServer = new CAsyncServer();
            asyncServer.SetTextToChatViewCtrl += new CAsyncServer.SetTextToChatView(DisplayText);
            asyncServer.Start();
        }

        private void _initCalendar()
        {
            System.Timers.Timer timer = new System.Timers.Timer() { Interval = MyDefine.INTERVAL_SECOND };
            timer.Elapsed += new ElapsedEventHandler(_calendarTimer);
            timer.Start();

            _calendarTimer(null, null);
        }

        private void _calendarTimer(object sender, ElapsedEventArgs e)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    MyDefine.SCOPES,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = MyDefine.APP_NAME,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 5;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            m_CalendarEvent.Clear();
            Events events = request.Execute();
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    if ((eventItem.Start.DateTime == null && eventItem.Start.Date == null)
                        || string.IsNullOrEmpty(eventItem.Summary))
                    {
                        continue;
                    }
                    CalendarEvent data = new CalendarEvent();
                    DateTime? tempDate = eventItem.Start.DateTime;
                    if (tempDate == null)
                    {
                        tempDate = Convert.ToDateTime(eventItem.Start.Date);
                    }
                    data.Date = tempDate;
                    data.Title = eventItem.Summary;
                    TimeSpan timeDiff = (DateTime)tempDate - DateTime.Now;
                    data.Dday = timeDiff.Days + +1;
                }
            }
        }

        private void _initWeather()
        {
            System.Timers.Timer timer = new System.Timers.Timer() { Interval = MyDefine.INTERVAL_SECOND };
            timer.Elapsed += new ElapsedEventHandler(_weatherTimer);
            timer.Start();

            _weatherTimer(null, null);
        }

        private void _weatherTimer(object sender, ElapsedEventArgs e)
        {
            UriBuilder ub = new UriBuilder(MyDefine.RSS_URL_KWANGMYONG);
            HttpWebRequest request;
            request = HttpWebRequest.Create(ub.Uri) as HttpWebRequest;
            request.BeginGetResponse(new AsyncCallback(GetResponse), request);
        }
        private void GetResponse(IAsyncResult ar)
        {
            HttpWebRequest wr = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse wp = (HttpWebResponse)wr.EndGetResponse(ar);

            Stream stream = wp.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            String strRead = reader.ReadToEnd();

            XElement xmlMain = XElement.Parse(strRead);
            XElement xmlBody = xmlMain.Descendants("body").First();
            m_weatherData.Clear();
            foreach (var elem in xmlBody.Descendants("data"))
            {
                WeatherData data = new WeatherData
                {
                    Hour = Convert.ToInt32(elem.Element("hour").Value),
                    Day = Convert.ToInt32(elem.Element("day").Value),
                    Temp = Convert.ToDouble(elem.Element("temp").Value),
                    Rain = MyDefine.ConvertRainType(elem.Element("pty").Value),
                    WeatherType = MyDefine.ConvertWeatherType(elem.Element("wfEn").Value),
                    RainPercent = Convert.ToDouble(elem.Element("pop").Value),
                    Humidity = Convert.ToInt32(elem.Element("reh").Value)
                };
                m_weatherData.Add(data);
            }
        }

        private bool _getWeatherFromLocation(out WeatherData data, string strLocation)
        {
            bool bRet = false;
            data = null;
            for (int i = 0; i < (int)MyDefine.LocationType.Max; i++)
            {
                UriBuilder ub = new UriBuilder(MyDefine.RSS_URL);
                ub.Query = MyDefine.RSS_URL_PARAM[i];

                HttpWebRequest request;
                request = HttpWebRequest.Create(ub.Uri) as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                String strRead = reader.ReadToEnd();
                XElement xmlMain = XElement.Parse(strRead);
                XElement xmlBody = xmlMain.Descendants("body").First();
                foreach (var elem in xmlBody.Descendants("location"))
                {
                    if (elem.Element("city").Value.Equals(strLocation))
                    {
                        data = new WeatherData();
                        XElement xmlData = elem.Elements("data").First();

                        var dd = xmlData.Element("wf").Value;
                    }
                }
            }
            return bRet;
        }

        private void STT_Button_Click(object sender, RoutedEventArgs e)
        {
            var speech = SpeechClient.Create();

            var audio = RecognitionAudio.FromFile("D:\\BlueSky\\Client\\BlueSky_Ment\\sys_wait_5902cf1726650.wav");

            var response = speech.Recognize(new RecognitionConfig()
            {
                //Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                //SampleRateHertz = 8000,
                LanguageCode = "ko-KR",
            }, audio);
            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    Console.WriteLine(alternative.Transcript);
                }
            }
        }

        private void TTS_Button_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate a client
            TextToSpeechClient client = TextToSpeechClient.Create();

            // Set the text input to be synthesized.
            SynthesisInput input = new SynthesisInput
            {
                Text = "N Pay구매하신 상품의구매확정처리부탁드립니다.상품을 받으신 후 만족하셨다면 구매확정을 부탁드립니다."
                        + "아래 기한까지 구매확정을 하지 않으실 경우,이후 자동으로 구매가 확정될 예정입니다."
                        + "만일,구매확정기한 내 정상적으로 상품을 수령하지 못하신 경우에는 판매자문의 또는 구매확정 연장을 해주세요."
                        + "고객명 이 * 연님주문번호    2019100971174081주문일자    2019.10.09 23:13발송일자    2019.10.10자동구매확정일 2019.10.19"
                        + "결제정보총 주문금액  12,100원할인금액    0원환불정산액 / 포인트   0원 / 2,394원결제수단    신용카드"
                        + "최종결제금액  9,706원배송정보수령인 이*연연락처 010 - 5234 - ****배송지 14305경기도 광명시 금당로 11(하안동, 하안6단지고층주공아파트)"
                        + "603동****배송메모발송상품상품이미지애플 인증 고속충전 정품 1.2m 2m 아이패드 아이폰 케이블"
                        + "옵션 : 옵션선택: mfi인증 메탈릭1.2m_다크그레이주문금액    9,600원수량  1"
            };

            // Build the voice request, select the language code ("en-US"),
            // and the SSML voice gender ("neutral").
            VoiceSelectionParams voice = new VoiceSelectionParams
            {
                LanguageCode = "ko-KR",
                SsmlGender = SsmlVoiceGender.Neutral
            };

            // Select the type of audio file you want returned.
            AudioConfig config = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the Text-to-Speech request, passing the text input
            // with the selected voice parameters and audio file type
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = voice,
                AudioConfig = config
            });

            // Write the binary AudioContent of the response to an MP3 file.
            using (Stream output = File.Create("C:\\Users\\이제연\\Desktop\\sample.mp3"))
            {
                response.AudioContent.WriteTo(output);
            }
            mciSendString("open \"" + "C:\\Users\\이제연\\Desktop\\sample.mp3" + "\" type mpegvideo alias MediaFile", null, 0, IntPtr.Zero);

            StringBuilder returnData = new StringBuilder(128);
            mciSendString("status MediaFile length", returnData, returnData.Capacity, IntPtr.Zero);
            int nMilliSecond = Convert.ToInt32(returnData.ToString());

            mciSendString("play MediaFile", null, 0, IntPtr.Zero);

            Thread thread = new Thread(() => _deleteMp3File("C:\\Users\\이제연\\Desktop\\sample.mp3", nMilliSecond));
            thread.Start();
        }
        private static void _deleteMp3File(string strFilePath, int nTimer)
        {
            nTimer += 2000;
            Thread.Sleep(nTimer);
            FileInfo fileDel = new FileInfo(strFilePath);
            fileDel.Delete();
        }
        private void DisplayText(string strText)
        {
            if (chatViewCtrl.Dispatcher.CheckAccess())
            {
                chatViewCtrl.AppendText(strText + Environment.NewLine);
                chatViewCtrl.ScrollToEnd();
            }
            else
            {
                chatViewCtrl.Dispatcher.BeginInvoke(new Action(delegate
                {
                    chatViewCtrl.AppendText(strText + Environment.NewLine);
                    chatViewCtrl.ScrollToEnd();
                }));
            }
        }
    }
}
