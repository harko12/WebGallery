using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebGallery.Data;
using WebGallery.Models;

namespace WebGallery.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly WebGallery.Data.GalleryDbContext _context;

        public IndexModel(WebGallery.Data.GalleryDbContext context, ILogger<IndexModel> logger)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            var gm = new GalleryManager(_context);
            var gal = ViewData["gal"];
            if (gal == null)
            {
                //TODO: this viewdata never gets used, so it inits on every reload of index
                gal = gm.InitGalleryByName("testgallery", "", @"\\mansun\shared_photos");
                ViewData["gal"] = gal;
            }
        }
    }
}
