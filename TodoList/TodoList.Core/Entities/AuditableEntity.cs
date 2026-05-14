namespace TodoList.Core.Entities
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; set; }
    }
}
