using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using T_FANCY_Back.Models;
namespace T_FANCY_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly TfancyContext _context;
        public ProductController(TfancyContext context)
        {
            _context = context;

        }
        //Get Products
        [Authorize(Roles = "Manager,Client")]
        [HttpGet]
        [Route("GetProducts")]

        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products= await _context.product.ToListAsync();

            foreach(Product product in products)
            { 
            if (DateTime.Now > product.exp_date && product.exp_date != null)
            {
                product.price = product.old_price;
                   _context.Entry(product).State = EntityState.Modified;
                   _context.SaveChanges();
                }
            }
            return products;

        }

        //Get Product By Id
        [Authorize(Roles = "Manager,Client")]
        [HttpGet]
        [Route("GetProduct/{id}")]
        public async Task<Product> GetProduct(int id)
        {
            var product= await _context.product.FirstOrDefaultAsync(x => x.id == id);
           if(DateTime.Now> product.exp_date &&product.exp_date!=null)
            {
                product.price=product.old_price;
            }
            _context.Entry(product).State = EntityState.Modified;
            _context.SaveChanges();
            return product;
        }

        //Add new Product
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (product != null)
            {
               await _context.AddAsync(product);
               await _context.SaveChangesAsync();
                return Ok(new
                {
                    idProduct = product.id
                });
            }
            return  StatusCode(StatusCodes.Status200OK, "Product succefully added !");
        }


        //Delete Product
        [Authorize(Roles = "Manager")]
        [HttpDelete]
        [Route("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.product.FindAsync(id);
            if (product != null)
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
                return Ok(new
                {
                    msg = "Brand succefully deleted !"
                });
            }
            return NotFound();
        }

        //Update Product in the list
        [Authorize(Roles = "Manager")]
        [HttpPut]
        [Route("UpdateProductList/{id}")]
        public async Task<IActionResult> UpdateProductList(int id, Product prod)
        {
            var product = await _context.product.FindAsync(id);
            if (product != null)
            {
                product.productName = prod.productName;
                product.price = prod.price;
                product.old_price = prod.old_price;
                product.quantity = prod.quantity;
                product.qnt_sold = prod.qnt_sold;
                product.new_prod = prod.new_prod;
                product.best_prod = prod.best_prod;
              
                
          
                return Ok(new
                {
                    updateproduct = product
                });
            }
            return NotFound();
        }

        //Update Product 
        [Authorize(Roles = "Manager")]
        [HttpPut]
        [Route("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product prod)
        {
            var product = await _context.product.FindAsync(id);
            if (product != null)
            {
                product.productName = prod.productName;
                product.price = prod.price;
                product.old_price = prod.old_price;
                product.quantity = prod.quantity;
                product.qnt_sold = prod.qnt_sold;
                product.new_prod = prod.new_prod;
                product.best_prod = prod.best_prod;
                product.category = prod.category;
                product.size = prod.size;
                product.description = prod.description;
                product.Bestsell = prod.Bestsell;
                product.gender = prod.gender;
                product.exp_date = prod.exp_date;
                product.brandid = prod.brandid;


                _context.Entry(product).State = EntityState.Modified;
                _context.SaveChanges();
                return Ok(new
                {
                    updateproduct = product
                });
            }
            return NotFound();
        }

    }
}
