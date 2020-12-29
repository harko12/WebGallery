using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Data
{
    public class GalleryDbContext : DbContext
    {
        public GalleryDbContext(DbContextOptions<GalleryDbContext> options) : base(options)
        {
        }

        public DbSet<ImageGallery> ImageGalleries { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaggedImages> TaggedImages { get; set; }
        public DbSet<GalleryUser> GalleryUsers { get; set; }
        public DbSet<ImageCaption> ImageCaptions { get; set; }

    }
}
