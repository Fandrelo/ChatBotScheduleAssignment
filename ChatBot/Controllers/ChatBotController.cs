using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatBot.Data;
using ChatBot.Models;
using ChatBot.Models.ScheduleAssignment;
using ChatBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChatBot.Controllers
{
    [Authorize]
    public class ChatBotController : Controller
    {
        private readonly IAWSLexService _awsLexService;
        private ISession _userHttpSession;
        private Dictionary<string, string> _lexSessionData;
        private List<ChatBotMessage> _botMessages;
        private string _currentSlot;
        private readonly string _botMsgKey = "ChatBotMessages";
        private readonly string _botAtrribsKey = "LexSessionData";
        private readonly string _botCurrentSlotKey = "LatestSlot";
        private string _userSessionID = string.Empty;
        private readonly IEmailSender _emailSender;
        private readonly IStackDriverLogger _stackDriverLogger;
        private readonly ChatBotContext _context;
        private const string MAIL_LIST_SLOT = "MailList";

        public ChatBotController(
            IAWSLexService awsLexService,
            IEmailSender emailSender,
            IStackDriverLogger stackDriverLogger,
            ChatBotContext context)
        {
            _awsLexService = awsLexService;
            _emailSender = emailSender;
            _stackDriverLogger = stackDriverLogger;
            _context = context;
        }

        /// <summary>
        /// Tests mail and logger
        /// </summary>
        /// <param name="email">email address to send the email</param>
        /// <param name="message">email content/log content</param>
        /// <returns></returns>
        [Route("/ChatBot/Info/{email}/{message}")]
        public async Task<IActionResult> Info(string email, string message)
        {
            await _emailSender.SendEmailAsync(email, $"TESTING NOW {DateTime.Now.ToString()}", message);
            await _stackDriverLogger.WriteLogEntryAsync(nameof(ChatBotController), message, null);
            return View();
        }

        public IActionResult Index(List<ChatBotMessage> messages)
        {
            return View(messages);
        }

        public IActionResult ClearBot()
        {
            _userHttpSession = HttpContext.Session;

            _userHttpSession.Clear();

            _botMessages = new List<ChatBotMessage>();
            _lexSessionData = new Dictionary<string, string>();

            _userHttpSession.Set(_botMsgKey, _botMessages);
            _userHttpSession.Set(_botAtrribsKey, _lexSessionData);

            _awsLexService.Dispose();
            return View(nameof(Index), _botMessages);
        }

        [HttpGet]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetChatMessage(string userMessage)
        {
            _userHttpSession = HttpContext.Session;
            _userSessionID = _userHttpSession.Id;
            _botMessages = _userHttpSession.Get<List<ChatBotMessage>>(_botMsgKey) ?? new List<ChatBotMessage>();
            _lexSessionData = _userHttpSession.Get<Dictionary<string, string>>(_botAtrribsKey) ?? new Dictionary<string, string>();
            _currentSlot = _userHttpSession.GetString(_botCurrentSlotKey);
            if (!string.IsNullOrEmpty(_currentSlot))
            {
                _currentSlot = _currentSlot.Replace("\"", "");
            }

            if (string.IsNullOrEmpty(userMessage)) return View(nameof(Index), _botMessages);

            _botMessages.Add(new ChatBotMessage()
            { MsgType = MessageType.UserMessage, ChatMessage = userMessage });

            await PostUserData(_botMessages);

            if(_currentSlot == MAIL_LIST_SLOT)
            {
                if (userMessage.Contains("|"))
                {
                    userMessage = userMessage.Substring(0, userMessage.IndexOf("|"));
                }
                var list = await _context.MailLists.Select(m => m.Name).ToListAsync();
                var delimitedList = string.Join(";", list);
                userMessage = $"{userMessage}|{delimitedList}";
            }

            var lexResponse = await _awsLexService.SendTextMsgToLex(userMessage, _lexSessionData, _userSessionID);

            _lexSessionData = lexResponse.SessionAttributes;
            AssignmentDTO assignmentFromLex = new AssignmentDTO();
            var currentObjectJson = _lexSessionData.GetValueOrDefault("CURRENT_OBJECT");
            if (!string.IsNullOrEmpty(currentObjectJson))
            {
                assignmentFromLex = DeserializeObject<AssignmentDTO>(currentObjectJson);
            }
            _botMessages.Add(new ChatBotMessage()
            { MsgType = MessageType.LexMessage, ChatMessage = lexResponse.Message });


            if (!string.IsNullOrEmpty(_lexSessionData.GetValueOrDefault("DONE")))
            {
                if(_lexSessionData.GetValueOrDefault("DONE") == "TRUE")
                {
                    await ScheduleAssignmentAsync(assignmentFromLex);
                }
            }

            _userHttpSession.Set(_botMsgKey, _botMessages);
            _userHttpSession.Set(_botAtrribsKey, _lexSessionData);
            _userHttpSession.Set(_botCurrentSlotKey, lexResponse.SlotToElicit);

            return View(nameof(Index), _botMessages);
        }

        private async Task ScheduleAssignmentAsync(AssignmentDTO assignmentFromLex)
        {
            var mailList = await _context.MailLists
                .Where(m => m.Name.Contains(assignmentFromLex.MailList))
                .Include(m => m.Emails)
                .FirstOrDefaultAsync();

            foreach (var email in mailList.Emails)
            {
                await _emailSender.SendEmailAsync(email.Address, $"{assignmentFromLex.Title} {DateTime.Now.ToString()}", assignmentFromLex.Description);
            }

            var assignment = new Assignment
            {
                Title = assignmentFromLex.Title,
                Description = assignmentFromLex.Description,
                DueDate = DateTime.Parse(assignmentFromLex.DueDate),
                MailList = mailList.Id,
                OwnerEmail = User.FindFirst(ClaimTypes.Name).Value
            };
            _context.Add(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> PostUserData(List<ChatBotMessage> messages)
        {
            return await Task.Run(() => Index(messages));
        }

        protected T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}