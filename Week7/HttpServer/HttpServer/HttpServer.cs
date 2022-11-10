using System;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using HttpServer.Attributes;
using System.Reflection;

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

            //var a = "http://localhost:" + serverSettings.Port + "/";
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

                Console.WriteLine(request.RawUrl);
                Console.WriteLine(request.HttpMethod);

                byte[] buffer;
                if (Directory.Exists(serverSettings.Path))
                {
                    buffer = GetFileAndSetHeader(request, response);

                    if (buffer == null)
                    {
                        if (MethodHandler(_httpContext))
                            continue;

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

                WriteIntoOutput(response, buffer);
            }
        }

        private byte[] GetFileAndSetHeader(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] buffer = null;
            var rawUrl = request.RawUrl;
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

        private bool MethodHandler(HttpListenerContext _httpContext)
        {
            // объект запроса
            HttpListenerRequest request = _httpContext.Request;

            // объект ответа
            HttpListenerResponse response = _httpContext.Response;

            Console.WriteLine(request.RawUrl);
            Console.WriteLine(request.HttpMethod);

            if (request.Url.Segments.Length < 2) return false;

            string controllerName = request.Url.Segments[1].Replace("/", "");

            string[] strParams = request.Url
                .Segments
                .Skip(2)
                .Select(s => s.Replace("/", ""))
                .ToArray();

            var assembly = Assembly.GetExecutingAssembly();

            var controller = assembly
                .GetTypes()
                .Where(t => Attribute.IsDefined(t, typeof(HttpController)))
                .FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());

            if (controller == null) return false;
            Console.WriteLine("control success");

            var methods = controller
                .GetMethods()
                .Where(t => t.GetCustomAttributes(true).Any(attr => attr.GetType().Name == $"Http{request.HttpMethod}"))
                .ToArray();

            MethodInfo method = null;
            if (request.HttpMethod == "GET")
            {
                if (strParams.Length == 0)
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == 0);
                else
                    method = methods.FirstOrDefault(m => m.GetParameters().Length > 0);

                object[] queryParams = method
                    .GetParameters()
                    .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                    .ToArray();
                var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);
                response.ContentType = "Application/json";

                byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));

                WriteIntoOutput(response, buffer);

                return true;
            }
            else
            {
                var requestBody = ReadRequestBody(request.InputStream);
                if (requestBody.Length == 0)
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == 0);
                else
                    method = methods.FirstOrDefault(m => m.GetParameters().Length > 0);
                var ret = method.Invoke(Activator.CreateInstance(controller), ParsePostBody(requestBody));
                response.ContentType = "Application/json";

                byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));

                WriteIntoOutput(response, buffer);

                return true;
            }
        }

        private void WriteIntoOutput(HttpListenerResponse response, byte[] buffer)
        {
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private string ReadRequestBody(Stream inputStream)
        {
            string documentContents;
            using (Stream receiveStream = inputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }

        private string[] ParsePostBody(string body)
        {
            var result = new List<string>();
            for (int i = 0; i < body.Length; i++)
            {
                var arg = new StringBuilder();
                if (body[i] == '=')
                {
                    while (i != body.Length && body[i] != '&')
                    {
                        arg.Append(body[i]);
                        i++;
                    }
                    result.Add(arg.ToString().Substring(1));
                }   
            }
            return result.ToArray();
        }
    }
}