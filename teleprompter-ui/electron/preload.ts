import { contextBridge, ipcRenderer } from 'electron';

// Expondo uma API segura e limitada para o React
contextBridge.exposeInMainWorld('api', {
  getTree: () => ipcRenderer.invoke('get-tree'),
  openPlayer: (songId: string) => ipcRenderer.invoke('open-player', songId),
  importCifraClub: (url: string) => ipcRenderer.invoke('import-cifraclub', url),
  saveSong: (data: any) => ipcRenderer.invoke('save-song', data),
  toggleFavorite: (songId: number, isFav: boolean) => ipcRenderer.invoke('toggle-favorite', songId, isFav),
  createFolder: (name: string) => ipcRenderer.invoke('create-folder', name),
  deleteSong: (songId: number) => ipcRenderer.invoke('delete-song', songId),
  deleteFolder: (folderId: number) => ipcRenderer.invoke('delete-folder', folderId),
  removeFromFolder: (songId: number, folderId: number) => ipcRenderer.invoke('remove-from-folder', songId, folderId),
  syncFolderSongs: (folderId: number, songIds: number[]) => ipcRenderer.invoke('sync-folder-songs', folderId, songIds),
});
