using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.GeometriesGraph;
using SuperShop.Data;
using SuperShop.Data.Entities;
using SuperShop.Helpers;
using SuperShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace SuperShop.Controllers
{
   
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;

        private readonly IConverterHelper _converterHelper;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(
            IProductRepository productRepository,
            IUserHelper userHelper,
            IBlobHelper blobHelper,
            IConverterHelper converterHelper,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public IActionResult Index()
        {
            return View(_productRepository.GetAll().OrderBy(p => p.Name));
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product =  await _productRepository.GetByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;


                if(model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageId = Guid.NewGuid();

                    // 1. Definir o caminho físico onde a imagem será guardada (wwwroot/images/products)
                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", $"{imageId}.jpg");

                    // 2. Guardar a imagem no disco do Somee
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                }
                

                var product = _converterHelper.ToProduct(model, imageId, true);

               
                product.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); 
                await _productRepository.CreateAsync(product);
                
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Products/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            var model = _converterHelper.ToProductViewModel(product);
            
            return View(model);
        }
        


        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Guid imageId = model.ImageId; ;
                    
                    if(model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        imageId = Guid.NewGuid();

                        // 1. Definir o caminho físico onde a imagem será guardada
                        string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", $"{imageId}.jpg");

                        // 2. Guardar a imagem no disco do Somee
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }
                    }

                    var product = _converterHelper.ToProduct (model, imageId, false);

                 
                    product.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                    await _productRepository.UpdateAsync(product);
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! await _productRepository.ExistAsync(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Products/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product =  await _productRepository.GetByIdAsync(id.Value);
            if  (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product =  await _productRepository.GetByIdAsync(id);
            await _productRepository.DeleteAsync(product);
            
            return RedirectToAction(nameof(Index));
        }
       

    }
}
