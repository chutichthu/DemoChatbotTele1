using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using DEMO_CHATBOT_TELEGRAM.Services;
using System.Xml.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json.Linq;

public class PrivateChatBot

{
    private  ITelegramBotClient _botClient;
    private  PrivateChatBotService _botChatService;
    CancellationTokenSource _cts;
    string _token;


    public PrivateChatBot(string token, CancellationTokenSource cts)
    {
        _token = token;
        _cts = cts;
    }

    public async Task MainMethod()
    {
        // Khởi tạo bot với token
        _botClient = new TelegramBotClient(_token);
        _botChatService = new PrivateChatBotService(_botClient); ; // Chỉ khởi tạo một lần

        var cancellationToken = _cts.Token;
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Nhận tất cả các loại cập nhật
        };

        // Bắt đầu nhận tin nhắn và xử lý
        _botClient.StartReceiving(
           updateHandler: HandleUpdateAsync,
           pollingErrorHandler: HandleErrorAsync,
           receiverOptions: receiverOptions,
           cancellationToken: cancellationToken
       );
        var me = await _botClient.GetMeAsync();
        Console.WriteLine(me.FirstName);
    }


    // Hàm xử lý khi nhận tin nhắn
    private readonly Dictionary<long, string> userInputs = new();
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Kiểm tra xem update có chứa tin nhắn không
        if (update.Type == UpdateType.Message && update.Message is { } message)
        {
            var pollAnswer = update.PollAnswer;
            
            // Kiểm tra loại chat là riêng tư
            if (message.Chat.Type == ChatType.Private)
                {
                    if (message.Text == "/start")
                    {
                       // await _botChatService.LoiChao(botClient, message.Chat.Id, cancellationToken);
                        userInputs[message.Chat.Id] = ""; // Khởi tạo đầu vào của người dùng

                        await botClient.SendTextMessageAsync(message.Chat.Id, "Vui lòng nhập tên của bạn:", cancellationToken: cancellationToken);
                    }
                    else if (userInputs.ContainsKey(message.Chat.Id))
                    {

                        if (string.IsNullOrEmpty(userInputs[message.Chat.Id])) // Chưa nhập tên
                        {
                            userInputs[message.Chat.Id] = message.Text; // Lưu tên
                            await _botChatService.HandleUserNameInput(botClient, message.Chat.Id, message.Text!, cancellationToken);
                           // await botClient.SendTextMessageAsync(message.Chat.Id, "Cảm ơn! Bây giờ, vui lòng nhập số điện thoại của bạn:", cancellationToken: cancellationToken);
                        }
                        else // Đã nhập tên, yêu cầu nhập số điện thoại
                        {
                            string userName = userInputs[message.Chat.Id]; // Lấy tên người dùng đã lưu
                            string userPhone = message.Text; // Lấy số điện thoại nhập vào
                            await _botChatService.HandleUserPhoneInput(botClient, message.Chat.Id, userPhone, userName, cancellationToken);
                            userInputs.Remove(message.Chat.Id); // Xóa đầu vào người dùng sau khi lưu

                            // Gửi lời chào sau khi nhập tên và số điện thoại
                            await _botChatService.LoiChao(botClient, message.Chat.Id, cancellationToken);
                        }
                    }
                    else
                    {
                        // Xử lý các tin nhắn khác
                        await _botChatService.HandleTravelInquiry(botClient, message.Chat.Id, message.Text, cancellationToken);





                        //switch (message.Text)
                        //{
                        //    case "/start":

                        //        //await _botChatService.LoiChao(botClient, message.Chat.Id, cancellationToken);

                        //        //await _botChatService.HandleNewAccount(botClient, message.Chat.Id, cancellationToken);
                        //        //userInputs.Add(message.Chat.Id, ""); // Khởi tạo đầu vào của người dùng

                        //        //await _botChatService.SendConsultationInquiry(_botClient, message.Chat.Id, cancellationToken);

                        //        break;
                        //    case "/newaccount":
                        //        await _botChatService.HandleNewAccount(botClient, message.Chat.Id, cancellationToken);
                        //        userInputs.Add(message.Chat.Id, ""); // Khởi tạo đầu vào của người dùng
                        //        break;

                        //    default:

                        //        // Gọi phương thức tư vấn du lịch nếu có yêu cầu
                        //        string userMessage = update.Message.Text!;
                        //        await _botChatService.HandleTravelInquiry(_botClient, message.Chat.Id, userMessage, cancellationToken);
                        //        // Kiểm tra xem người dùng có đang trong quá trình tạo tài khoản mới không
                        //        if (userInputs.ContainsKey(message.Chat.Id))
                        //        {
                        //            if (string.IsNullOrEmpty(userInputs[message.Chat.Id])) // Nhập tên
                        //            {
                        //                userInputs[message.Chat.Id] = message.Text!; // Lưu tên XONG TRA VE ID
                        //                await _botChatService.HandleUserNameInput(botClient, message.Chat.Id, message.Text!, cancellationToken);
                        //            }
                        //            else // Nhập số điện thoại
                        //            {
                        //                string userName = userInputs[message.Chat.Id]; // Lấy tên người dùng đã lưu
                        //                string userPhone = message.Text; // Lấy số điện thoại nhập vào

                        //                // KHI CAP SO PHONE THI DUA THEO ID
                        //                await _botChatService.HandleUserPhoneInput(botClient, message.Chat.Id, userPhone, userName, cancellationToken);
                        //               //userInputs.Remove(message.Chat.Id); // Xóa đầu vào người dùng sau khi lưu
                        //            }

                        //        }
                        //        break;
                    }
                }

                if (message.Chat.Type == ChatType.Channel)
                {

                }
                if (message.Chat.Type == ChatType.Group)
                {
                }
                if (message.Chat.Type == ChatType.Supergroup)
                {

                }
                if (message.Chat.Type == ChatType.Sender)
                {

                }



            
           
        }
        // Kiểm tra nếu có callback query
        else if (update.CallbackQuery != null)
        {
            await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
            return;
        }
        // Kiểm tra nếu có phản hồi từ poll
        else if (update.Type == UpdateType.PollAnswer)
        {
            var pollAnswer = update.PollAnswer;
            await HandlePollResponse(botClient, pollAnswer!);
        }
    }

    private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        long chatId = callbackQuery.Message.Chat.Id;
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id); // Phản hồi cho callback query
        
        var callbackData = callbackQuery.Data;

        // Xử lý các nút được nhấn
        switch (callbackQuery.Data)
        {
            case "traveltovietnam":
                await _botChatService.HandleTravelToVietnam(botClient, chatId);
                
                break;
            case "goto_doctor":
                await _botChatService.HandleGoToDoctor(botClient, chatId);
                break;
            case "business":
                await _botChatService.HandleBusiness(botClient, chatId);
                break;
            case "other":
                await _botChatService.HandleOther(botClient, chatId);
                break;
            case "vietnam_info":

                await botClient.SendTextMessageAsync(chatId, "Here is some information about VietNam...", cancellationToken: cancellationToken);
                await SendInfoAndPoll(botClient, chatId, "Here is some information about traveling to Vietnam...");
                break;
            case "doctor_info":
                await botClient.SendTextMessageAsync(chatId, "Here is some information about hospitals in VietNam...", cancellationToken: cancellationToken);
                await SendInfoAndPoll(botClient, chatId, "Here is some information about hospitals...");
                
                break;
            case "business_info":
                await botClient.SendTextMessageAsync(chatId, "Here is some information about business opportunities in VietNam...", cancellationToken: cancellationToken);
                await SendInfoAndPoll(botClient, chatId, "Here is some information about business...");
                break;
            case "back_to_main":
                await _botChatService.LoiChao(botClient, chatId, cancellationToken);
                break;
            default:
                // Không cần phản hồi cho trường hợp không khớp
                break;
        }
    }
    private async Task SendInfoAndPoll(ITelegramBotClient botClient, long chatId, string infoMessage)
    {
        await botClient.SendTextMessageAsync(chatId, infoMessage);

        var options = new[] { "Hài lòng", "Không hài lòng" };
        await botClient.SendPollAsync(chatId, "Bạn có hài lòng với thông tin không?", options);

       // await botClient.StopPollAsync(botClient, chatId);

    }

      // https: //gist.github.com/Muaath5/22d39734ac0bc5b9f9ad6b66a97e064c
    private async Task HandlePollResponse(ITelegramBotClient botClient, PollAnswer pollAnswer)
    {
        
        // Kiểm tra phản hồi từ poll
        if (pollAnswer.OptionIds.Contains(1)) // "Không hài lòng" có id là 1
        {
            var replyMessage = "Xin lỗi vì bạn không hài lòng. Vui lòng liên hệ với chúng tôi qua số điện thoại sau:";
            var phoneButton = InlineKeyboardButton.WithUrl("Gọi ngay", "tel:+123456789");
            var keyboard = new InlineKeyboardMarkup(new[] { new[] { phoneButton } });
           
            // Gửi tin nhắn đến người dùng
            await botClient.SendTextMessageAsync(pollAnswer.User.Id, replyMessage, replyMarkup: keyboard);
        }
    }


    // Hàm xử lý lỗi
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}