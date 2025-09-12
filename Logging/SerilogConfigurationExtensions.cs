using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Logging
{
	public static class SerilogConfigurationExtensions
	{
		public static void ConfigureLogging(this WebApplicationBuilder builder, string serviceName)
		{
			var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{environment}.json", optional: true)
				.Build();

			// Получите URL Elasticsearch из конфигурации
			var elasticsearchUrl = configuration["Elasticsearch:Url"] ?? "http://pharmacy-elasticsearch:9200";

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration) // Чтение базовой конфигурации из appsettings.json
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithEnvironmentName()
				.Enrich.WithThreadId()
				.Enrich.WithProperty("Environment", environment)
				.Enrich.WithProperty("Application", serviceName)
				// Конфигурация Elasticsearch Sink
				.WriteTo.Elasticsearch(new[] { new Uri(elasticsearchUrl) }, opts =>
				{
				// Формат имени Data Stream: {type}-{dataset}-{namespace}
				opts.DataStream = new DataStreamName(
					type: "logs",
					dataSet: serviceName.ToLower(),
					@namespace: environment?.ToLower().Replace('.', '-'));
                    // Стратегия обработки ошибок начальной загрузки (регистрация шаблонов)
                    opts.BootstrapMethod = BootstrapMethod.Failure; // Исключение при ошибке
                    // Опционально: Настройка буферизации и параллелизма
                    opts.ConfigureChannel = channelOpts =>
                    {
                        channelOpts.BufferOptions = new BufferOptions
                        {
							ExportMaxConcurrency = 10 // Количество параллельных потребителей для записи
                        };
					};
                })
                .CreateLogger();

			builder.Host.UseSerilog();
		}
	}
}
