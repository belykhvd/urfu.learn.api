using System;
using Jint;
using Jint.Parser;
using Jint.Runtime;

namespace JsJob
{
    public class JsRunner
    {
        public TestRunResult Execute(string source, object input = null)
        {
            try
            {
                var engine = CreateEngine();

                var output = engine
                    .SetValue("input", input)
                    .Execute(source)
                    .GetCompletionValue()
                    .ToObject();

                return new TestRunResult
                {
                    IsSuccess = true,
                    Output = output.ToString()
                };
            }
            catch (ParserException p)
            {
                return TestRunResult.Fail($"Compilation error: {p.Message}");
            }
            catch (JavaScriptException js)
            {
                return TestRunResult.Fail($"Runtime error: {js.Message}");
            }
            catch (TimeoutException)
            {
                return TestRunResult.Fail("Time limit exceeded");
            }
            catch (RecursionDepthOverflowException)
            {
                return TestRunResult.Fail("Recursion limit exceeded");
            }
            catch (StatementsCountOverflowException)
            {
                return TestRunResult.Fail("Statements limit exceeded");
            }
        }

        private static Engine CreateEngine()
        {
            return new Engine(options =>
            {
                options.DiscardGlobal();
                options.LimitRecursion(20);
                options.TimeoutInterval(TimeSpan.FromSeconds(10));
                options.MaxStatements(200);
            });
        }
    }
}