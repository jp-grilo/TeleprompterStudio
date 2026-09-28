import { contextBridge, ipcRenderer } from 'electron';

// Expondo uma API segura e limitada para o React
contextBridge.exposeInMainWorld('api', {
  getTree: () => ipcRenderer.invoke('get-tree'),
});
