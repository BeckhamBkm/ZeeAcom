using ZeeAcom.Common.Models.EntityModels;

namespace ZeeAcom.Common.Entities
{
    public sealed class Picture
    {
        public int Id { get; set; }

        public required string Task { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? DateCreated { get; set; }

        public Guid SessionId { get; set; }

        public Guid EntityId { get; set; }
        public Entity? Entity { get; set; }

    }
}
