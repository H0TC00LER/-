namespace HttpServer.Attributes
{
    internal class HttpPOST : Attribute
    {
        public string MethodUri;
        public HttpPOST(string methodUri)
        {
            MethodUri = methodUri;
        }
        public HttpPOST() { MethodUri = null; }
    }
}
