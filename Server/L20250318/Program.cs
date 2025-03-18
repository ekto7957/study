using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace L20250318
{
    public class MessageData
    {
        public string Message { get; set; }

    }
    class Program
    {


        static void Main(string[] args)
        {
            // 서버 시작
            Console.WriteLine(" json meassage exchange start ");

            //서버 IP 주소와 포트 설정
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 4000;

            // Generate TCP Listener

            TcpListener listener = new TcpListener(ipAddress, port);

            try 
            {
                // listening 시작
                listener.Start();
                Console.WriteLine($"Server starts at {ipAddress}:{port}");
                Console.WriteLine("클라이언트 연결을 기다리는중....");

                // 클라이언트 접속 무한대기
                while (true)
                {



                    // 클라이언트 연결수락
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("클라이얹트가 연결되었습니다!");


                    // 클라이언트 처리를 위한 스레드 생성
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.Start(client);
                }
                
            
            }

            catch (Exception e)
            {
                Console.WriteLine($"서버오류 {e.Message}");
            }

            finally
            {
                // 리스너 종료
                listener.Stop();
                Console.WriteLine("서버가 중지되었습니다.");
            }
        }

        // 클라이언트 처리 메소드

        static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();

            try
            {
                // 클라이언트 데이터 수신
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"클라이언트로부터 받은 데이터 : {receivedData}");

                // JSON 데이터 파싱
                JObject json = JObject.Parse(receivedData);
                string clientMessage = json.Value<string>("message");

                // 조건 확인 - 클라이언트가 "안녕하세요"를 보냈는지 확인
                if (clientMessage == "안녕하세요")
                {
                    //응답 메세지 생성
                    MessageData responseMessage = new MessageData { Message = "반가워요" };

                    //JSON으로 직렬화
                    string reponseJson = JsonConvert.SerializeObject(responseMessage);
                    Console.WriteLine($"클라이언트에게 노낼 응답 : {responseMessage}");

                    // 클라이언트에게 응답 전송
                    byte[] responseDate = Encoding.UTF8.GetBytes(reponseJson);
                    stream.Write(responseDate, 0, responseDate.Length);

                    Console.WriteLine("응답을 클라이언트에게 전송했습니다");

                }

                else
                {
                    // 다른 메세지에 대한 응답

                    MessageData responseMessage = new MessageData { Message = "메세지를 받았습니다." };
                    string reponseJson = JsonConvert.SerializeObject(responseMessage);
                    byte[] responseData = Encoding.UTF8.GetBytes(reponseJson);
                    stream.Write(responseData, 0, responseData.Length);
                }

                // 잠시 대기 후 연결 종료
                Thread.Sleep(1000);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"클라이언트 처리 오류: {ex.Message}");
            }

            finally
            {
                //연결 종료
                stream.Close();
                client.Close();
                Console.WriteLine("클라이언트 연결이 종료되었습니다");
            }
        }

      
    }
}
