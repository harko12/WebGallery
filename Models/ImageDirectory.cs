using MetadataExtractor;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using WebGallery.Data;
using WebGallery.Extensions;

namespace WebGallery.Models
{
    public class ImageDirectory
    {
        public ImageDirectory(string name)
        {
            Name = name;
            ImageList = new List<string>();
            DirectoryList = new List<string>();
        }
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string ImageRoot { get; set; }
        public string ImagePath { get; set; }
        public List<string> DirectoryList { get; set; }
        public List<string> ImageList { get; set; }
    }

    public class ImageGallery
    {
        public ImageGallery()
        {
            ImageList = new List<Image>();
            SelectedGroups = new Dictionary<string, bool>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }
        public List<Image> ImageList { get; set; }
        public Dictionary<string, bool> SelectedGroups;
    }

    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
        public DateTime DateTaken { get; set; }
        public string DateString { get; set; }
        public string DateMonthString { get; set; }
        public string DateYearString { get; set; }
        public bool Limbo { get; set; }
    }

    public class GalleryUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string AspNetId { get; set; }
        public string UserName { get; set; }
    }

    public class ImageCaption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ImageId { get; set; }
        public string Caption { get; set; }
    }

    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class TaggedImages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageID { get; set; }
        public List<Tag> Tags { get; set; }
    }

    public class GalleryManager
    {
        public const string SESSION_KEY = "1234Gallery{0}";
        public const string DateFormat = "MM/dd/yyyy";
        public const string MonthFormat = "MM";
        public const string YearFormat = "yyyy";

        private GalleryDbContext _dbContext;

        public GalleryManager(GalleryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public void SetGallery(ISession ses, ImageGallery gal)
        {
            var key = string.Format(SESSION_KEY, gal.Name);
            ses.SetGallery(key, gal);
        }

        public ImageGallery GetGallery(ISession ses, string name)
        {
            var key = string.Format(SESSION_KEY, name);
            ImageGallery gal = ses.GetGallery(key);
            return gal;
        }
        
        /// <summary>
        /// returns found gallery, or new one by that name
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ImageGallery InitGalleryByName(string name, string subDirPath, string drivePath)
        {
            ImageGallery gallery = null;

            using (var db = _dbContext)
            {
                db.ImageGalleries.Load();
                var query = (from g in db.ImageGalleries.Include(gal => gal.ImageList)
                             where g.Name == name
                             select g).FirstOrDefault();
                if (query != null)
                {
                    gallery = query;
                }
                else
                {
                    var newGallery = new ImageGallery() { Name = name };
                    db.ImageGalleries.Add(newGallery);
                    gallery = newGallery;
                }

                //UpdateImageDirectory(db, gallery, subDirPath, drivePath);

                return gallery;
            }
        }

        public DateTime? GetPictureOriginalDate(string imagePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(imagePath);
                foreach (var d in directories)
                {
                    if (d.Name != "Exif SubIFD")
                    {
                        continue;
                    }
                    foreach (var t in d.Tags)
                    {
                        if (t.Name == "Date/Time Original")
                        {
                            //2012:08:18 00:54:29
                            var originalDate_string = t.Description;
                            var originalDate = DateTime.ParseExact(originalDate_string, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                            return originalDate;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // nothign right now
            }
            return null;
        }

        public void UpdateImageDirectory(GalleryDbContext db, ImageGallery gallery, string subDirPath, string drivePath)
        {
            var imagePath = drivePath; // + "\\" + subDirPath;

            var imageFiles = System.IO.Directory.GetFiles(imagePath);
            var imageDirectories = System.IO.Directory.GetDirectories(imagePath);
            var path = subDirPath;
            foreach (var item in imageFiles)
            {
                var regexPattern = @"\.(?i:)(?:jpg|gif|mp4|jpeg|png)$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(item, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    continue;
                }
                var itemFilename = Path.GetFileName(item);
                var query = (from img in gallery.ImageList
                             where img.Filename == itemFilename
                             select img).Any();

                if (query)
                {
                    continue;
                }

                var pictureDate = this.GetPictureOriginalDate(imagePath + "\\" + itemFilename);
                if (pictureDate == null)
                {
                    pictureDate = File.GetCreationTime(item);
                }
                var newImg = new WebGallery.Models.Image()
                {
                    Path = path,
                    Filename = itemFilename,
                    DateTaken = (DateTime)pictureDate // not sure what to default
                };
                /*
                if (!item.Contains(".mp4"))
                {
                    newImg.DateTaken = File.GetLastWriteTime(item);
                }
                else
                {
                    newImg.DateTaken = File.GetLastWriteTime(item);
                }
                */
                newImg.DateString = newImg.DateTaken.ToString(DateFormat);
                newImg.DateMonthString = newImg.DateTaken.ToString(MonthFormat);
                newImg.DateYearString = newImg.DateTaken.ToString(YearFormat);
                gallery.ImageList.Add(newImg);
            }
            var rc = db.SaveChanges(true);
            foreach (var dir in imageDirectories)
            {
                var subDir = Path.GetFileName(dir);
                var newPath = imagePath + @"\" + subDir;
                UpdateImageDirectory(db, gallery, subDir, newPath);
            }
        }
    }
}