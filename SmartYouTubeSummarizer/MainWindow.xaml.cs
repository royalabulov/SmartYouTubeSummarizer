using SmartYouTubeSummarizer.Models;
using SmartYouTubeSummarizer.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartYouTubeSummarizer
{
    public partial class MainWindow : Window
    {
        private readonly IYouTubeService _youTubeService;
        private readonly IAiService _aiService;
        private readonly ISummaryRepository _summaryRepository;

        private string _currentTranscript = "";
        private const string PlaceholderText = "Bu video haqqında sual verin... (Məs: 10-cu dəqiqədə nə deyir?)";

        public ObservableCollection<SummaryHistory> HistoryList { get; set; }

        // SOLID & Dependency Injection Konstruktoru
        public MainWindow(IYouTubeService youTubeService, IAiService aiService, ISummaryRepository summaryRepository)
        {
            InitializeComponent();

            _youTubeService = youTubeService;
            _aiService = aiService;
            _summaryRepository = summaryRepository;

            HistoryList = new ObservableCollection<SummaryHistory>();
            lstHistory.ItemsSource = HistoryList;

            ConfigureEventHandlers();
            ResetUiToDefault();
            LoadHistory();
        }

        private void ConfigureEventHandlers()
        {
            lstHistory.SelectionChanged += LstHistory_SelectionChanged;
            btnNewChat.Click += BtnNewChat_Click;
            txtChatPrompt.GotFocus += txtChatPrompt_GotFocus;
            txtChatPrompt.LostFocus += txtChatPrompt_LostFocus;
            txtChatPrompt.KeyDown += txtChatPrompt_KeyDown;
        }

        private void ResetUiToDefault()
        {
            txtUrl.Clear();
            txtResult.Clear();
            txtChatPrompt.Text = PlaceholderText;
            lstHistory.SelectedIndex = -1;
            webViewBorder.Visibility = Visibility.Collapsed;
            btnOpenYouTube.Visibility = Visibility.Collapsed;
        }

        private void LoadHistory()
        {
            try
            {
                var data = _summaryRepository.GetAllDescending();
                HistoryList.Clear();
                foreach (var item in data)
                {
                    HistoryList.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tarixçə yüklənərkən xəta baş verdi: {ex.Message}", "Xəta", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ExtractVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var youtubeRegex = new Regex(@"youtu(?:\.be|be\.com)\/(?:.*v(?:\/|=)|(?:.*\/)?)([a-zA-Z0-9-_]{11})");
            var match = youtubeRegex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }

        private async Task PlayYouTubeVideoAsync(string url)
        {
            string videoId = ExtractVideoId(url);
            if (!string.IsNullOrEmpty(videoId))
            {
                await webViewVideo.EnsureCoreWebView2Async(null);
                webViewVideo.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
                webViewVideo.CoreWebView2.Navigate($"https://www.youtube.com/watch?v={videoId}");

                webViewBorder.Visibility = Visibility.Visible;
                btnOpenYouTube.Visibility = Visibility.Visible;
            }
            else
            {
                webViewBorder.Visibility = Visibility.Collapsed;
                btnOpenYouTube.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnSummarize_Click(object sender, RoutedEventArgs e)
        {
            string videoUrl = txtUrl.Text;
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                MessageBox.Show("Zəhmət olmasa, YouTube linkini daxil edin.", "Xəbərdarlıq", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                txtResult.Text = "Altyazılar çəkilir, gözləyin...";
                btnSummarize.IsEnabled = false;

                _currentTranscript = await _youTubeService.GetVideoTranscriptAsync(videoUrl);
                txtResult.Text = "Altyazılar uğurla çəkildi. AI mətni analiz edir...";

                string selectedLength = cmbLength.Text;
                string aiSummary = await _aiService.SummarizeTextAsync(_currentTranscript, selectedLength);

                var historyItem = new SummaryHistory
                {
                    VideoUrl = videoUrl,
                    Title = $"YouTube Video ({DateTime.Now:dd.MM.yyyy HH:mm})",
                    SummaryText = aiSummary,
                    CreatedAt = DateTime.Now
                };
                _summaryRepository.Add(historyItem);

                LoadHistory();
                txtResult.Text = aiSummary;
                await PlayYouTubeVideoAsync(videoUrl);

            }
            catch (Exception ex)
            {
                txtResult.Text = $"Xəta baş verdi: {ex.Message}";
            }
            finally
            {
                btnSummarize.IsEnabled = true;
            }
        }

        private async void btnSendPrompt_Click(object sender, RoutedEventArgs e)
        {
            string userQuestion = txtChatPrompt.Text;
            if (string.IsNullOrWhiteSpace(userQuestion) || userQuestion == PlaceholderText)
            {
                MessageBox.Show("Zəhmət olmasa, sualınızı daxil edin.", "Xəbərdarlıq", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_currentTranscript))
            {
                MessageBox.Show("Əvvəlcə bir videonu konspektləşdirin.", "Xəbərdarlıq", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                btnSendPrompt.IsEnabled = false;
                txtResult.AppendText($"\n\n🤔 SİZİN SUALINIZ:\n{userQuestion}\n");
                txtResult.AppendText($"\n🤖 AI CAVABLANDIRIR (Gözləyin...):\n");
                txtResult.ScrollToEnd();

                string aiResponse = await _aiService.AskQuestionAboutVideoAsync(_currentTranscript, userQuestion);
                txtResult.AppendText(aiResponse);
                txtResult.ScrollToEnd();
                txtChatPrompt.Clear();
            }
            catch (Exception ex)
            {
                txtResult.AppendText($"\nXəta baş verdi: {ex.Message}");
            }
            finally
            {
                btnSendPrompt.IsEnabled = true;
            }
        }

        private void btnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var result = MessageBox.Show("Bu konspekti silmək istəyirsiniz?", "Silməni Təsdiqlə", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _summaryRepository.Delete(id);
                        LoadHistory();
                        ResetUiToDefault();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Silmə xətası: {ex.Message}", "Xəta", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void LstHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstHistory.SelectedItem is SummaryHistory selectedItem)
            {
                txtUrl.Text = selectedItem.VideoUrl;
                txtResult.Text = selectedItem.SummaryText;
                _currentTranscript = selectedItem.SummaryText;
                await PlayYouTubeVideoAsync(selectedItem.VideoUrl);
            }
        }

        private void BtnNewChat_Click(object sender, RoutedEventArgs e) => ResetUiToDefault();

        private void btnOpenYouTube_Click(object sender, RoutedEventArgs e)
        {
            string url = txtUrl.Text;
            if (!string.IsNullOrWhiteSpace(url))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }

        private void txtChatPrompt_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtChatPrompt.Text == PlaceholderText) txtChatPrompt.Text = "";
        }

        private void txtChatPrompt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtChatPrompt.Text)) txtChatPrompt.Text = PlaceholderText;
        }

        private void txtChatPrompt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                btnSendPrompt_Click(btnSendPrompt, new RoutedEventArgs());
            }
        }

    }
}