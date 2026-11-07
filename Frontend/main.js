// ================================
// #region Modules
// ================================

const { app, BrowserWindow, screen, ipcMain } = require("electron") 
const path = require("node:path")
const fs = require("node:fs")

// ================================
// #region Constants
// ================================

const DEVELOPER_MODE = true
const DEFAULT_BOUNDS = {
    width: 1440,
    height: 810
}
const MINIMUM_BOUNDS = {
    width: 800,
    height: 600
}
const DISPLAY_OVERLAP_THRESHOLD = 250

const SETTINGS_PATH = path.join(app.getPath("userData"), "window-settings.json")

const API_URL = "http://127.0.0.1:5017"

// ================================
// #region Window Functions
// ================================

function loadWindowSettings() {
    try {
        if(fs.existsSync(SETTINGS_PATH)) {
            return JSON.parse(fs.readFileSync(SETTINGS_PATH, "utf8"))
        }
    } catch (error) {
        console.error("Failed to load WindowSettings:", error)
    }

    return {}
}

function saveWindowSettings(window) {
    try {
        const settings = {
            bounds: window.getNormalBounds(),
            isMaximized: window.isMaximized()
        }

        fs.writeFileSync(SETTINGS_PATH, JSON.stringify(settings, null, 2))
    } catch(error) {
        console.error("Failed to save WindowSettings:", error)
    }
}

function isWindowPositionValid(bounds) {
    const displays = screen.getAllDisplays()

    return displays.some(display => {
        const area = display.workArea

        const overlapWidth = Math.max(
            0,
            Math.min(bounds.x + bounds.width, area.x + area.width) - Math.max(bounds.x, area.x)
        )

        const overlapHeight = Math.max(
            0,
            Math.min(bounds.y + bounds.height, area.y + area.height) - Math.max(bounds.y, area.y)
        )

        return overlapWidth >= DISPLAY_OVERLAP_THRESHOLD && overlapHeight >= DISPLAY_OVERLAP_THRESHOLD
    })
}

function createWindow() {
    const settings = loadWindowSettings()
    let bounds = settings.bounds

    // Check if the saved position of the window is still valid. If not, we reset it.
    if(bounds && !isWindowPositionValid(bounds)) {
        bounds = undefined
    }

    const window = new BrowserWindow({
        // Screen size
        width: bounds?.width ?? DEFAULT_BOUNDS.width,
        height: bounds?.height ?? DEFAULT_BOUNDS.height,

        minWidth: MINIMUM_BOUNDS.width,
        minHeight: MINIMUM_BOUNDS.height,

        // Screen Position (defaults to center of the screen if values are undefined)
        x: bounds?.x,
        y: bounds?.y,

        // Screen configuration
        frame: DEVELOPER_MODE,
        resizable: true,
        maximizable: true,
        minimizable: true,

        // Web preferences
        webPreferences: {
            // Security
            preload: path.join(__dirname, "preload.js"),
            contextIsolation: true,
            nodeIntegration: false,
            sandbox: true,
            
            // Configuration
            devTools: DEVELOPER_MODE,
        }
    })

    // Remove the (File, Edit, View, ...) menu at the top
    window.removeMenu()

    window.webContents.openDevTools()

    // Maximize screen
    if(settings.isMaximized)
        window.maximize()

    // Save window settings on close
    window.on("close", () => {
        saveWindowSettings(window)
    })

    // Load html file
    window.loadFile("index.html")
}

// ================================
// #region HTTP Endpoint Handlers
// ================================

async function handleImportFromPath(event, path) {
    const response = await fetch(`${API_URL}/api/library?path=${path}`)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}`)

    return await response.json()
}

async function handleGetAlbums() {
    const response = await fetch(`${API_URL}/api/library/albums`)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}`)

    return await response.json()
}

async function handleGetAlbum(event, albumId) {
    const response = await fetch(`${API_URL}/api/library/albums/album?albumId=${albumId}`)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}`)

    return await response.json()
}

async function handleGetPlaylists() {
    const response = await fetch(`${API_URL}/api/library/playlists`)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}`)

    return await response.json()
}

async function handleGetPlaylist(event, playlistId) {
    const response = await fetch(`${API_URL}/api/library/playlists/playlist?playlistId=${playlistId}`)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}`)

    return await response.json()
}

async function handleCreatePlaylist(event, requestBody) {
    const request = {
        method: "POST",
        headers: { 
            "Content-Type": "application/json" 
        },
        body: JSON.stringify(requestBody)
    }
    const response = await fetch(`${API_URL}/api/library/playlists/playlist`, request)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}\nRequest: ${JSON.stringify(request)}`)

    return await response.json()
}

async function handleUpdatePlaylistMetadata(event, requestBody) {
    const request = {
        method: "PATCH",
        headers: { 
            "Content-Type": "application/json" 
        },
        body: JSON.stringify(requestBody)
    }
    const response = await fetch(`${API_URL}/api/library/playlists/playlist`, request)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}\nRequest: ${JSON.stringify(request)}`)

    return await response.text()
}

async function handleDeletePlaylist(event, playlistId) {
    const response = await fetch(`${API_URL}/api/library/playlists/playlist?playlistId=${playlistId}`)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}\nRequest: ${JSON.stringify(request)}`)

    return await response.json()
}

async function handleAddPlaylistTracks(event, requestBody) {
    const request = {
        method: "POST",
        headers: { 
            "Content-Type": "application/json" 
        },
        body: JSON.stringify(requestBody)
    }
    const response = await fetch(`${API_URL}/api/library/playlists/playlist/tracks`, request)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}\nRequest: ${JSON.stringify(request)}`)

    return await response.json()
}

async function handleMovePlaylistTracks(event, requestBody) {
    const request = {
        method: "PATCH",
        headers: { 
            "Content-Type": "application/json" 
        },
        body: JSON.stringify(requestBody)
    }
    const response = await fetch(`${API_URL}/api/library/playlists/playlist/tracks`, request)

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}\nRequest: ${JSON.stringify(request)}`)

    return await response.json()
}

async function handleRemovePlaylistTracks(event, requestBody) {
    const params = new URLSearchParams()

    params.append("playlistId", requestBody.playlistId)
    requestBody.positions.forEach(position => {
        params.append("positions", position)
    });

    console.log(`${API_URL}/api/library/playlists/playlist/tracks?${params}`)
    const response = await fetch(`${API_URL}/api/library/playlists/playlist/tracks?${params}`,
        { method: "DELETE" }
    )

    if(!response.ok)
        throw new Error(`${response.status} ${response.statusText}\nRequest: ${JSON.stringify(requestBody)}`)

    return await response.json()
}

// ================================
// #region Runtime
// ================================

// API Callback
ipcMain.handle("import-from-path", handleImportFromPath)
ipcMain.handle("get-albums", handleGetAlbums)
ipcMain.handle("get-album", handleGetAlbum)
ipcMain.handle("get-playlists", handleGetPlaylists)
ipcMain.handle("get-playlist", handleGetPlaylist)
ipcMain.handle("create-playlist", handleCreatePlaylist)
ipcMain.handle("update-playlist-metadata", handleUpdatePlaylistMetadata)
ipcMain.handle("delete-playlist", handleDeletePlaylist)
ipcMain.handle("add-playlist-tracks", handleAddPlaylistTracks)
ipcMain.handle("move-playlist-tracks", handleMovePlaylistTracks)
ipcMain.handle("remove-playlist-tracks", handleRemovePlaylistTracks)

// Application Window
app.whenReady().then(() => {
    createWindow()

    app.on("activate", () => {
        if(BrowserWindow.getAllWindows().length === 0)
            createWindow()
    })
})

app.on("window-all-closed", () => {
    if(process.platform !== "darwin")
        app.quit()
})