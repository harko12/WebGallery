using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebGallery.Models;
using Microsoft.AspNetCore.Hosting;
using WebGallery.Data;
using WebGallery.UIBL;
using System.Text.Json;

namespace WebGallery.Pages.Gallery
{
    public class ImageGroupsModel : PageModel
    {
        private readonly WebGallery.Data.GalleryDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ImageGroupsModel(WebGallery.Data.GalleryDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public ImageGroupModel GetImageGroupModel(List<Image> images)
        {
            var mod = new ImageGroupModel(_context) { Images = images };
            return mod;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, List<Image>>>> Groups { get; set; }

        public Dictionary<string, Dictionary<string, Dictionary<string, List<Image>>>> GetImageGroups(ImageGallery gallery)
        {
            var defaultDate = string.Format("{0:MM/dd/yyyy}", DateTime.Today);
            //            var defaultDate = $"{DateTime.Today.Month}/01/{DateTime.Today.Year}";
            var currentDate = HttpContext.Session.GetString("CurrentDate") != null ? HttpContext.Session.GetString("CurrentDate") : defaultDate;
            var currentDateInfo = currentDate.Split('/');

            var groups = gallery.ImageList.OrderBy(i => i.DateString).OrderBy(i => i.Filename).Select(i => i.DateString).Distinct().ToList();

            var dirSet = new Dictionary<string, Dictionary<string, Dictionary<string, List<Image>>>>();
            for (int lcv = 0, length = groups.Count; lcv < length; lcv++)
            {
                var dateString = groups[lcv];
                var datesplit = dateString.Split('/');
                var month = datesplit[0];
                var day = datesplit[1];
                var year = datesplit[2];
                if (!dirSet.ContainsKey(year))
                {
                    dirSet.Add(year, new Dictionary<string, Dictionary<string, List<Image>>>());
                }
                if (!dirSet[year].ContainsKey(month))
                {
                    dirSet[year].Add(month, new Dictionary<string, List<Image>>());
                }
                if (!dirSet[year][month].ContainsKey(day))
                {
                    var images = gallery.ImageList.Where(i => i.DateString == dateString).OrderBy(i => i.DateTaken).ToList();
                    dirSet[year][month].Add(day, images);
                }
            }

            //ViewBag.GroupList = groups;
            ViewData["DirSet"] =  JsonSerializer.Serialize(dirSet);
            //return DisplayImagesForDate(currentDate, (int)DateGroups.Month);
            ViewData["CurrentDate"] = currentDate;
            ViewData["GroupStyle"] = DateGroups.Month;
            return dirSet;
        }


        public void OnGet()
        {
            var gm = new GalleryManager(_context);
            var gal = gm.InitGalleryByName("testgallery", "", @"\\mansun\shared_photos");
            if (gal != null)
            {
                Groups = GetImageGroups(gal);
            }
        }

        public void OnPost(string dirSet, string date, int group)
        {
            Groups = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, List<Image>>>>>(dirSet);

            ViewData["DirSet"] = dirSet;
            ViewData["galleryName"] = "testgallery";
            ViewData["CurrentDate"] = date;
            ViewData["GroupStyle"] = (DateGroups)group;
        }
    }
}
