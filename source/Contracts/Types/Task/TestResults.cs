namespace Contracts.Types.Task
{
    public class TestResults
    {
        public int Passed { get; set; }
        public int All { get; set; }

        public int? FailedNumber { get; set; }
        public string Stacktrace { get; set; }
    }
}