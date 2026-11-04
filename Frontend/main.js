// ================================
// #region Modules
// ================================

const { app, BrowserWindow, screen } = require("electron") 
const path = require("node:path")
const fs = require("node:fs")

// ================================
// #region Constants
// ================================

const DEV_MODE = true
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

// ================================
// #region Functions
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
        frame: !DEV_MODE,
        resizable: true,
        maximizable: true,
        minimizable: true,

        // Web preferences
        webPreferences: {
            // Security
            contextIsolation: true,
            nodeIntegration: false,
            sandbox: true,
            
            // Configuration
            devTools: DEV_MODE,
        }
    })

    // Remove the (File, Edit, View, ...) menu at the top
    window.removeMenu()

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
// #region Runtime
// ================================

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