using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 Load Firestore credentials from appsettings.json
            string firebaseKeyPath = builder.Configuration["Firebase:KeyPath"];
            if (!string.IsNullOrEmpty(firebaseKeyPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseKeyPath);
            }

            // 🔹 Register FirestoreDb as a singleton
            builder.Services.AddSingleton(provider =>
            {
                var projectId = builder.Configuration["Firebase:ProjectId"];
                return FirestoreDb.Create(projectId);
            });

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

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
