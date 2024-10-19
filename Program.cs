using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;


class Program
{
    static async Task Main(string[] args)
    {
        // Token của bot
        string token = "8154829637:AAGKzr5x20dt_8eLCkzEwSgrRrm_Ye50NPk";

        // Khởi tạo CancellationTokenSource
        CancellationTokenSource cts = new CancellationTokenSource();

        // Khởi tạo bot cho các loại chat
        PrivateChatBot privateChatBot = new PrivateChatBot(token, cts);

        await privateChatBot.MainMethod();

        // Bắt đầu nhận cập nhật
        Console.WriteLine("Bot started. Press any key to exit...");

        // Đợi người dùng nhấn phím
        Console.ReadLine();

        // Hủy bỏ nhận cập nhật
        cts.Cancel();
    }

}
