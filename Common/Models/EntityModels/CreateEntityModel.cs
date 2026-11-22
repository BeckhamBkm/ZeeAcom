using System.ComponentModel.DataAnnotations;

namespace ZeeAcom.Common.Models.EntityModels
{
    public sealed class CreateEntityModel
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Suburb { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid SessionId { get; set; }

        /// <summary>
        /// Collection Of pictures of the Entity
        /// </summary>
        public ICollection<Picture> Pictures { get; set; } = [];

        /// <summary>
        /// Owner Navigation Property
        /// </summary>
        public Guid OwnerId { get; set; }
    }
}
