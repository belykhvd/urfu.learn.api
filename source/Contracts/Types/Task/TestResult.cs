namespace Contracts.Types.Task
{
    public class TestResult
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public bool IsPassed { get; set; }
        public string ErrorMessage { get; set; }
    }
}