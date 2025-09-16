using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.SystemMetrics;

namespace Monitoring
{
	public static class HealthCheckExtensions
	{
		public static void ConfigureHealthCheck(this WebApplicationBuilder builder)
		{
			builder.Services.AddHealthChecks().ForwardToPrometheus();
			builder.Services.AddSystemMetrics();
		}

		public static void ConfigureMetrics(this WebApplication application)
		{
			application.UseHttpMetrics(options =>
			{
				options.RequestDuration.Enabled = true;
				options.RequestCount.Enabled = true;
				options.InProgress.Enabled = true;
			});

			application.UseEndpoints(endpoints =>
			{
				endpoints.MapMetrics();
				endpoints.MapHealthChecks("/health");
			});

			application.UseMetricServer();
		}
	}
}
