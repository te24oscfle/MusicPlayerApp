using System.Text.Json.Serialization;

using API;
using Application;
using Database;
using Playback;

// Consts
const string APPLICATION_URL = "http://127.0.0.1:5017";

// Create builder
var builder = WebApplication.CreateBuilder(args);

// Convert string requests to enums where applicable
builder.Services.ConfigureHttpJsonOptions(options => 
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter()));

// Build app
var app = builder.Build();

// Initilization
DatabaseHelper.InitilizeDatabase();
DatabaseManager.RescanTracks();
AudioPlayer.InitilizeAudioPlayer();

// Map Endpoints
app.MapLibraryEndpoints();
app.MapQueueEndpoints();

// Start debug console
DebugConsole console = new DebugConsole();
Task consoleTask = Task.Run(async () => await console.Start());

// Run application
app.Run(APPLICATION_URL);