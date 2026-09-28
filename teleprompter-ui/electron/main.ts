import { app, BrowserWindow, ipcMain } from 'electron';
import * as path from 'path';
import * as fs from 'fs';
import { PrismaClient } from '@prisma/client';

const isDev = !app.isPackaged;
let dbUrl = 'file:./dev.db';

if (!isDev) {
  const dbPath = path.join(app.getPath('userData'), 'teleprompt.db');
  const packagedDbPath = path.join(process.resourcesPath, 'prisma', 'dev.db');
  if (!fs.existsSync(dbPath)) {
    try {
      fs.copyFileSync(packagedDbPath, dbPath);
    } catch (err) {
      console.error('Failed to copy database:', err);
    }
  }
  dbUrl = `file:${dbPath}`;
}

const prisma = new PrismaClient({
  datasources: {
    db: {
      url: dbUrl
    }
  }
});

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
    mainWindow.loadFile(path.join(__dirname, '../dist/index.html'));
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

// Buscar música específica
ipcMain.handle('get-song', async (_, songId: number) => {
  return await prisma.song.findUnique({
    where: { id: songId }
  });
});

ipcMain.handle('update-song-speed', async (_, songId: number, scrollSpeed: number) => {
  return await prisma.song.update({
    where: { id: songId },
    data: { scrollSpeed }
  });
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
  return new Promise((resolve, reject) => {
    let hiddenWin: BrowserWindow | null = new BrowserWindow({
      show: false,
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true
      }
    });

    hiddenWin.webContents.on('did-finish-load', async () => {
      try {
        const data = await hiddenWin!.webContents.executeJavaScript(`
          (() => {
            let title = 'Música Desconhecida';
            let artist = 'Artista Desconhecido';

            // Tenta usar JSON-LD para maior precisão (CifraClub usa isso)
            try {
              const scripts = Array.from(document.querySelectorAll('script[type="application/ld+json"]'));
              for (const script of scripts) {
                const data = JSON.parse(script.innerText || '{}');
                // Estrutura do CifraClub para música/artista
                if (data['@type'] && data['@type'].includes('MusicRecording')) {
                  if (data.name) {
                    const parts = data.name.split(' - ');
                    if (parts.length > 1) {
                      title = parts[1].trim();
                    } else {
                      title = data.name;
                    }
                  }
                  if (data.byArtist && data.byArtist.name) {
                    artist = data.byArtist.name;
                  }
                }
              }
            } catch (e) {}

            // Fallback para os H1 e H2 caso o JSON-LD falhe
            if (title === 'Música Desconhecida') {
              title = document.querySelector('h1')?.innerText || title;
            }
            if (artist === 'Artista Desconhecido') {
              artist = document.querySelector('h2 a')?.innerText || document.querySelector('h2')?.innerText || artist;
            }
            
            const preEl = document.querySelector('pre[data-chord-content="true"]') || document.querySelector('pre');
            if (!preEl) return { error: "Não foi possível encontrar a cifra/letra na página." };

            let rawHtml = preEl.innerHTML;
            
            // Remover conteúdo de spans (geralmente tabs ocultas e notas extras)
            rawHtml = rawHtml.replace(/<span[^>]*>[\\s\\S]*?<\\/span>/gi, '');
            
            // Transformar fechamento de div em quebra de linha (estrutura do cifraclub)
            rawHtml = rawHtml.replace(/<\\/div>/gi, '\\n');
            
            // Remover o resto das tags HTML
            rawHtml = rawHtml.replace(/<[^>]+>/g, '');
            
            // Decoding de HTML entities básicos
            rawHtml = rawHtml
              .replace(/&quot;/g, '"')
              .replace(/&amp;/g, '&')
              .replace(/&lt;/g, '<')
              .replace(/&gt;/g, '>')
              .replace(/&#39;/g, "'");

            return { title: title.trim(), artist: artist.trim(), rawContent: rawHtml.trim() };
          })();
        `);

        hiddenWin?.destroy();
        hiddenWin = null;
        
        if (data.error) {
          reject(new Error(data.error));
        } else {
          resolve(data);
        }
      } catch (err: any) {
        if (hiddenWin) hiddenWin.destroy();
        reject(new Error("Erro ao extrair dados da página: " + err.message));
      }
    });

    hiddenWin.webContents.on('did-fail-load', () => {
      if (hiddenWin) hiddenWin.destroy();
      reject(new Error("Falha ao carregar a página do CifraClub."));
    });

    // Inicia o carregamento com um User-Agent de navegador normal
    hiddenWin.loadURL(url, {
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36'
    });
  });
});

// Salvar Música
ipcMain.handle('save-song', async (_, songData) => {
  if (songData.id === 'temp-id') {
    // É uma nova música importada, salvar no BD (Na pasta principal por ex)
    const newSong = await prisma.song.create({
      data: {
        title: songData.title,
        artist: songData.artist,
        rawContent: songData.rawContent,
        isFavorite: false
      }
    });
    return newSong;
  } else {
    const updated = await prisma.song.update({
      where: { id: songData.id },
      data: { rawContent: songData.rawContent }
    });
    return updated;
  }
});

// Toggle Favorite
ipcMain.handle('toggle-favorite', async (_, songId, isFav) => {
  return await prisma.song.update({
    where: { id: songId },
    data: { isFavorite: isFav }
  });
});

// Criar Pasta
ipcMain.handle('create-folder', async (_, name) => {
  return await prisma.folder.create({
    data: { name, orderIndex: 99 }
  });
});

// Excluir Música
ipcMain.handle('delete-song', async (_, songId) => {
  return await prisma.song.delete({
    where: { id: songId }
  });
});

// Excluir Pasta
ipcMain.handle('delete-folder', async (_, folderId) => {
  return await prisma.folder.delete({
    where: { id: folderId }
  });
});

// Remover Música da Pasta Específica
ipcMain.handle('remove-from-folder', async (_, songId, folderId) => {
  return await prisma.songFolder.deleteMany({
    where: { songId, folderId }
  });
});

// Sincronizar Músicas na Pasta Específica
ipcMain.handle('sync-folder-songs', async (_, folderId: number, songIds: number[]) => {
  // Deleta todas as relações desta pasta
  await prisma.songFolder.deleteMany({
    where: { folderId }
  });

  // Recria as selecionadas
  const data = songIds.map((id, index) => ({
    folderId,
    songId: id,
    order: index
  }));

  if (data.length > 0) {
    await prisma.songFolder.createMany({ data });
  }
  return true;
});
