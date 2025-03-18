using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{
    // 메시지 클래스 정의 (서버와 동일한 구조)
    public class MessageData
    {
        public string Message { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===== JSON 메시지 교환 클라이언트 시작 =====");

            try
            {
                // 서버에 연결
                Console.WriteLine("서버에 연결 중...");
                TcpClient client = new TcpClient("127.0.0.1", 4000);
                Console.WriteLine("서버에 연결되었습니다!");

                // 서버와의 통신을 위한 네트워크 스트림 가져오기
                NetworkStream stream = client.GetStream();

                // 메시지 객체 생성
                MessageData message = new MessageData { Message = "안녕하세요" };

                // JSON으로 직렬화
                string jsonData = JsonConvert.SerializeObject(message);
                Console.WriteLine($"서버로 전송할 JSON 데이터: {jsonData}");

                // 서버로 데이터 전송
                byte[] data = Encoding.UTF8.GetBytes(jsonData);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("데이터를 서버로 전송했습니다.");

                // 서버로부터 응답 수신
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"서버로부터 받은 응답: {responseData}");

                // 응답 데이터 파싱
                MessageData serverResponse = JsonConvert.DeserializeObject<MessageData>(responseData);
                Console.WriteLine($"서버 메시지: {serverResponse.Message}");

                // 연결 종료
                stream.Close();
                client.Close();
                Console.WriteLine("서버와의 연결이 종료되었습니다.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"클라이언트 오류: {e.Message}");
            }

            Console.WriteLine("종료하려면 아무 키나 누르세요...");
            Console.ReadKey();
        }
    }
}
