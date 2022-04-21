using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using T_FANCY_Back.Models;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace T_FANCY_Back.Controllers
{
    [Authorize(Roles = "Manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class Prod_imageController : ControllerBase
    {
        private readonly TfancyContext _context;
        private static IWebHostEnvironment _webHostEnvironment;

        public Prod_imageController(TfancyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

        }
        //Upload product images
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("upload")]
        public async Task<string> Upload([FromForm] UploadFiles obj)
        {
            if (obj.files != null && obj.files.Count > 0)
            {
                foreach (IFormFile file in obj.files)
                {
                    try
                    {
                        if (Directory.Exists(_webHostEnvironment.WebRootPath + "\\Images\\"))
                            Directory.CreateDirectory(_webHostEnvironment.WebRootPath + "\\Images\\");
                        using (FileStream filestream = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Images\\" + file.FileName))
                        {
                            file.CopyTo(filestream);
                            filestream.Flush();
                        }
                        var product = _context.product.FirstOrDefault(x => x.id == obj.idProduct);
                        Prod_image prod_image = new Prod_image
                        {
                            product = product,
                            name = file.FileName
                        };
                       await _context.AddAsync(prod_image);
                       await _context.SaveChangesAsync();
                        product.prod_image.Add(prod_image);
                        _context.Entry(product).State = EntityState.Modified;
                        _context.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }

                return "Image uploaded successfully";

            }
            else
            {
                return "upload Failed";
            }
        }




        // delete image from Images folder
        public static void DeleteImage(string name)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwroot\\Images\\", name);
            if (System.IO.File.Exists(path))
            {
                // If file found, delete it    
                System.IO.File.Delete(path);
                Console.WriteLine("File deleted.");
            }


        }

        //Delete Product images
        [Authorize(Roles = "Manager")]
        [HttpDelete]
        [Route("DeleteProd_image/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.product.FindAsync(id);
            if (product != null)
            {
                var prodImg = await _context.prod_Image.Where(x => x.product == product).ToListAsync();
                foreach (Prod_image image in prodImg)
                {
                   DeleteImage(image.name);
                    _context.Remove(image);
                    _context.SaveChanges();
                }
                return Ok(new
                {
                    msg = "Images succefully deleted !"
                });
            }
         
            return NotFound();
        }
    }

}
