using FluentAssertions;
using NUnit.Framework;

namespace JsJob
{
    [TestFixture]
    public class JsRunnerTests
    {
        private readonly JsRunner runner = new JsRunner();        

        [Test]
        public void Return1Plus1()
        {
            runner.Execute("1 + 1").Should().Be(2);
        }

        [Test]
        public void TestLoop()
        {
            runner.Execute(
                @"var x = 0;
                  for (var i = 0; i < 2; i++) {
                      x += 2;
                  }
                  x"
            ).Should().Be(4);
        }

        [Test]
        public void TestConditions()
        {
            runner.Execute(
                @"var x = 0;
                  if (x === 0) 5;
                  else 10;"
            ).Should().Be(5);
        }

        [Test]
        public void TestInput()
        {
            runner.Execute("input", 42).Should().Be(42);
        }

        [Test]
        public void TestJsException()
        {
            runner.Execute("throw 'Unexpected'").Should().Be("Runtime error: Unexpected");
        }

        [Test]
        public void TestParserException()
        {
            runner.Execute("0df = 1").Should().Be("Compilation error: Line 1: Unexpected token ILLEGAL");
        }

        [Test]
        public void TestTimeLimitExceeded()
        {
            runner.Execute("while (true) {}").Should().Be("Time limit exceeded");
        }

        //[Test]
        public void TestMemoryLimitExceeded()
        {
            runner.Execute("var a = new Array(1000000)").Should().Be("Memory limit exceeded");
        }

        [Test]
        public void TestRecursionLimitExceeded()
        {
            runner.Execute(
                @"function r() { r(); }
                  r();"
            ).Should().Be("Recursion limit exceeded");
        }
    }
}