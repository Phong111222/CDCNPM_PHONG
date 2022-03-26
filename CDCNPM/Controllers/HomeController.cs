using CDCNPM.Models;
using CDCNPM.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace CDCNPM.Controllers
{
    [Route("home")]
    public class HomeController:Controller
    {
        private ISqlTableRepository _sqlTableRepository;
        private readonly IConfiguration _configuration;

        public HomeController(ISqlTableRepository sqltableRepository,IConfiguration configuration)
        {
            this._sqlTableRepository = sqltableRepository;
            this._configuration = configuration;
        }

        [Route("test")]
        public JsonResult test()
        {
            return Json(this._sqlTableRepository.getListFKsByTable(_configuration,"CTDDH"));
        }
    }
}
