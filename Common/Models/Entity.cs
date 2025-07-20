namespace ZeeAcom.Common.Models
{
    public class Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; }= string.Empty;
        public string Suburb { get; set; }= string.Empty;
        public bool Active { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid OwnerId { get; set; }
        public Guid SessionId { get; set; }
    }
}
