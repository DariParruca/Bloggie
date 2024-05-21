using Bloggie.Web.Data;
using Bloggie.Web.Models.Domain;
using Bloggie.Web.Models.ViewModels;
using Bloggie.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloggie.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTagsController : Controller
    {
        public ITagRepository TagRepository { get; }

        public AdminTagsController(ITagRepository tagRepository)
        {
            TagRepository = tagRepository;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        
        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> Add(AddTagRequest addTagRequest) 
        {
            ValidateAddTagRequest(addTagRequest);

            if (!ModelState.IsValid)
            {
                return View();
            }

            //Mapping the AddTagRequest to Tag Domain Model
            var tag = new Tag
            {
                Name = addTagRequest.Name,
                DisplayName = addTagRequest.DisplayName,
            };

            await TagRepository.AddAsync(tag);
           
            TempData["AddTag"] = "The tag has been added successfully!";

            return RedirectToAction("List");
        }

        [HttpGet]
        [ActionName("List")]
        public async Task<IActionResult> List()
        {
            //use dbContext to read the tags
            var tags = await TagRepository.GetAllAsync();

            return View(tags);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id) 
        {
            var tag = await TagRepository.GetAsync(id);

            if (tag != null)
            {
                var editTagRequest = new EditTagRequest
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    DisplayName = tag.DisplayName,
                };
                return View(editTagRequest);
            }
            else
            {
                return View(null);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTagRequest editTagRequest)
        {
            var tag = new Tag
            {
                Id = editTagRequest.Id,
                Name = editTagRequest.Name,
                DisplayName = editTagRequest.DisplayName,
            };

            var updatedTag = await TagRepository.UpdateAsync(tag);

            if (updatedTag != null) 
            {
                //Show success notification
                TempData["EditTag"] = "The changes have been added successfully!";
                return RedirectToAction("List");
            }

            //Show error notification
            return RedirectToAction("Edit", new { id = editTagRequest.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(EditTagRequest editTagRequest) 
        {
            var deletedTag =  await TagRepository.DeleteAsync(editTagRequest.Id);
            if (deletedTag != null)
            {
                TempData["DeleteTag"] = "The tag has been deleted successfully!";

                //Show success notification
                return RedirectToAction("List");
            }


            //Show an error notification
            return RedirectToAction("Edit", new {id = editTagRequest.Id });
        }

        private void ValidateAddTagRequest(AddTagRequest addTagRequest)
        {
            if (addTagRequest.Name is not null && addTagRequest.DisplayName is not null)
            {
                if(addTagRequest.Name == addTagRequest.DisplayName)
                {
                    ModelState.AddModelError("DisplayName", "Name cannot be the same as DisplayName");
                }
            }
        }
    }
}
