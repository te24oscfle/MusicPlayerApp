using API;
using Application;
using Database;
using Playback;

// Consts
const string APPLICATION_URL = "http://127.0.0.1:5017";

// Build application
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Initilization
DatabaseHelper.InitilizeDatabase();
DatabaseManager.RescanTracks();
AudioPlayer.InitilizeAudioPlayer();

// Map Endpoints
app.MapLibraryEndpoints();

// Start debug console
DebugConsole console = new DebugConsole();
Task consoleTask = Task.Run(async () => await console.Start());

// Run application
app.Run(APPLICATION_URL);