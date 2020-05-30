using System;
using Jint;
using Jint.Parser;
using Jint.Runtime;

namespace JsJob
{
    public class JsRunner
    {
        public object Execute(string source, object input = null)
        {
            try
            {
                var engine = new Engine(options =>
                {
                    options.DiscardGlobal();
                    options.LimitRecursion(20);
                    options.TimeoutInterval(TimeSpan.FromSeconds(10));
                    options.MaxStatements(200);
                });

                return engine
                    .SetValue("input", input)
                    .Execute(source)
                    .GetCompletionValue()
                    .ToObject();
            }
            catch (ParserException p)
            {
                return $"Compilation error: {p.Message}";
            }
            catch (JavaScriptException js)
            {
                return $"Runtime error: {js.Message}";
            }
            catch (TimeoutException)
            {
                return "Time limit exceeded";
            }
            catch (RecursionDepthOverflowException)
            {
                return "Recursion limit exceeded";
            }
            catch (StatementsCountOverflowException)
            {
                return "Statements limit exceeded";
            }
        }
    }
}