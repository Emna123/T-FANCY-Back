using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using T_FANCY_Back.Models;
using Microsoft.AspNetCore.Authorization;

namespace T_FANCY_Back.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly TfancyContext _context;
        private readonly UserManager<Client> userManager;

        public ClientController(UserManager<Client> userManager, TfancyContext context)
        {
            _context = context;
            this.userManager = userManager;
        }

        //Get list Client
        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("GetClients")]

        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.client.ToListAsync();

        }

    }
}

        

