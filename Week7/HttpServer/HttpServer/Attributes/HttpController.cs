namespace HttpServer.Attributes
{
    internal class HttpController : Attribute
    {
        public string ClassUri;
        public HttpController(string classUri)
        {
            ClassUri = classUri;
        }
        public HttpController() { ClassUri = null; }
    }
}
