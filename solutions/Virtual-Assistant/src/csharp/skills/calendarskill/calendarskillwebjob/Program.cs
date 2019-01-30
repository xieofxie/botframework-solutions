using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CalendarSkillWebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureWebJobs(webJobConfiguration =>
                {
                    webJobConfiguration.AddTimers();
                    webJobConfiguration.AddAzureStorageCoreServices();
                    webJobConfiguration.AddServiceBus();
                })
                .ConfigureServices(serviceCollection => serviceCollection.AddTransient<UpcomingMeetingHandler>())
                .Build();

            builder.Run();
        }
    }
}