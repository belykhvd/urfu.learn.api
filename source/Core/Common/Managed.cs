using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contracts.Types.Common;

namespace Core.Common
{
    public static class Managed
    {
        public static async Task<Result> Try(Task<Result> task, string context)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                var status = await task.ConfigureAwait(false);

                sw.Stop();
                Console.WriteLine($"{context} {sw.Elapsed}");
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // logging
                return Result.Fail(OperationStatusCode.InternalServerError);
            }
        }

        // protected async Task<OperationStatus<T>> Try<T>(Task<OperationStatus<T>> task, string context)
        // {
        //     try
        //     {
        //         var sw = Stopwatch.StartNew();
        //
        //         var status = await task.ConfigureAwait(false);
        //
        //         sw.Stop();
        //         Console.WriteLine($"{context} {sw.Elapsed}");
        //         return status;
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);  // logging
        //         return OperationStatus<T>.Fail(OperationStatusCode.InternalServerError);
        //     }
        // }
    }
}