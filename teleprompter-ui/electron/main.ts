import { app, BrowserWindow, ipcMain } from 'electron';
import * as path from 'path';
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

let mainWindow: BrowserWindow | null = null;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
    },
  });

  // Em modo desenvolvimento (Vite), carregamos o localhost
  if (process.env.VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(process.env.VITE_DEV_SERVER_URL);
  } else {
    // Em produção, carregaremos o arquivo compilado
    // mainWindow.loadFile(path.join(__dirname, '../dist/index.html'));
  }
}

app.whenReady().then(() => {
  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

// ==========================================
// IPC HANDLERS (Comunicação com React)
// ==========================================

ipcMain.handle('get-tree', async () => {
  try {
    // 1. Pastas Virtuais (Sistema)
    const allSongs = await prisma.song.findMany({ orderBy: { title: 'asc' } });
    
    const favorites = allSongs.filter(s => s.isFavorite);
    
    // Agrupar por artista
    const artistsMap = new Map<string, any>();
    for (const song of allSongs) {
      if (!artistsMap.has(song.artist)) {
        artistsMap.set(song.artist, {
          id: `artist-${song.artist}`,
          name: song.artist,
          icon: '🎤',
          isSystem: true,
          songs: [],
          subFolders: []
        });
      }
      artistsMap.get(song.artist).songs.push({ song });
    }
    const artistsFolders = Array.from(artistsMap.values()).sort((a, b) => a.name.localeCompare(b.name));

    const systemFolders = [
      { id: 'sys-all', name: 'Todas as músicas', icon: '🎵', isSystem: true, songs: allSongs.map(song => ({ song })), subFolders: [] },
      { id: 'sys-artists', name: 'Artistas', icon: '👤', isSystem: true, songs: [], subFolders: artistsFolders },
      { id: 'sys-fav', name: 'Favoritas', icon: '⭐', isSystem: true, songs: favorites.map(song => ({ song })), subFolders: [] }
    ];

    // 2. Pastas do Usuário
    const userFolders = await prisma.folder.findMany({
      include: {
        songs: { include: { song: true }, orderBy: { order: 'asc' } }
      },
      orderBy: { orderIndex: 'asc' }
    });

    // Reconstruir a árvore (apenas 1 nível por simplicidade, ou recursivo)
    const rootFolders = userFolders.filter(f => !f.parentFolderId).map(f => {
      return {
        ...f,
        isSystem: false,
        subFolders: userFolders.filter(sub => sub.parentFolderId === f.id)
      };
    });

    return [...systemFolders, ...rootFolders];
  } catch (error) {
    console.error('Erro ao buscar a árvore:', error);
    throw error;
  }
});

// Abrir Teleprompter Player nativo
ipcMain.handle('open-player', async (_, songId) => {
  const playerWindow = new BrowserWindow({
    fullscreen: true,
    autoHideMenuBar: true,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
    }
  });

  const baseUrl = process.env.VITE_DEV_SERVER_URL 
    ? process.env.VITE_DEV_SERVER_URL 
    : `file://${path.join(__dirname, '../dist/index.html')}`;
    
  // O React Router (HashRouter) ou estado local gerenciará a tela
  playerWindow.loadURL(`${baseUrl}#/player/${songId}`);
});

// Importar do CifraClub
ipcMain.handle('import-cifraclub', async (_, url: string) => {
  try {
    const response = await fetch(url);
    const html = await response.text();
    
    // Extrações simples usando Regex (Em prod ideal seria usar um HTML Parser como Cheerio)
    const titleMatch = html.match(/<h1[^>]*>(.*?)<\/h1>/i);
    const artistMatch = html.match(/<h2[^>]*><a[^>]*>(.*?)<\/a><\/h2>/i);
    const textMatch = html.match(/<pre[^>]*>(.*?)<\/pre>/is); // 'is' captura múltiplas linhas
    
    if (!textMatch) throw new Error("Não foi possível encontrar a cifra/letra na página.");

    // Limpar HTML da cifra
    const rawContent = textMatch[1]
      .replace(/<b[^>]*>(.*?)<\/b>/g, '$1')
      .replace(/<span[^>]*>(.*?)<\/span>/g, '')
      .replace(/<[^>]+>/g, '')
      .replace(/&quot;/g, '"')
      .replace(/&amp;/g, '&');

    return {
      title: titleMatch ? titleMatch[1].trim() : 'Música Desconhecida',
      artist: artistMatch ? artistMatch[1].trim() : 'Artista Desconhecido',
      rawContent: rawContent.trim()
    };
  } catch (error: any) {
    throw new Error('Falha ao importar: ' + error.message);
  }
});
