namespace HttpServer.Attributes
{
    internal class HttpGET : Attribute
    {
        public string MethodUri;
        public HttpGET(string methodUri)
        {
            MethodUri = methodUri;
        }
        public HttpGET() { MethodUri = null; }
    }
}
