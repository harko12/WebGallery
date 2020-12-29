using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Extensions
{
    public static class WebGalleryExtensions
    {
        public static ImageGallery GetGallery(this ISession session, string key)
        {
            var json = session.GetString(key);
            if (json == null)
            {
                return null;
            }
            var gal = JsonSerializer.Deserialize<ImageGallery>(json);
            return gal;
        }

        public static void SetGallery(this ISession session, string key, ImageGallery gal)
        {
            var json = JsonSerializer.Serialize(gal);
            session.SetString(key, json);
        }
    }
}
