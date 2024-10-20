using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DEMO_CHATBOT_TELEGRAM.Services
{
    internal class PrivateChatBotService // khuon mau
    {
        private readonly ITelegramBotClient _botClient;
        
        private string sqlConnectStr = "Data Source=DESKTOP-7TOH8P9\\SQLEXPRESS;Initial Catalog=chatbotlehuuthu;User ID=sa;Password=Admin@1";

  
        InlineKeyboardMarkup inlineKeyboard = CreateMainMenu();

        public PrivateChatBotService(ITelegramBotClient botClient)
        {
            _botClient = botClient; // Lưu botClient vào biến trường
        }



        public async Task LoiChao(ITelegramBotClient botClient, long id, CancellationToken cancellationToken)
        {

            await botClient.SendTextMessageAsync(id, " Hello! Welcome to GOX ChatBot, a system specifically designed to assist tourists in Asia & VietNam. \n \nHow can I assist you?", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync(id,
                "Bạn đang cần tư vấn về địa điểm du lịch, khám sức khoẻ, làm đẹp, kinh doanh?",
                cancellationToken: cancellationToken);

        }

        private static InlineKeyboardMarkup CreateMainMenu()
        {
            return new InlineKeyboardMarkup(new InlineKeyboardButton[4][]
            {
        new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Travel to VietNam", "traveltovietnam") },
        new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Go to Doctor/Hospital", "goto_doctor") },
        new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Business", "business") },
        new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Other...", "other") }
            });
        }


        private int userID;

        // Lưu tài khoản
        // Thêm một từ điển để theo dõi trạng thái đầu vào của người dùng
        public static Dictionary<long, int> userInputStep = new();


        //public async Task HandleNewAccount(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        //{
        //    userInputStep[chatId] = 1; // Đặt bước hiện tại là 1 (nhập tên)
        //    await botClient.SendTextMessageAsync(chatId, "Bạn đã dùng GOX chưa cho tôi xin tên đăng ký của bạn:", cancellationToken: cancellationToken);
        //}

        public async Task HandleUserNameInput(ITelegramBotClient botClient, long chatId, string userName, CancellationToken cancellationToken)
        {
            userID = SaveUserName(chatId, userName); // Lưu tên vào cơ sở dữ liệu
            userInputStep[chatId] = 2; // Chuyển sang bước tiếp theo (nhập số điện thoại)
            await botClient.SendTextMessageAsync(chatId, " Vui lòng nhập số điện thoại của bạn :", cancellationToken: cancellationToken);
        }

        private  int SaveUserName(long chatId, string userName)
        {
            using (var con = new SqlConnection(sqlConnectStr))
            {
                int newID;
                var cmd = "INSERT INTO Users (Name) VALUES (@Name);SELECT CAST(scope_identity() AS int)";
                using (var insertCommand = new SqlCommand(cmd, con))
                {
                    insertCommand.Parameters.AddWithValue("@Name", userName);
                    con.Open();
                    newID = (int)insertCommand.ExecuteScalar();
                    return newID;
                }
            }

            //string sqlInsert = "INSERT INTO Users (Name) VALUES (@Name)"; // Nếu không cần ChatId
            //using SqlConnection connection = new(sqlConnectStr);
            //await connection.OpenAsync();

            //using SqlCommand command = new(sqlInsert, connection);
            //command.Parameters.AddWithValue("@Name", userName);
            //await command.ExecuteNonQueryAsync();
        }


        public async Task HandleUserPhoneInput(ITelegramBotClient botClient, long chatId, string userPhone, string userName, CancellationToken cancellationToken)
        {
            // Kiểm tra nếu số điện thoại không rỗng
            if (!string.IsNullOrWhiteSpace(userPhone))
            {
                // Lưu số điện thoại vào cơ sở dữ liệu
                await SaveUserPhone(userName, userPhone);
            }

            // Xóa trạng thái sau khi hoàn thành
            userInputStep.Remove(chatId);

            // Gửi tin nhắn cảm ơn
            await botClient.SendTextMessageAsync(chatId, "Cảm ơn bạn đã đăng ký!", cancellationToken: cancellationToken);

            // Gọi phương thức gửi câu hỏi tư vấn
            //await SendConsultationInquiry(botClient, chatId,  cancellationToken);
        }


        private async Task SaveUserPhone(string userName, string userPhone)
        {
           

            using SqlConnection connection = new(sqlConnectStr);
           
            // Câu lệnh SQL để cập nhật số điện thoại
            string sqlUpdate = "UPDATE Users set Phone = @Phone WHERE id=@userID";
            await connection.OpenAsync();
            using SqlCommand command = new(sqlUpdate, connection);
            command.Parameters.AddWithValue("@Phone", userPhone);
             command.Parameters.AddWithValue("@userID", userID);

            // Thực hiện cập nhật
            await command.ExecuteNonQueryAsync();
        }

        //// Tạo một từ điển để theo dõi trạng thái hỏi của người dùng
        //private Dictionary<long, bool> userInquiries = new Dictionary<long, bool>();

        //public async Task SendConsultationInquiry(ITelegramBotClient botClient, long chatId, string userMessage, CancellationToken cancellationToken)
        //{
        //    // Chuyển đổi tin nhắn thành chữ thường để dễ kiểm tra
        //    var lowerMessage = userMessage.ToLower();

        //    // Kiểm tra xem người dùng đã hỏi về du lịch chưa
        //    if (userInquiries.ContainsKey(chatId) && userInquiries[chatId])
        //    {
        //        if (lowerMessage.Contains("du lịch đà nẵng"))
        //        {
        //            await botClient.SendTextMessageAsync(chatId,
        //                "Dưới đây là một số địa điểm du lịch nổi tiếng ở Đà Nẵng:\n" +
        //                "1. Bãi biển Mỹ Khê\n" +
        //                "2. Ngũ Hành Sơn\n" +
        //                "3. Cầu Rồng\n" +
        //                "4. Bà Nà Hills\n" +
        //                "5. Chùa Linh Ứng\n" +
        //                "6. Sơn Trà\n" +
        //                "7. Phố cổ Hội An (gần đó)\n" +
        //                "Bạn có muốn tìm hiểu thêm về địa điểm nào không?", cancellationToken: cancellationToken);
        //        }
        //        else
        //        {
        //            await botClient.SendTextMessageAsync(chatId, "Xin lỗi, tôi chỉ có thông tin về Đà Nẵng. Bạn có muốn hỏi gì về thành phố này không?", cancellationToken: cancellationToken);
        //        }
        //    }
        //    else if (lowerMessage.Contains("du lịch") ||
        //             lowerMessage.Contains("tour") ||
        //             lowerMessage.Contains("địa điểm du lịch"))
        //    {
        //        // Đánh dấu người dùng đã hỏi về du lịch
        //        userInquiries[chatId] = true;

        //        await botClient.SendTextMessageAsync(chatId, "Bạn đang cần tư vấn về địa điểm du lịch, khám sức khoẻ, làm đẹp, kinh doanh? Vui lòng cho tôi biết.", cancellationToken: cancellationToken);
        //    }
        //    else
        //    {
        //        await botClient.SendTextMessageAsync(chatId, "Xin lỗi, tôi không hiểu yêu cầu của bạn. Vui lòng hỏi về thông tin du lịch.", cancellationToken: cancellationToken);
        //    }
        //}

        //public async Task SendConsultationInquiry(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        //{
        //    await botClient.SendTextMessageAsync(chatId, "Bạn đang cần tư vấn về địa điểm du lịch, khám sức khoẻ, làm đẹp, kinh doanh?", cancellationToken: cancellationToken);
        //}


        public async Task HandleTravelInquiry(ITelegramBotClient botClient, long chatId, string userMessage, CancellationToken cancellationToken)
        {
            // AI de su ly
            // Kiểm tra xem tin nhắn có chứa từ khóa du lịch không
            //if (userMessage.ToLower().Contains("du lịch") || userMessage.ToLower().Contains("tour") || userMessage.ToLower().Contains("Da Nang")
            //    || userMessage.ToLower().Contains("địa điểm du lịch"))
            if (userMessage.ToLower().Contains("Da Nang"))
            {
                await botClient.SendTextMessageAsync(chatId, "Duoi day la danh sach nhung noi de di o da nang, sau AI se su ly", cancellationToken: cancellationToken);
                // them cac botton hoac link
            }else if(userMessage.ToLower().Contains("Ho Chi Minh"))
            {

            }

            else
            {
                await botClient.SendTextMessageAsync(chatId, "Xin lỗi, tôi không hiểu yêu cầu của bạn. Vui lòng hỏi về thông tin du lịch.", cancellationToken: cancellationToken);
            }
        }


        // Menu Travel
        public async Task HandleTravelToVietnam(ITelegramBotClient botClient, long chatId)
           => await SendResponse(botClient, chatId, "You selected Travel to VietNam.", TravelToVietNamMenu());

        private static InlineKeyboardMarkup TravelToVietNamMenu() => new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Information about VietNam", "vietnam_info") },
            new[] { InlineKeyboardButton.WithCallbackData("Back to Main Menu", "back_to_main") }
        });

        public async Task HandleGoToDoctor(ITelegramBotClient botClient, long chatId)
            => await SendResponse(botClient, chatId, "You selected Go to Doctor/Hospital.", GoToDoctorMenu());

        private static InlineKeyboardMarkup GoToDoctorMenu() => new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Information about  Go to Doctor/Hospital", "doctor_info") },
            new[] { InlineKeyboardButton.WithCallbackData("Back to Main Menu", "back_to_main") }
        });

        public async Task HandleBusiness(ITelegramBotClient botClient, long chatId)
            => await SendResponse(botClient, chatId, "You selected Business.", BusinessMenu());

        private static InlineKeyboardMarkup BusinessMenu() => new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Information about Business", "business_info") },
            new[] { InlineKeyboardButton.WithCallbackData("Back to Main Menu", "back_to_main") }
        });

        public async Task HandleOther(ITelegramBotClient botClient, long chatId)
            => await SendResponse(botClient, chatId, "You selected Other.", OtherMenu());

        private static InlineKeyboardMarkup OtherMenu() => new InlineKeyboardMarkup(new[]
        {
           // new[] { InlineKeyboardButton.WithCallbackData("Information about VietNam", "vietnam_info") },
            new[] { InlineKeyboardButton.WithCallbackData("Back to Main Menu", "back_to_main") }
        });

        private async Task SendResponse(ITelegramBotClient botClient, long chatId, string message, InlineKeyboardMarkup replyMarkup)
            => await botClient.SendTextMessageAsync(chatId, message, replyMarkup: replyMarkup);
    }



  


}
