using Bloggie.Web.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Bloggie.Web.Models.ViewModels
{
    public class AddBlogPostRequest
    {
        [Required]
        public string Heading { get; set; }
        [Required]
        public string PageTitle { get; set; }
        [Required(ErrorMessage = "Please enter content.")]
        public string Content { get; set; }
        [Required]
        public string ShortDescription { get; set; }
        [Required]
        public string FeaturedImageUrl { get; set; }
        [Required]
        public string UrlHandle { get; set; }
        [Required]
        public DateTime PublishedDate { get; set; }
        [Required]
        public string Author { get; set; }
        public bool Visible { get; set; }

        //Display tags
        public IEnumerable<SelectListItem> Tags { get; set; }
        //Collect tag
        public string[] SelectedTags { get; set; } = Array.Empty<string>();
    }
}
