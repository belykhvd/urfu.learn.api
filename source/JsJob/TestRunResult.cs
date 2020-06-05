namespace JsJob
{
    public class TestRunResult
    {
        public bool IsSuccess { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }

        public static TestRunResult Fail(string errorMessage) => new TestRunResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}