using System;
using System.Net;
using System.IO;

namespace HttpServer
{
    class HttpServer
    {
        private readonly string _url;
        private readonly HttpListener _listener;
        private HttpListenerContext _httpContext;
        private string _path = "./index.html";

        public HttpServer(string url)
        {
            _url = url;
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
        }

        public async void Start()
        {
            Console.WriteLine("Запуск сервера...");
            _listener.Start();
            Console.WriteLine("Сервер запущен.");

            await Listen();
        }

        public void Stop()
        {
            Console.WriteLine("Остановка сервера...");
            _listener.Stop();
            Console.WriteLine("Сервер остановлен.");
        }

        private async Task Listen()
        {
            while (true)
            {
                HttpListenerContext _httpContext;
                try
                {
                    _httpContext = await _listener.GetContextAsync();
                }
                catch
                {
                    //Тут при stop выбрасывалось исключение, так что пришлось делать вот так.
                    return;
                }
                HttpListenerRequest request = _httpContext.Request;
                HttpListenerResponse response = _httpContext.Response;

                string responseString;
                if (!File.Exists(_path))
                {
                    Console.WriteLine("Файл не найден!");
                    return;
                }
                responseString = File.ReadAllText(_path);

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }
    }
}
