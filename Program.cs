using NewDawnPropertiesApi_V1.Services;
using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 Register FirestoreService as a singleton
            builder.Services.AddSingleton<FirestoreService>();

            // Add controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Swagger UI in development mode
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 🔹 Disable HTTPS redirection on Render (optional)
            // app.UseHttpsRedirection(); // Commented out for container deployments

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
