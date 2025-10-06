using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.EApiDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreArchV2.Api.Filters
{
    public class NotFoundFilter : ActionFilterAttribute
    {
        private readonly IGenericRepository<User> _userGenericRepository;
        private readonly IUnitOfWork _uow;

        public NotFoundFilter(IUnitOfWork uow)
        {
            _uow = uow;
            _userGenericRepository = uow.GetRepository<User>();
        }

        public async override Task OnActionExecutionAsync(ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            int id = (int)context.ActionArguments.Values.FirstOrDefault();
            var user = await _userGenericRepository.FindAsync(id);
            if (user != null)
                await next();
            else
            {
                AErrorDto errorDto = new AErrorDto();
                errorDto.Status = 404;
                errorDto.Errors.Add($"{id} id değeri veritabanında bulunamadı");
                context.Result = new BadRequestObjectResult(errorDto);
            }
        }
    }
}
