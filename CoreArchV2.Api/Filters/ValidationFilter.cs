using CoreArchV2.Dto.EApiDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CoreArchV2.Api.Filters
{
    public class ValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)//isrequired boşsa
            {
                AErrorDto errorDto = new AErrorDto();
                errorDto.Status = 400;
                IEnumerable<ModelError> modelError = context.ModelState.Values.SelectMany(v => v.Errors);
                modelError.ToList().ForEach(x =>
                {
                    errorDto.Errors.Add(x.ErrorMessage);
                });
                context.Result = new BadRequestObjectResult(errorDto);
            }
        }
    }
}
