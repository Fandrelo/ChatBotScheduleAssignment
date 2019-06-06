using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot.Models.ScheduleAssignment
{
    public class Assignment
    {
        public long Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DisplayName("Due Date")]
        public DateTime DueDate { get; set; }
        [DisplayName("Mail List")]
        [Required]
        public long MailList { get; set; }
        [DisplayName("Owner Mail")]
        public string OwnerEmail { get; set; }

        [DisplayName("Mail List")]
        public MailList MailListNavigation { get; set; }
    }
}
