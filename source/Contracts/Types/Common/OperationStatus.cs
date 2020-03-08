using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contracts.Types.Common
{
    public class OperationStatus
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

        public static OperationStatus Success => new OperationStatus {IsSuccess = true};
        public static OperationStatus Fail(OperationStatusCode statusCode, string errorMessage = null)
               => new OperationStatus {IsSuccess = false, StatusCode = statusCode, ErrorMessage = errorMessage};
    }
    
    public class OperationStatus<T> : OperationStatus
    {
        public T Value { get; set; }

        public new IActionResult ActionResult() => IsSuccess ? new OkObjectResult(Value) : ActionResultOnFail();

        public new static OperationStatus<T> Success(T value) => new OperationStatus<T> {IsSuccess = true, Value = value};
        public new static OperationStatus<T> Fail(OperationStatusCode statusCode, string errorMessage = null)
                   => new OperationStatus<T> {IsSuccess = false, StatusCode = statusCode, ErrorMessage = errorMessage};
    }
}