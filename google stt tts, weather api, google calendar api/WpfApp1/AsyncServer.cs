using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static WpfApp1.MyDefine;

namespace WpfApp1
{
    // test
    public static class ExClass
    {
        public static TcpState GetState(this TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
    // test
    class CAsyncServer
    {
        public delegate void SetTextToChatView(string strText); //델리게이트 선언
        public event SetTextToChatView SetTextToChatViewCtrl; //델리게이트 이벤트 선언

        private readonly TcpListener m_Server = null; // 서버
        private TcpClient m_ClientSocket = null; // 소켓
        private static int m_UserCount = 0; // 사용자 수
        private string m_Date; // 날짜
        // 각 클라이언트 마다 리스트에 추가
        private Dictionary<TcpClient, string> m_ClientList = new Dictionary<TcpClient, string>();

        public CAsyncServer()
        {
            m_Server = new TcpListener(IPAddress.Any, MyDefine.PORT); // 서버 접속 IP, 포트
            m_ClientSocket = default(TcpClient); // 소켓 설정
        }
        public void Start()
        {
            Thread thread = new Thread(_initSocket);
            thread.IsBackground = true;
            thread.Start();

            Thread healthCheckThread = new Thread(_healthCheck);
            healthCheckThread.IsBackground = true;
            healthCheckThread.Start();
        }
        private void _healthCheck()
        {
            while(true)
            {
                try
                {
                    Thread.Sleep(1000);
                    foreach (var pair in m_ClientList)
                    {
                        TcpClient client = pair.Key as TcpClient;
                        if(client.Connected)
                        {
                            TcpState state = client.GetState();
                            if(state == TcpState.Unknown)
                            {
                                _onDisconnect(ref client);
                            }
                        }
                        else
                        {
                            _onDisconnect(ref client);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                    continue;
                }
            }
        }
        private void _initSocket()
        {
            m_Server.Start(); // 서버 시작
            SetTextToChatViewCtrl(">> Server Started");
            while (true)
            {
                try
                {
                    m_UserCount++; // Client 수 증가
                    m_ClientSocket = m_Server.AcceptTcpClient(); // client 소켓 접속 허용

                    SetTextToChatViewCtrl(">> Accept connection from client");

                    NetworkStream stream = m_ClientSocket.GetStream();
                    byte[] buffer = new byte[1024]; // 버퍼

                    int bytes = stream.Read(buffer, 0, buffer.Length);

                    //test
                    var dd = Encoding.Default.GetString(buffer, 0, bytes);
                    var dd2 = ((int)JSON_KEY.USER_NAME).ToString();
                    //test

                    JObject json = JObject.Parse(Encoding.Default.GetString(buffer, 0, bytes));
                    var userNm = json[((int)JSON_KEY.USER_NAME).ToString()];
                    if(string.IsNullOrEmpty(userNm.ToString()))
                    {
                        SetTextToChatViewCtrl(">> Disconnection : Empty client name");
                        continue;
                    }
                    m_ClientList.Add(m_ClientSocket, userNm.ToString()); // client 리스트에 추가

                    _sendMsgAllJoinUser(m_ClientSocket, userNm.ToString()); // 모든 client에게 메세지 전송
                }
                catch (SocketException se)
                {
                    Trace.WriteLine(se.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    break;
                }

            }
            m_ClientSocket.Close(); // client 소켓 닫기
            m_Server.Stop(); // 서버 종료
        }
        private void _sendMsgAllJoinUser(TcpClient socket, string userNm)
        {
            handleClient h_client = new handleClient(); // 클라이언트 추가
            h_client.OnReceived += new handleClient.MessageDisplayHandler(_onReceived);
            h_client.OnDisconnected += new handleClient.DisconnectedHandler(_onDisconnect);
            h_client.startClient(socket, m_ClientList);

            m_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 현재 날짜 받기
            string strMsg = "[ " + m_Date + " ] " + userNm + " 님이 입장하셨습니다.";
            _sendMsgAll(strMsg);
            SetTextToChatViewCtrl(strMsg);
        }
        private void _sendMsgAllExitUser(string userNm)
        {
            m_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 현재 날짜 받기
            string strMsg = "[ " + m_Date + " ] " + userNm + " 님이 퇴장하셨습니다.";
            _sendMsgAll(strMsg);
            SetTextToChatViewCtrl(strMsg);
        }
        private void _onReceived(JObject json) // cleint로 부터 받은 데이터
        {
            string strMsg = json[((int)JSON_KEY.MESSAGE).ToString()].ToString();
            bool bIsExit = json[((int)JSON_KEY.CONNECT_STATUS).ToString()].ToString() == "0";
            string strUserNm = json[((int)JSON_KEY.USER_NAME).ToString()].ToString();
            if (bIsExit)
            {
                _sendMsgAllExitUser(strUserNm);
            }
            else
            {
                m_Date = DateTime.Now.ToString("tt HH:mm"); // 현재 날짜 받기
                _sendMsgAll("[ " + m_Date + " ] " + strUserNm + " : " + strMsg); // 모든 Client에게 전송
            }
        }
        private void _sendMsgAll(string message)
        {
            foreach (var pair in m_ClientList)
            {
                m_Date = DateTime.Now.ToString("tt HH:mm"); // 현재 날짜 받기
                TcpClient client = pair.Key as TcpClient;
                NetworkStream stream = client.GetStream();
                byte[] buffer = null;

                buffer = Encoding.Default.GetBytes(message);

                stream.Write(buffer, 0, buffer.Length); // 버퍼 쓰기
                stream.Flush();
            }
        }
        private void _onDisconnect(ref TcpClient clientSocket)
        {
            if (m_ClientList.ContainsKey(clientSocket))
            {
                m_ClientList.Remove(clientSocket);
            }
            clientSocket.Close();
        }
    }
}
