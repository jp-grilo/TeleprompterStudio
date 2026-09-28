let electron = require("electron");
//#region electron/preload.ts
electron.contextBridge.exposeInMainWorld("api", { getTree: () => electron.ipcRenderer.invoke("get-tree") });
//#endregion
