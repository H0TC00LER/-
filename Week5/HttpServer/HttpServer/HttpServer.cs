using System;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace HttpServer
{
    class HttpServer
    {
        private readonly HttpListener _httpListener;
        public string SettingsPath;

        private ServerSettings serverSettings;

        public ServerStatus ServerStatus { get; private set; } = ServerStatus.Stopped;

        public HttpServer(string settingsPath)
        {
            SettingsPath = settingsPath;
            _httpListener = new HttpListener();
        }

        public async void Start()
        {
            if (ServerStatus == ServerStatus.Started)
            {
                Console.WriteLine("Сервер уже запущен!");
                return;
            }

            if (!File.Exists(SettingsPath))
            {
                Console.WriteLine("Файл настроек не найден!");
                return;
            }
            serverSettings = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes(SettingsPath));

            var a = "http://localhost:" + serverSettings.Port + "/";
            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add("http://localhost:" + serverSettings.Port + "/");

            Console.WriteLine("Запуск сервера...");
            _httpListener.Start();
            ServerStatus = ServerStatus.Started;
            Console.WriteLine("Сервер запущен.");

            await Listen();
        }

        public void Stop()
        {
            if (ServerStatus == ServerStatus.Stopped)
                return;

            Console.WriteLine("Остановка сервера...");
            _httpListener.Stop();
            ServerStatus = ServerStatus.Stopped;
            Console.WriteLine("Сервер остановлен.");
        }

        private async Task Listen()
        {
            while(true)
            {
                HttpListenerContext _httpContext;
                try
                {
                    _httpContext = await _httpListener.GetContextAsync();
                }
                catch
                {
                    return;
                }
                HttpListenerRequest request = _httpContext.Request;
                HttpListenerResponse response = _httpContext.Response;

                byte[] buffer;
                if (Directory.Exists(serverSettings.Path))
                {
                    buffer = GetFileAndSetHeader(request.RawUrl, response);

                    if (buffer == null)
                    {
                        response.Headers.Set("Content-type", "text/plain");

                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        string error = "error 404 - not found";

                        buffer = Encoding.UTF8.GetBytes(error);
                    }
                }
                else
                {
                    var errorMessage = $"Directory {serverSettings.Path} is not found";
                    buffer = Encoding.UTF8.GetBytes(errorMessage);
                }

                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        private byte[] GetFileAndSetHeader(string rawUrl, HttpListenerResponse response)
        {
            byte[] buffer = null;
            var filePath = serverSettings.Path + rawUrl;
            if (Directory.Exists(filePath))
            {
                filePath += "/index.html";
                response.Headers.Set("Content-type", "text/html");
                response.StatusCode = (int)HttpStatusCode.OK;
                if (File.Exists(filePath))
                    buffer = File.ReadAllBytes(filePath);
            }
            else if (File.Exists(filePath))
            {
                var extencion = rawUrl.Substring(rawUrl.IndexOf('.') + 1);
                switch(extencion)
                {
                    case "png":
                        response.Headers.Set("Content-type", "image/png");
                        break;
                    case "gif":
                        response.Headers.Set("Content-type", "image/gif");
                        break;
                    case "css":
                        response.Headers.Set("Content-type", "text/css");
                        break;
                }
                response.StatusCode = (int)HttpStatusCode.OK;
                buffer = File.ReadAllBytes(filePath);
            }

            return buffer;
        }
    }
}