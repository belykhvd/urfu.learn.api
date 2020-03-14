using System;
using System.Collections.Generic;
using Contracts.Types.Common;
using Contracts.Types.User;

namespace Core.Validators
{
    // validate first
    public static class ValidatorExtensions
    {
        public static Result Validate(this Profile profile)
        {
            if (profile.Surname.Length > 256)
                return Result.Fail(OperationStatusCode.ValidationError, "");

            if (profile.FirstName == null || profile.FirstName.Length > 256)
                return Result.Fail(OperationStatusCode.ValidationError, "");

            if (profile.SecondName.Length > 256)
                return Result.Fail(OperationStatusCode.ValidationError, "");

            return Result.Success;
        }
        
        
        // public static OperationStatus Validate(this DbEntity entity)
        // {
        //     
        // }
    }
}