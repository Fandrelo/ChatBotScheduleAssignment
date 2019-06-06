using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot.Models.ScheduleAssignment
{
    public class MailList
    {
        public MailList()
        {
            Emails = new HashSet<Email>();
            Assignments = new HashSet<Assignment>();
        }

        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DisplayName("Owner Mail")]
        public string OwnerEmail { get; set; }

        [DisplayName("Emails")]
        public ICollection<Email> Emails { get; set; }
        [DisplayName("Assignments")]
        public ICollection<Assignment> Assignments { get; set; }
    }
}
