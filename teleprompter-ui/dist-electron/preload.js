let electron = require("electron");
//#region electron/preload.ts
electron.contextBridge.exposeInMainWorld("api", {
	getTree: () => electron.ipcRenderer.invoke("get-tree"),
	openPlayer: (songId) => electron.ipcRenderer.invoke("open-player", songId),
	importCifraClub: (url) => electron.ipcRenderer.invoke("import-cifraclub", url),
	saveSong: (data) => electron.ipcRenderer.invoke("save-song", data),
	toggleFavorite: (songId, isFav) => electron.ipcRenderer.invoke("toggle-favorite", songId, isFav),
	createFolder: (name) => electron.ipcRenderer.invoke("create-folder", name),
	deleteSong: (songId) => electron.ipcRenderer.invoke("delete-song", songId),
	deleteFolder: (folderId) => electron.ipcRenderer.invoke("delete-folder", folderId),
	removeFromFolder: (songId, folderId) => electron.ipcRenderer.invoke("remove-from-folder", songId, folderId),
	syncFolderSongs: (folderId, songIds) => electron.ipcRenderer.invoke("sync-folder-songs", folderId, songIds)
});
//#endregion
