using Bloggie.Web.Models.Domain;
using Bloggie.Web.Models.ViewModels;
using Bloggie.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bloggie.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBlogPostsController : Controller
    {
        private readonly ITagRepository tagRepository;
        private readonly IBlogPostRepository blogPostRepository;

        public AdminBlogPostsController(ITagRepository tagRepository, IBlogPostRepository blogPostRepository)
        {
            this.tagRepository = tagRepository;
            this.blogPostRepository = blogPostRepository;
        }


        [HttpGet]
       public async Task<IActionResult> Add() 
        {
            //Get tags from repository
            var tags = await tagRepository.GetAllAsync();

            var model = new AddBlogPostRequest
            {
                Tags = tags.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddBlogPostRequest addBlogPostRequest)
        {
            //Map view model to domain model
            var blogpost = new BlogPost
            {
                Heading = addBlogPostRequest.Heading,
                PageTitle = addBlogPostRequest.PageTitle,
                Content = addBlogPostRequest.Content,
                ShortDescription = addBlogPostRequest.ShortDescription,
                FeaturedImageUrl = addBlogPostRequest.FeaturedImageUrl,
                UrlHandle = addBlogPostRequest.UrlHandle,
                PublishedDate = addBlogPostRequest.PublishedDate,
                Author = addBlogPostRequest.Author,
                Visible = addBlogPostRequest.Visible,
            };

            var selectedTags = new List<Tag>();
            //Map tags from selected tags
            foreach  (var selectedTagId in addBlogPostRequest.SelectedTags) 
            {
                var selectedTagIdAsGuid = Guid.Parse(selectedTagId);
                var existingTag = await tagRepository.GetAsync(selectedTagIdAsGuid);
                if (existingTag != null) 
                { 
                    selectedTags.Add(existingTag);
                }
            }

            //Mapping tags back to domain model
            blogpost.Tags = selectedTags;

            await blogPostRepository.AddAsync(blogpost);

            TempData["PostAdded"] = "You have successfully added a post!";

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> List() 
        {
            //Call the repository 
            var blogposts = await blogPostRepository.GetAllAsync();

            return View(blogposts); 
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            //Retrieve the result from the repository 
            var blogPost = await blogPostRepository.GetAsync(id);
            var tagsDomainModel = await tagRepository.GetAllAsync();

            if (blogPost != null)
            {
                //map the domain model into the view model
                var model = new EditBlogPostRequests
                {
                    Id = blogPost.Id,
                    Heading = blogPost.Heading,
                    PageTitle = blogPost.PageTitle,
                    Content = blogPost.Content,
                    Author = blogPost.Author,
                    FeaturedImageUrl = blogPost.FeaturedImageUrl,
                    UrlHandle = blogPost.UrlHandle,
                    ShortDescription = blogPost.ShortDescription,
                    PublishedDate = blogPost.PublishedDate,
                    Visible = blogPost.Visible,
                    Tags = tagsDomainModel.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                    }),
                    SelectedTags = blogPost.Tags.Select(x => x.Id.ToString()).ToArray()
                };

                return View(model);
            }

            //pass data to view
            return View(null);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditBlogPostRequests editBlogPostRequests)
        {
            // map view model back to domain model
            var blogPostDomainModel = new BlogPost
            {
                Id = editBlogPostRequests.Id,
                Heading = editBlogPostRequests.Heading,
                PageTitle = editBlogPostRequests.PageTitle,
                Content = editBlogPostRequests.Content,
                Author = editBlogPostRequests.Author,
                ShortDescription = editBlogPostRequests.ShortDescription,
                FeaturedImageUrl = editBlogPostRequests.FeaturedImageUrl,
                PublishedDate = editBlogPostRequests.PublishedDate,
                UrlHandle = editBlogPostRequests.UrlHandle,
                Visible = editBlogPostRequests.Visible
            };

            // Map tags into domain model
            var selectedTags = new List<Tag>();
            foreach (var selectedTag in editBlogPostRequests.SelectedTags)
            {
                if (Guid.TryParse(selectedTag, out var tag)) 
                {
                    var foundTag = await tagRepository.GetAsync(tag);

                    if (foundTag != null) 
                    {
                    selectedTags.Add(foundTag);
                    }
                }
            }

            blogPostDomainModel.Tags = selectedTags;

            // Submit information to repository to update 
            var updatedBlog = await blogPostRepository.UpdateAsync(blogPostDomainModel);

            if (updatedBlog != null) 
            {
                TempData["EditPost"] = "The changes have been saved successfully!";

                // show success notification
                return RedirectToAction("List");
            }

            // show error notification
            return RedirectToAction("Edit");
            // redirect to GET
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(EditBlogPostRequests editBlogPostRequests)
        {
            // Talk to repository to delete this blogpost and tags 
            var deletedBlogPost = await blogPostRepository.DeleteAsync(editBlogPostRequests.Id);

            if (deletedBlogPost != null) 
            {
                TempData["DeletePost"] = "The post has been deleted successfully!";

                // Show success notif
                return RedirectToAction("List");   
            }

            // Show error notif
            return RedirectToAction("Edit", new {id  = editBlogPostRequests.Id});

            // display the response

        }

    }
}
