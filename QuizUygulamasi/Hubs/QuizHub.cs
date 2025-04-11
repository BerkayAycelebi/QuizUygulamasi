using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizUygulamasi.Data;
using QuizUygulamasi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QuizUygulamasi.Models.ErrorViewModel;

namespace QuizUygulamasi.Hubs
{
    public class QuizHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private static Dictionary<string, string> _userConnections = new Dictionary<string, string>(); // KullanıcıId - ConnectionId
        private static Dictionary<string, List<string>> _roomParticipants = new Dictionary<string, List<string>>(); // RoomCode - ConnectionIds
        private static Dictionary<string, Participant> _participants = new Dictionary<string, Participant>(); // ConnectionId - Participant
        private static Dictionary<string, QuizRoom> _activeRooms = new Dictionary<string, QuizRoom>(); // RoomCode - QuizRoom
        private static Dictionary<string, Dictionary<string, string>> _participantAnswers = new Dictionary<string, Dictionary<string, string>>(); // RoomCode - (ConnectionId - Answer)
        private static Dictionary<string, DateTime> _answerSubmissionTimes = new Dictionary<string, DateTime>(); // ConnectionId - Submission Time

        public QuizHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier; // Kimlik doğrulama kullanıyorsanız
            string connectionId = Context.ConnectionId;

            // Eğer kimlik doğrulama yoksa geçici bir ID oluşturabilirsiniz.
            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
            }

            if (!_userConnections.ContainsKey(userId))
            {
                _userConnections.Add(userId, connectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;

            var participantToRemove = _participants.FirstOrDefault(p => p.Key == connectionId).Value;
            if (participantToRemove != null)
            {
                var roomCode = _activeRooms.FirstOrDefault(r => r.Value.Id == participantToRemove.QuizRoomId).Key;
                if (_roomParticipants.ContainsKey(roomCode))
                {
                    _roomParticipants[roomCode].Remove(connectionId);
                    await Clients.Group(roomCode).SendAsync("ParticipantLeft", participantToRemove.Username, _roomParticipants[roomCode].Count);
                    _participants.Remove(connectionId);
                    if (_activeRooms.ContainsKey(roomCode))
                    {
                        _activeRooms[roomCode].ParticipantCount = _roomParticipants[roomCode].Count;
                        // Belki oda sahibi ayrıldığında odayı kapatma gibi bir mantık eklenebilir.
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomCode, string email)
        {
            roomCode = roomCode.ToUpper();
            var connectionId = Context.ConnectionId;

            var room = await _context.QuizRooms
                .Include(r => r.Quiz)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode && r.IsActive == false);

            if (room != null)
            {
                if (!_roomParticipants.ContainsKey(roomCode))
                {
                    _roomParticipants.Add(roomCode, new List<string>());
                }

                if (!_roomParticipants[roomCode].Contains(connectionId))
                {
                    _roomParticipants[roomCode].Add(connectionId);

                    Participant participant = new Participant
                    {
                        QuizRoomId = room.Id,
                        Email = email,
                        Username = GenerateRandomUsername() // Otomatik kullanıcı adı atama
                        
                    };

                    _participants.Add(connectionId, participant);
                    await Groups.AddToGroupAsync(connectionId, roomCode);
                    await Clients.Caller.SendAsync("JoinedRoom", participant.Username);
                    await Clients.Group(roomCode).SendAsync("ParticipantJoined", participant.Username, _roomParticipants[roomCode].Count);

                    room.ParticipantCount = _roomParticipants[roomCode].Count;
                    if (!_activeRooms.ContainsKey(roomCode))
                    {
                        _activeRooms.Add(roomCode, room);
                    }
                    else
                    {
                        _activeRooms[roomCode] = room;
                    }

                    if (room.ParticipantCount >= 1) // Başlat butonu için en az 1 katılımcı
                    {
                        await Clients.Group(roomCode).SendAsync("EnableStartButton");
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Bu odaya zaten katıldınız.");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Geçersiz oda kodu.");
            }
        }

        public async Task StartQuiz(string roomCode)
        {
            roomCode = roomCode.ToUpper();
            if (_activeRooms.ContainsKey(roomCode) && _roomParticipants[roomCode].Count > 0)
            {
                var room = _activeRooms[roomCode];
                room.IsActive = true;
                room.CurrentQuestionIndex = 0;
                room.QuestionStartTime = DateTime.Now;

                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == room.QuizId);

                if (quiz != null && quiz.Questions.Count > 0)
                {
                    _participantAnswers[roomCode] = new Dictionary<string, string>(); // Cevapları sıfırla
                    var firstQuestion = quiz.Questions.OrderBy(q => q.Id).Skip(room.CurrentQuestionIndex).First();
                    await Clients.Group(roomCode).SendAsync("QuizStarted", firstQuestion.Text, firstQuestion.OptionA, firstQuestion.OptionB, firstQuestion.OptionC, firstQuestion.OptionD, room.CurrentQuestionIndex + 1, quiz.Questions.Count);
                    await StartQuestionTimer(roomCode);
                }
                else
                {
                    await Clients.Group(roomCode).SendAsync("Error", "Bu quizde soru bulunmamaktadır.");
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task SubmitAnswer(string roomCode, string answer)
        {
            var connectionId = Context.ConnectionId;
            roomCode = roomCode.ToUpper();

            if (_activeRooms.ContainsKey(roomCode) && _activeRooms[roomCode].IsActive && _participants.ContainsKey(connectionId))
            {
                if (!_participantAnswers[roomCode].ContainsKey(connectionId))
                {
                    _participantAnswers[roomCode].Add(connectionId, answer);
                    _answerSubmissionTimes[connectionId] = DateTime.Now;
                    await Clients.Caller.SendAsync("AnswerSubmitted");
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Bu soru için zaten cevap verdiniz.");
                }
            }
        }

        private async Task StartQuestionTimer(string roomCode)
        {
            await Task.Delay(30000); // 30 saniye

            if (_activeRooms.ContainsKey(roomCode) && _activeRooms[roomCode].IsActive)
            {
                var room = _activeRooms[roomCode];
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == room.QuizId);

                if (quiz != null && quiz.Questions.Count > room.CurrentQuestionIndex)
                {
                    var currentQuestion = quiz.Questions.OrderBy(q => q.Id).Skip(room.CurrentQuestionIndex).First();
                    await RevealCorrectAnswerAndShowLeaderboard(roomCode, currentQuestion.CorrectAnswer);

                    room.CurrentQuestionIndex++;
                    if (room.CurrentQuestionIndex < quiz.Questions.Count)
                    {
                        await Task.Delay(5000); // Cevap gösterildikten sonra 5 saniye bekle
                        var nextQuestion = quiz.Questions.OrderBy(q => q.Id).Skip(room.CurrentQuestionIndex).First();
                        room.QuestionStartTime = DateTime.Now;
                        _participantAnswers[roomCode] = new Dictionary<string, string>(); // Yeni soru için cevapları sıfırla
                        _answerSubmissionTimes.Clear();
                        await Clients.Group(roomCode).SendAsync("NextQuestion", nextQuestion.Text, nextQuestion.OptionA, nextQuestion.OptionB, nextQuestion.OptionC, nextQuestion.OptionD, room.CurrentQuestionIndex + 1, quiz.Questions.Count);
                        await StartQuestionTimer(roomCode);
                    }
                    else
                    {
                        await Task.Delay(5000); // Son cevap gösterildikten sonra 5 saniye bekle
                        await ShowFinalLeaderboard(roomCode);
                        room.IsActive = false;
                    }
                }
            }
        }

        private async Task RevealCorrectAnswerAndShowLeaderboard(string roomCode, string correctAnswer)
        {
            await Clients.Group(roomCode).SendAsync("CorrectAnswerRevealed", correctAnswer);
            await UpdateScoresAndShowLeaderboard(roomCode);
        }

        private async Task UpdateScoresAndShowLeaderboard(string roomCode)
        {
            if (_activeRooms.ContainsKey(roomCode) && _participantAnswers.ContainsKey(roomCode))
            {
                var room = _activeRooms[roomCode];
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == room.QuizId);
                var currentQuestion = quiz.Questions.OrderBy(q => q.Id).Skip(room.CurrentQuestionIndex).First();

                foreach (var participantAnswer in _participantAnswers[roomCode])
                {
                    if (_participants.ContainsKey(participantAnswer.Key) && participantAnswer.Value.ToUpper() == currentQuestion.CorrectAnswer.ToUpper())
                    {
                        var submissionTime = _answerSubmissionTimes.ContainsKey(participantAnswer.Key) ? _answerSubmissionTimes[participantAnswer.Key] : DateTime.Now;
                        var timeTaken = submissionTime - room.QuestionStartTime;
                        int points = 1000 - (int)timeTaken.TotalMilliseconds;
                        if (points < 100) points = 100; // Minimum puan

                        var participant = _participants[participantAnswer.Key];
                        participant.Score += points;
                    }
                }

                var leaderboard = _participants.Where(p => p.Value.QuizRoomId == room.Id)
                    .OrderByDescending(p => p.Value.Score)
                    .Take(5)
                    .Select(p => new { p.Value.Username, p.Value.Score })
                    .ToList();

                await Clients.Group(roomCode).SendAsync("ShowLeaderboard", leaderboard);
            }
        }

        private async Task ShowFinalLeaderboard(string roomCode)
        {
            if (_activeRooms.ContainsKey(roomCode))
            {
                var room = _activeRooms[roomCode];
                var leaderboard = _participants.Where(p => p.Value.QuizRoomId == room.Id)
                    .OrderByDescending(p => p.Value.Score)
                    .Select(p => new { p.Value.Username, p.Value.Score })
                    .ToList();

                await Clients.Group(roomCode).SendAsync("ShowFinalLeaderboard", leaderboard);
            }
        }

        private string GenerateRandomUsername()
        {
            return "Yarışmacı_" + Guid.NewGuid().ToString().Substring(0, 8);
        }

        public async Task<string> GenerateRoomCode(int quizId)
        {
            var newRoom = new QuizRoom { QuizId = quizId };
            _context.QuizRooms.Add(newRoom);
            await _context.SaveChangesAsync();
            return newRoom.RoomCode;
        }
    }
}