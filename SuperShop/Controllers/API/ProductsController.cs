using Microsoft.AspNetCore.Mvc;
using SuperShop.Data;
using SuperShop.Data.Entities;
using System;
using System.Linq;

namespace SuperShop.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }


        [HttpGet]
        public IActionResult GetProducts()
        {
            return Ok(_productRepository.GetAll());
        }
    }
}
