using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartYouTubeSummarizer.Services
{
    public class AiService : IAiService 
    {
        private readonly string _endpoint;

        public AiService(IConfiguration configuration)
        {
            string _apiKey = configuration["ApiSettings:GeminiApiKey"];
            _endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent?key={_apiKey}";
        }

        public async Task<string> SummarizeTextAsync(string text, string lengthOption)
        {
            string lengthInstruction = "";

            // İstifadəçinin yazdığı və ya seçdiyi mətni təmizləyirik (Məs: "3 Maddə" -> "3")
            string cleanInput = lengthOption.Replace("Maddə", "").Trim();

            // Əgər istifadəçi bura sırf rəqəm daxil edibsə (Məsələn: 2, 5, 15)
            if (int.TryParse(cleanInput, out int itemCount))
            {
                lengthInstruction = $"Mətndən çıxarılan ən kritik və vacib {itemCount} məqamı dəqiq {itemCount} maddə halında ardıcıl olaraq Azərbaycan dilində yaz. Nə bir əskik, nə bir artıq maddə olmasın.";
            }
            else if (lengthOption.Contains("Geniş Hesabat"))
            {
                lengthInstruction = "Mətni mükəmməl dərəcədə ətraflı analiz et, bütün alt başlıqları və mühüm detalları qoruyaraq geniş bir konspekt hesabatı halında Azərbaycan dilində yaz.";
            }
            else // Hər hansı bir xətalı yazı yazılarsa default olaraq 10 maddə edirik
            {
                lengthInstruction = "Mətndən çıxarılan en vacib 10 məqamı ardıcıl maddələr halında Azərbaycan dilində yaz.";
            }

            // Əsas System Prompt təlimatı
            string systemPrompt = $"Sən peşəkar bir video analiz köməkçisisən. Sənə təqdim olunan mətni analiz etməlisən.\n\n" +
                                  $"[TƏLİMAT]: {lengthInstruction}\n\n" +
                                  $"[ANALİZ OLUNACAQ MƏTN]:\n{text}";

            var requestBody = new { contents = new[] { new { parts = new[] { new { text = systemPrompt } } } } };
            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(requestBody);

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_endpoint, content);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();

                using (System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(jsonResponse))
                {
                    return doc.RootElement.GetProperty("candidates")[0]
                                          .GetProperty("content")
                                          .GetProperty("parts")[0]
                                          .GetProperty("text")
                                          .GetString();
                }
            }
        }

        public async Task<string> AskQuestionAboutVideoAsync(string videoTranscript, string userQuestion)
        {
            string systemPrompt = $"Sən köməkçi bir AI-sən. Aşağıda sənə bir YouTube videosunun mətni verilmişdir. " +
                           $"Sən İSTİFADƏÇİNİN SUALINI YALNIZ VƏ YALNIZ BU MƏTNƏ ƏSASƏN CAVABLANDIRMALISAN. Kənara çıxma. " +
                           $"Cavabını səliqəli və Azərbaycan dilində yaz.\n\n" +
                           $"[VİDEO MƏTNİ]:\n{videoTranscript}\n\n" +
                           $"[SUAL]: {userQuestion}";

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = systemPrompt }
                }
            }
        }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");


                    var response = await httpClient.PostAsync(_endpoint, content);
                    response.EnsureSuccessStatusCode();


                    string jsonResponse = await response.Content.ReadAsStringAsync();


                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        var root = doc.RootElement;
                        string aiText = root.GetProperty("candidates")[0]
                                            .GetProperty("content")
                                            .GetProperty("parts")[0]
                                            .GetProperty("text")
                                            .GetString();

                        return aiText;
                    }
                }
                catch (Exception ex)
                {
                    return $"AI Sorğusunda xəta baş verdi: {ex.Message}";
                }
            }
        }
    }
}