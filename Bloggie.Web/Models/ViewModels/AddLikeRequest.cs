namespace Bloggie.Web.Models.ViewModels
{
    public class AddLikeRequest
    {
        public Guid BLogPostId { get; set; }

        public Guid UserId { get; set; }

    }
}
