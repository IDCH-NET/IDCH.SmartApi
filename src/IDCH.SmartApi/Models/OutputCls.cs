namespace Models
{
    public class OutputCls
    {
        public bool IsSucceed { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public List<string> FileUrls { get; set; } = new();
    }
}
