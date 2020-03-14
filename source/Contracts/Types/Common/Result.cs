using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contracts.Types.Common
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public OperationStatusCode StatusCode { get; set; }

        public IActionResult ActionResult() => IsSuccess ? new OkResult() : ActionResultOnFail();

        protected IActionResult ActionResultOnFail()
        {
            return StatusCode switch
            {
                OperationStatusCode.NotFound => (IActionResult) new NotFoundResult(),
                OperationStatusCode.ValidationError => new BadRequestObjectResult(ErrorMessage),
                OperationStatusCode.Conflict => new ConflictObjectResult(ErrorMessage),
                OperationStatusCode.InternalServerError => new StatusCodeResult(StatusCodes.Status500InternalServerError),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
        }

        public static Result Success => new Result {IsSuccess = true};
        public static Result Fail(OperationStatusCode statusCode, string errorMessage = null)
               => new Result {IsSuccess = false, StatusCode = statusCode, ErrorMessage = errorMessage};
    }
    
    public class Result<T> : Result
    {
        public T Value { get; set; }

        public new IActionResult ActionResult() => IsSuccess ? new OkObjectResult(Value) : ActionResultOnFail();

        public new static Result<T> Success(T value) => new Result<T> {IsSuccess = true, Value = value};
        public new static Result<T> Fail(OperationStatusCode statusCode, string errorMessage = null)
                   => new Result<T> {IsSuccess = false, StatusCode = statusCode, ErrorMessage = errorMessage};
    }
}