using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using T_FANCY_Back.Models;

namespace T_FANCY_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly TfancyContext _context;
        private static IWebHostEnvironment _webHostEnvironment;

        public BrandController(TfancyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        //Get Brand By Id
        [Authorize(Roles = "Manager,Client")]
        [HttpGet]
        [Route("GetBrand/{id}")]
        public async Task<Brand> GetBrand(int id)
        {
            return await _context.brand.FirstOrDefaultAsync(x => x.id == id);
        }

        // Add new Brand 
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("add-brand")]
        public async Task<string> AddBrand([FromForm] UploadFile obj)
        {
            if (obj.file != null )
            {
                    try
                    {
                        if (Directory.Exists(_webHostEnvironment.WebRootPath + "\\Images\\"))
                            Directory.CreateDirectory(_webHostEnvironment.WebRootPath + "\\Images\\");
                        using (FileStream filestream = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Images\\" + obj.file.FileName))
                        {
                            obj.file.CopyTo(filestream);
                            filestream.Flush();
                        }
                        Brand brand = new Brand
                        {
                            brand_name = obj.name,
                            brand_image = obj.file.FileName
                        };
                        await _context.AddAsync(brand);
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

        //Get list brand
        [Authorize(Roles = "Manager,Client")]
        [HttpGet]
        [Route("GetBrands")]

        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            return await _context.brand.ToListAsync();

        }

        //Update Brand in the list
        [Authorize(Roles = "Manager")]
        [HttpPut]
        [Route("UpdateBrandList/{id}")]
        public async Task<IActionResult> UpdateBrandList(int id, Brand brd)
        {
            var brand = await _context.brand.FindAsync(id);
            if (brand != null)
            {
                brand.brand_name = brd.brand_name;

                _context.Entry(brand).State = EntityState.Modified;
                _context.SaveChanges();
                return Ok(new
                {
                    updatebrand = brand
                });
            }
            return NotFound();
        }

        //Update brand
        [Authorize(Roles = "Manager")]
        [HttpPut]
        [Route("UpdateBrand/{id}")]
        public async Task<IActionResult> UpdateBrand(int id, [FromForm] UploadFile obj)
        {
            var brand = await _context.brand.FindAsync(id);
            if (brand!=null)
            {
         
                    try{
                        if (obj.file != null)

                        {
                            Prod_imageController.DeleteImage(brand.brand_image);

                            if (Directory.Exists(_webHostEnvironment.WebRootPath + "\\Images\\"))
                                Directory.CreateDirectory(_webHostEnvironment.WebRootPath + "\\Images\\");
                            using (FileStream filestream = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Images\\" + obj.file.FileName))
                            {
                                obj.file.CopyTo(filestream);
                                filestream.Flush();
                            }
                        brand.brand_image = obj.file.FileName;
                        }
                    brand.brand_name = obj.name;

                    _context.Entry(brand).State = EntityState.Modified;
                    _context.SaveChanges();
                    return Ok(new
                    {
                        updatebrand = brand
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

        //Delete brand
        [Authorize(Roles = "Manager")]
        [HttpDelete]
        [Route("DeleteBrand/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var brand = await _context.brand.FindAsync(id);
            var products = await _context.product.Where(x => x.brandid == id).ToListAsync();

            if (brand != null)
            {
                foreach (Product product in products)
                {
                    try
                    {
                        var prodImg = await _context.prod_Image.Where(x => x.product == product).ToListAsync();
                    foreach (Prod_image image in prodImg)
                    {
                        Prod_imageController.DeleteImage(image.name);
                        _context.Remove(image);
                        _context.SaveChanges();
                    }
                    _context.Remove(product);
                    _context.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        return (IActionResult)ex;
                    }
                }
               Prod_imageController.DeleteImage(brand.brand_name); 
                _context.Remove(brand);
                _context.SaveChanges();
                return Ok(new
                {
                    msg = "product succefully deleted !"
                });
            }
            return NotFound();
        }


    }

}

