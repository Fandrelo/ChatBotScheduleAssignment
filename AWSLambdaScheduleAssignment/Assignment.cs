using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambdaScheduleAssignment
{
    public class Assignment
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string DueDate { get; set; }
        public string MailList { get; set; }
    }
}
