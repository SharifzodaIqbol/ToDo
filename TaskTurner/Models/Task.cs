using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTurner.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueData { get; set; }
        public DateTime StartData { get; set; }
        public bool IsCompleted { get; set; }
        public TimeSpan Timer { get; set; }

        public TaskState TaskState { get; set; }
        public TaskCategory TaskCategory { get; set; }
        public TaskImportance TaskImportance { get; set; }
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
        High
        }
}

