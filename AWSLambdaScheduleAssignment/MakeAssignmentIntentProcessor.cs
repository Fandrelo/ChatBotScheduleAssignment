using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;

namespace AWSLambdaScheduleAssignment
{
    public class MakeAssignmentIntentProcessor : AbstractIntentProcessor
    {
        public const string TITLE_SLOT = "Title";
        public const string DESCRIPTION_SLOT = "Description";
        public const string DUE_DATE_SLOT = "DueDate";
        public const string MAIL_LIST_SLOT = "MailList";
        public const string INVOCATION_SOURCE = "invocationSource";
        private bool _flagDescription = true;
        private bool _flagMailList = true;

        public override LexResponse Process(LexEvent lexEvent, ILambdaContext context)
        {
            IDictionary<string, string> slots = lexEvent.CurrentIntent.Slots;
            IDictionary<string, string> sessionAttributes = lexEvent.SessionAttributes ?? new Dictionary<string, string>();

            if (slots.All(x => x.Value == null))
            {
                return Delegate(sessionAttributes, slots);
            }
			
            Assignment assignment = CreateAssignment(slots, lexEvent.InputTranscript);

            if (string.Equals(lexEvent.InvocationSource, "DialogCodeHook", StringComparison.Ordinal))
            {
                sessionAttributes["DONE"] = "FALSE";

                var validateResult = ValidateAssignment(ref assignment, ref slots);

                sessionAttributes["TRANSCRIPT"] = lexEvent.InputTranscript;

                if (!validateResult.IsValid)
                {
                    slots[validateResult.ViolationSlot] = null;
                    return ElicitSlot(sessionAttributes, lexEvent.CurrentIntent.Name, slots, validateResult.ViolationSlot, validateResult.Message);
                }

                sessionAttributes["CURRENT_OBJECT"] = SerializeObject(assignment);

                return Delegate(sessionAttributes, slots);
            }

            sessionAttributes["DONE"] = "TRUE";

            return Close(
                sessionAttributes,
                "Fulfilled",
                new LexResponse.LexMessage
                {
                    ContentType = MESSAGE_CONTENT_TYPE,
                    Content = $"The assignment {assignment.Title} was sent to the mail list {assignment.MailList}, and its due on {assignment.DueDate}"
                }
            );
        }

        private Assignment CreateAssignment(IDictionary<string, string> slots, string transcript)
        {
            Assignment assignment = new Assignment
            {
                Title = slots.ContainsKey(TITLE_SLOT) ? slots[TITLE_SLOT] : null,
                Description = slots.ContainsKey(DESCRIPTION_SLOT) ? slots[DESCRIPTION_SLOT] : null,
                DueDate = slots.ContainsKey(DUE_DATE_SLOT) ? slots[DUE_DATE_SLOT] : null,
                MailList = slots.ContainsKey(MAIL_LIST_SLOT) ? slots[MAIL_LIST_SLOT] : null
            };

            if (assignment.Title != null &&
                assignment.Description != null &&
                assignment.DueDate == null &&
                assignment.MailList == null)
            {
                slots[DESCRIPTION_SLOT] = transcript;
                assignment.Description = transcript;
                return assignment;
            }

            if (transcript == "yes" || transcript == "Yes" || transcript == "no" || transcript == "No")
            {
                return assignment;
            }

            if (!string.IsNullOrEmpty(assignment.Title) &&
                !string.IsNullOrEmpty(assignment.Description) &&
                !string.IsNullOrEmpty(assignment.DueDate) &&
                !string.IsNullOrEmpty(assignment.MailList))
            {
                assignment.MailList = transcript;
            }

            /* (slots.ContainsKey(TITLE_SLOT) &&
                slots.ContainsKey(DESCRIPTION_SLOT) &&
                !slots.ContainsKey(DUE_DATE_SLOT) &&
                slots.ContainsKey(MAIL_LIST_SLOT))
            {
                slots[DESCRIPTION_SLOT] = transcript;
                order.Description = transcript;
                return order;
            }

            if (slots.ContainsKey(TITLE_SLOT) &&
                slots.ContainsKey(DESCRIPTION_SLOT) &&
                slots.ContainsKey(DUE_DATE_SLOT) &&
                slots.ContainsKey(MAIL_LIST_SLOT))
            {
                order.MailList = transcript;
            }*/

            return assignment;
        }

        private ValidationResult ValidateAssignment(ref Assignment assignment, ref IDictionary<string, string> slots)
        {

            if (!string.IsNullOrEmpty(assignment.DueDate))
            {
                DateTime pickUpDate = DateTime.MinValue;
                if (!DateTime.TryParse(assignment.DueDate, out pickUpDate))
                {
                    return new ValidationResult(false, DUE_DATE_SLOT,
                        "I did not understand that, what date is the assignment due?");
                }
                if (pickUpDate < DateTime.Today)
                {
                    return new ValidationResult(false, DUE_DATE_SLOT,
                        "You can pick up the flowers from tomorrow onwards.  What day would you like to pick them up?");
                }
            }

            if (!string.IsNullOrEmpty(assignment.MailList))
            {
                if (assignment.MailList.Contains("|"))
                {
                    var splittedRaw = assignment.MailList.Split("|");
                    var data = splittedRaw[1].Split(";");
                    var delimitedList = string.Join("-", data);
                    var dataList = data.ToList();
                    dataList = dataList.ConvertAll(s => s.ToLower());
                    string result = data.FirstOrDefault(s => s.Contains(splittedRaw[0]));
                    var toFind = splittedRaw[0].ToLower();
                    var foundAt = -1;
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        if (dataList[i].IndexOf(toFind) != -1)
                        {
                            foundAt = i;
                            break;
                        }
                    }
                    if (!data.Any(s => s.Contains(splittedRaw[0])))
                    {
                        return new ValidationResult(false, MAIL_LIST_SLOT,
                            $"Can't find that mail list. What mail list would you like this assignment to be for?");
                    }
                    else
                    {
                        assignment.MailList = data.Where(s => s == splittedRaw[0]).FirstOrDefault();
                        slots[MAIL_LIST_SLOT] = data.Where(s => s == splittedRaw[0]).FirstOrDefault();
                    }
                    /*if (assignment.MailList == "test")
                    {
                        return new ValidationResult(false, MAIL_LIST_SLOT,
                            "Can't find this mail list. What mail list would you like this assignment to be for?");
                    }
                    var Mails = new List<string>
                        {
                            "AI",
                            "First Semester",
                            "Second Semester",
                            "JUST",
                        };*/
                    /*int index = Mails.FindIndex(mailList => mailList.Contains(mailList));
                    if (index == -1)
                    {
                        return new ValidationResult(false, MAIL_LIST_SLOT,
                            "Can't find this mail list. What mail list would you like this assignment to be for?");
                    }*/
                }

            }

            return ValidationResult.VALID_RESULT;
        }

        /*/// <summary>
        /// Verifies that any values for flower type slot in the intent is valid.
        /// </summary>
        /// <param name="flowertypeString"></param>
        /// <returns></returns>
        private ValidationResult ValidateFlowerType(string flowerTypeString)
        {
            bool isFlowerTypeValid = Enum.IsDefined(typeof(FlowerTypes), flowerTypeString.ToUpper());

            if (Enum.TryParse(typeof(FlowerTypes), flowerTypeString, true, out object flowerType))
            {
                _chosenFlowerType = (FlowerTypes)flowerType;
                return ValidationResult.VALID_RESULT;
            }
            else
            {
                return new ValidationResult(false, TITLE_SLOT, String.Format("We do not have {0}, would you like a different type of flower? Our most popular flowers are roses", flowerTypeString));
            }
        }*/

    }

}
