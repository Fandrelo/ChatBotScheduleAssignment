using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot.Models.ScheduleAssignment
{
    public class Email
    {
        [Required]
        public string Address { get; set; }
        [Required]
        [DisplayName("Mail List")]
        public long MailList { get; set; }
        [DisplayName("Owner")]
        public string OwnerEmail { get; set; }

        [DisplayName("Mail List")]
        public MailList MailListNavigation { get; set; }
    }
}
