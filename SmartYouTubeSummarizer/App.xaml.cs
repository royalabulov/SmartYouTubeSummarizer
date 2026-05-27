using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartYouTubeSummarizer.Services;

namespace SmartYouTubeSummarizer
{
    public partial class App : Application
    {
        // Bütün proqram daxilində xidmətləri paylayacaq mərkəzi provayder
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // MainWindow-u container daxilindən asılılıqları ilə birlikdə çağırırıq
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Xidmətlərimizi interfeysləri ilə bərabər Container-ə qeydiyyatdan keçiririk
            services.AddSingleton<IYouTubeService, YouTubeService>();
            services.AddSingleton<IAiService, AiService>();
            services.AddSingleton<ISummaryRepository, SummaryRepository>();

            // Pəncərənin özünü də qeydiyyata alırıq ki, konstruktorda DI işləyə bilsin
            services.AddTransient<MainWindow>();
        }
    }
}