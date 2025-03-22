using AutoMapper;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class ManagerController : AdminController
    {
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IUnitOfWork _uow;


        public ManagerController(IMapper mapper,
            IReportService reportService,
            IUnitOfWork uow)
        {
            _uow = uow;
            _mapper = mapper;
            _reportService = reportService;
        }

        public IActionResult Dashboard() => View();

    }
}
