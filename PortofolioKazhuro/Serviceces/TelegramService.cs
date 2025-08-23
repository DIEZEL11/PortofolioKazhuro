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

    public async Task<bool> SendDocumentAsync(string botToken, string chatId, Stream fileStream, string fileName, string caption = null)
    {
        var url = $"https://api.telegram.org/{botToken}/sendDocument";
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(chatId), "chat_id");
        if (!string.IsNullOrEmpty(caption))
            form.Add(new StringContent(caption), "caption");
        form.Add(new StreamContent(fileStream), "document", fileName);

        try
        {
            var response = await _httpClient.PostAsync(url, form);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ошибка Telegram API при отправке файла. StatusCode: {StatusCode}, Content: {Content}",
                                 response.StatusCode, errorContent);
                return false;
            }
            _logger.LogInformation("Файл успешно отправлен в Telegram. ChatId: {ChatId}, FileName: {FileName}", chatId, fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при отправке файла в Telegram. ChatId: {ChatId}", chatId);
            return false;
        }

    }
}
