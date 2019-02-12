namespace DatingApp.API.Models
{
    public class Like
    {
        //Id of the user that's liking another user
        public int LikerId { get; set; }

        //Id of the user that's being liked
        public int LikeeId { get; set; }

        //User that liked another user
        public User Liker { get; set; }

        //User that's being liked
        public User Likee { get; set; }
    }
}