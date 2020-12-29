using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebGallery.Data;
using WebGallery.Models;

namespace WebGallery.Pages.Gallery
{
    public class ImageGroupModel : PageModel
    {
        private readonly WebGallery.Data.GalleryDbContext _context;

        public ImageGroupModel(WebGallery.Data.GalleryDbContext context)
        {
            _context = context;
        }

        public IList<Image> Images { get;set; }


        public async Task OnGetAsync()
        {
            if (Images == null || Images.Count == 0)
            {
                Images = await _context.Images.ToListAsync();
            }
        }
    }
}
