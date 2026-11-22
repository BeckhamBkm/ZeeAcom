using System.ComponentModel.DataAnnotations;
using ZeeAcom.Common.Models.PictureModels;

namespace ZeeAcom.Common.Models.PropertyModels
{
    public sealed class CreatePropertyModel
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Suburb { get; set; } = string.Empty;
        public bool Active { get; set; }

        /// <summary>Monthly rent in rand.</summary>
        [Range(0, 1_000_000, ErrorMessage = "Enter a monthly rent between R0 and R1 000 000.")]
        public decimal? MonthlyRent { get; set; }

        public DateTime DateCreated { get; set; }
        public Guid SessionId { get; set; }

        /// <summary>
        /// Collection of pictures of the property
        /// </summary>
        public ICollection<PictureModel> Pictures { get; set; } = [];

        /// <summary>
        /// Owner Navigation Property
        /// </summary>
        public Guid OwnerId { get; set; }
    }
}
