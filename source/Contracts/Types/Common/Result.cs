namespace Contracts.Types.Common
{
    public class Result<T>
    {
        public bool IsSuccessful { get; set; }
        public T Value { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}