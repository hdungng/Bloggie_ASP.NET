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
            // Get Tags from Repository
            var tags = await tagRepository.GetAllAsync();

            var model = new AddPostBlogRequest
            {
                Tags = tags.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddPostBlogRequest addPostBlogRequest)
        {
            // Map (view model request) to (domain model)
            var blogPost = new BlogPost
            {
                Heading = addPostBlogRequest.Heading,
                PageTitle = addPostBlogRequest.PageTitle,
                Content = addPostBlogRequest.Content,
                ShortDescription = addPostBlogRequest.ShortDescription,
                FeaturedImageUrl = addPostBlogRequest.FeaturedImageUrl,
                UrlHandle = addPostBlogRequest.UrlHandle,
                PublishedDate = addPostBlogRequest.PublishedDate,
                Author = addPostBlogRequest.Author,
                Visible = addPostBlogRequest.Visible,
            };

            // Map Tags from selected Tags
            // from TagGuiId (string[]) to List<Tag>
            var selectedTags = new List<Tag>();

            foreach (var selectedTagId in addPostBlogRequest.SelectedTags)
            {
                var selectedTagIdAsGuid = Guid.Parse(selectedTagId);
                var existingTag = await tagRepository.GetAsync(selectedTagIdAsGuid);

                if (existingTag != null)
                {
                    selectedTags.Add(existingTag);
                }
            }
            // Mapping tags back to the domain model
            blogPost.Tags = selectedTags;

            await blogPostRepository.AddAsync(blogPost);
            return RedirectToAction("Add");
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            // Call the repository
            var blogPosts = await blogPostRepository.GetAllAsync();

            return View(blogPosts);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // Retrieve the result from the repository
            var blogPost = await blogPostRepository.GetAsync(id);
            var tagsDomainModel = await tagRepository.GetAllAsync();

            if (blogPost != null)
            {
                // map the Domain model into the View model
                var model = new EditPostBlogRequest
                {
                    Id = blogPost.Id,
                    Heading = blogPost.Heading,
                    PageTitle = blogPost.PageTitle,
                    Content = blogPost.Content,
                    Author = blogPost.Author,
                    FeaturedImageUrl = blogPost.FeaturedImageUrl,
                    PublishedDate = blogPost.PublishedDate,
                    ShortDescription = blogPost.ShortDescription,
                    UrlHandle = blogPost.UrlHandle,
                    Visible = blogPost.Visible,
                    Tags = tagsDomainModel.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                    }),
                    SelectedTags = blogPost.Tags.Select(x => x.Id.ToString()).ToArray(),
                };

                // Pass data to view
                return View(model);
            }

            // Pass data to view
            return View(null);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(EditPostBlogRequest editPostBlogRequest)
        {
            // Map (View Model Request) into Domain Model
            var blogPostDomainModel = new BlogPost
            {
                Id = editPostBlogRequest.Id,
                Heading = editPostBlogRequest.Heading,
                PageTitle = editPostBlogRequest.PageTitle,
                Content = editPostBlogRequest.Content,
                Author = editPostBlogRequest.Author,
                FeaturedImageUrl = editPostBlogRequest.FeaturedImageUrl,
                PublishedDate = editPostBlogRequest.PublishedDate,
                ShortDescription = editPostBlogRequest.ShortDescription,
                UrlHandle = editPostBlogRequest.UrlHandle,
                Visible = editPostBlogRequest.Visible,
            };

            // Map Tags into Domain model
            var selectedTags = new List<Tag>();

            foreach(var selectedTag in editPostBlogRequest.SelectedTags)
            {
                if(Guid.TryParse(selectedTag, out var tag))
                {
                    var foundTag = await tagRepository.GetAsync(tag);

                    if(foundTag != null)
                    {
                        selectedTags.Add(foundTag);
                    }
                }
            }

            blogPostDomainModel.Tags = selectedTags;

            // Submit Information to Repository to update
            var updatedBlog = await blogPostRepository.UpdateAsync(blogPostDomainModel);  
            
            
            if(updatedBlog != null)
            {
                // Show a successful notification 
                return RedirectToAction("Edit");
            }

            // Show a failed notification 
            return RedirectToAction("Edit");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(EditPostBlogRequest editPostBlogRequest)
        {
            // Talk to repository to delete this Blog Post and Tags
            var deletedBlog = await blogPostRepository.DeleteAsync(editPostBlogRequest.Id);

            if(deletedBlog != null)
            {
                // Show a success notification
                return RedirectToAction("List");
            }

            // Show a error notification
            return RedirectToAction("Edit", new { id = editPostBlogRequest.Id  });
        }
    }
}
