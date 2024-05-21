using Bloggie.Web.Models.Domain;
using System.Collections;

namespace Bloggie.Web.Repositories
{
    public interface IBlogPostLikeRepository
    {
        Task<int> GetTotalLikes(Guid BlogPostId);
        
        Task<IEnumerable<BlogPostLike>> GetLikesForBlog(Guid BlogPostId);

        Task<BlogPostLike> AddLikeForBlog(BlogPostLike blogPostLike);
    }
}
