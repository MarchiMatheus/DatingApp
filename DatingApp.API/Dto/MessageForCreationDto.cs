using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dto
{
    public class MessageForCreationDto
    {
        public MessageForCreationDto()
        {
            this.MessageSent = DateTime.Now;
        }

        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public DateTime MessageSent { get; set; }
        public string Content { get; set; }
    }
}