namespace Models
{
    public class ToDoTask
    {
        public int Id { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CompletedAt { get; set; } = string.Empty;
    }

    public enum TaskStatus
    {
        Pending,
        Completed
    }
}