let electron = require("electron");
//#region electron/preload.ts
electron.contextBridge.exposeInMainWorld("api", {
	getTree: () => electron.ipcRenderer.invoke("get-tree"),
	openPlayer: (songId) => electron.ipcRenderer.invoke("open-player", songId),
	importCifraClub: (url) => electron.ipcRenderer.invoke("import-cifraclub", url)
});
//#endregion
