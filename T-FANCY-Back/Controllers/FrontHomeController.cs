using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T_FANCY_Back.Models;

namespace T_FANCY_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrontHomeController : ControllerBase
    {
        private readonly TfancyContext _context;
        private static IWebHostEnvironment _webHostEnvironment;

        public FrontHomeController(TfancyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: FrontHome by id
        [Authorize(Roles = "Manager,Client")]
        [HttpGet]
        [Route("GetFrontHome/{id}")]
        public async Task<FrontHome> GetFront(int id)
        {
            return await _context.frontHome.FirstOrDefaultAsync(x => x.id == id);
        }


        // POST: Create FrontHome
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("Create")]
        public async Task<string> Create([FromForm] UploadFrontHomeFiles obj)
        {
            if (obj != null)
            {

                try
                {
                    if (Directory.Exists(_webHostEnvironment.WebRootPath + "\\Images\\"))
                        Directory.CreateDirectory(_webHostEnvironment.WebRootPath + "\\Images\\");
                    using (FileStream filestream1 = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Images\\" + obj.femmeCollection_image.FileName))
                    {
                        obj.femmeCollection_image.CopyTo(filestream1);
                        filestream1.Flush();

                    }
                    using (FileStream filestream2 = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Images\\" + obj.hommeCollection_image.FileName))

                    {
                        obj.hommeCollection_image.CopyTo(filestream2);
                        filestream2.Flush();

                    }
                      FrontHome frontHome = new FrontHome
                      {
                        animated_text = obj.animated_text,
                        historical_text=obj.historical_text,
                        femmeCollection_image=obj.femmeCollection_image.FileName,
                        hommeCollection_image=obj.hommeCollection_image.FileName,
                    };
                   await _context.AddAsync(frontHome);
                   await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

                return "Images uploaded successfully";

            }
            else
            {
                return "upload Failed";
            }
        }


        // update FrontHome
        [Authorize(Roles = "Manager")]
        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UploadFrontHomeFiles obj)
        {
            var front = await _context.frontHome.FindAsync(id);
            if (front != null)
            {

                try
                {
                    if (obj.femmeCollection_image != null)

                    {
                        Prod_imageController.DeleteImage(front.femmeCollection_image);

                        if (Directory.Exists(_webHostEnvironment.WebRootPath + "\\Images\\"))
                            Directory.CreateDirectory(_webHostEnvironment.WebRootPath + "\\Images\\");
                        using (FileStream filestream = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Images\\" + obj.femmeCollection_image.FileName))
                        {
                            obj.femmeCollection_image.CopyTo(filestream);
                            filestream.Flush();
                        }
                        front.femmeCollection_image = obj.femmeCollection_image.FileName;
                    }
                    if (obj.hommeCollection_image != null)

                    {
                        Prod_imageController.DeleteImage(front.femmeCollection_image);

                        if (Directory.Exists(_webHostEnvironment.WebRootPath + "\\Images\\"))
                            Directory.CreateDirectory(_webHostEnvironment.WebRootPath + "\\Images\\");
                        using (FileStream filestream = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Images\\" + obj.hommeCollection_image.FileName))
                        {
                            obj.hommeCollection_image.CopyTo(filestream);
                            filestream.Flush();
                        }
                        front.hommeCollection_image= obj.hommeCollection_image.FileName;
                    }
                    front.historical_text = obj.historical_text;
                    front.animated_text = obj.animated_text;

                    _context.Entry(front).State = EntityState.Modified;
                    _context.SaveChanges();
                    return Ok(new
                    {
                        updatefront = front
                    });

                }
                catch (Exception ex)
                {
                    return (IActionResult)ex;
                }


            }
            else
                return NotFound();

        }
     
    }
}
