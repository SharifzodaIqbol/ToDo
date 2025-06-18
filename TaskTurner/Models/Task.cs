using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTurner.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }

    public enum TaskState
    {
        InProgress,
        Complete,
        NotStarted,
        Late,
        Archived,
        Deleted
    }

    public enum TaskCategory
    {
        Work,
        Personal,
        Home,
        Health,
        Finance,
        Shopping,
        SocialFamily,
        Education,
        Travel,
        Errands,
        Hobbies,
        VolunteeringCommunity,
        Projects,
        Goals
    }

    public enum TaskImportance{
        Low,
        Medium,
        High,
        Critical
        }
}

