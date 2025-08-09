public class TelegramService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly HttpClient _httpClient;

    public TelegramService(ILogger<TelegramService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> SendMessageAsync(string botToken, string chatId, string message)
    {
        var url = $"https://api.telegram.org/{botToken}/sendMessage";

        var payload = new Dictionary<string, string>
        {
            { "chat_id", chatId },
            { "text", message }
        };

        try
        {
            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(payload));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ошибка Telegram API. StatusCode: {StatusCode}, Content: {Content}",
                                 response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Сообщение отправлено в Telegram. ChatId: {ChatId}, Text: {Text}",
                                   chatId, message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке сообщения в Telegram. ChatId: {ChatId}", chatId);
            return false;
        }
    }
}
