import { useEffect, useState, useMemo } from 'react'
import { Search, FolderPlus, FilePlus, Settings, FolderOpen, Music, Play, Edit3, Star, Trash2, X, Save, ChevronDown, ChevronRight, Plus } from 'lucide-react'
import './index.css'

function App() {
  const [folders, setFolders] = useState<any[]>([])
  const [error, setError] = useState<string | null>(null)
  
  const [selectedSong, setSelectedSong] = useState<any | null>(null)
  const [isEditMode, setIsEditMode] = useState(false)
  const [editContent, setEditContent] = useState('')
  const [searchQuery, setSearchQuery] = useState('')
  
  const [showImportModal, setShowImportModal] = useState(false)
  const [importUrl, setImportUrl] = useState('')
  const [isImporting, setIsImporting] = useState(false)

  const [showFolderModal, setShowFolderModal] = useState(false)
  const [newFolderName, setNewFolderName] = useState('')

  const [contextMenu, setContextMenu] = useState<{x: number, y: number, item: any, type: 'song'|'folder', folderId?: number} | null>(null)
  
  const [collapsedFolders, setCollapsedFolders] = useState<Set<number>>(new Set())
  const [showAddToFolderModal, setShowAddToFolderModal] = useState<number | null>(null)
  const [selectedSongsForFolder, setSelectedSongsForFolder] = useState<number[]>([])

  const fetchTree = async () => {
    try {
      const data = await (window as any).api.getTree()
      setFolders(data)
    } catch (err: any) {
      setError(err.message)
    }
  }

  useEffect(() => {
    fetchTree()
  }, [])

  // Filtro de Pesquisa (Global)
  const filteredFolders = useMemo(() => {
    if (!searchQuery.trim()) return folders;
    
    const lowerQuery = searchQuery.toLowerCase();
    
    return folders.map(folder => {
      // Filtrar músicas da pasta
      const filteredSongs = folder.songs?.filter((sf: any) => 
        sf.song.title.toLowerCase().includes(lowerQuery) || 
        sf.song.artist.toLowerCase().includes(lowerQuery)
      ) || [];
      
      // Filtrar subpastas
      const filteredSubFolders = folder.subFolders?.map((sub: any) => {
        const subFilteredSongs = sub.songs?.filter((sf: any) => 
          sf.song.title.toLowerCase().includes(lowerQuery) || 
          sf.song.artist.toLowerCase().includes(lowerQuery)
        ) || [];
        return { ...sub, songs: subFilteredSongs };
      }).filter((sub: any) => sub.songs.length > 0) || [];

      return {
        ...folder,
        songs: filteredSongs,
        subFolders: filteredSubFolders
      };
    }).filter(folder => folder.songs?.length > 0 || folder.subFolders?.length > 0);
  }, [folders, searchQuery]);

  const handleSelectSong = (song: any) => {
    setSelectedSong(song)
    setIsEditMode(false)
    setEditContent(song.rawContent || '')
  }

  const handleEditClick = () => {
    setIsEditMode(true)
    setEditContent(selectedSong.rawContent || '')
  }

  const handleCancelEdit = () => {
    if (selectedSong.isNewImport) {
      setSelectedSong(null);
    } else {
      setEditContent(selectedSong.rawContent || '')
    }
    setIsEditMode(false)
  }

  const handleSaveSong = async () => {
    try {
      const updatedData = { ...selectedSong, rawContent: editContent };
      const savedSong = await (window as any).api.saveSong(updatedData);
      setSelectedSong(savedSong);
      setIsEditMode(false);
      fetchTree(); // Recarregar a árvore
    } catch(err) {
      alert("Erro ao salvar: " + err);
    }
  }

  const handleToggleFavorite = async () => {
    try {
      const newFavStatus = !selectedSong.isFavorite;
      await (window as any).api.toggleFavorite(selectedSong.id, newFavStatus);
      setSelectedSong({ ...selectedSong, isFavorite: newFavStatus });
      fetchTree();
    } catch(err) {
      alert("Erro ao favoritar: " + err);
    }
  }

  const handleCreateFolder = async () => {
    if (!newFolderName.trim()) return;
    try {
      await (window as any).api.createFolder(newFolderName);
      setShowFolderModal(false);
      setNewFolderName('');
      fetchTree();
    } catch(err) {
      alert("Erro ao criar pasta: " + err);
    }
  }

  const handleDeleteSong = async (songId: number) => {
    if (!confirm("Tem certeza que deseja excluir esta música?")) return;
    try {
      await (window as any).api.deleteSong(songId);
      if (selectedSong?.id === songId) setSelectedSong(null);
      fetchTree();
    } catch(err) {
      alert("Erro ao excluir música: " + err);
    }
  }

  const handleDeleteFolder = async (folderId: number) => {
    if (!confirm("Tem certeza que deseja excluir esta pasta e tudo dentro dela?")) return;
    try {
      await (window as any).api.deleteFolder(folderId);
      fetchTree();
    } catch(err) {
      alert("Erro ao excluir pasta: " + err);
    }
  }

  const handleRemoveFromFolder = async (songId: number, folderId: number) => {
    try {
      await (window as any).api.removeFromFolder(songId, folderId);
      fetchTree();
    } catch(err) {
      alert("Erro ao remover música da pasta: " + err);
    }
  }

  const handleSyncFolderSongs = async () => {
    if (showAddToFolderModal === null) return;
    try {
      await (window as any).api.syncFolderSongs(showAddToFolderModal, selectedSongsForFolder);
      setShowAddToFolderModal(null);
      fetchTree();
    } catch(err) {
      alert("Erro ao sincronizar músicas na pasta: " + err);
    }
  }

  const toggleFolder = (folderId: number) => {
    setCollapsedFolders(prev => {
      const next = new Set(prev);
      if (next.has(folderId)) next.delete(folderId);
      else next.add(folderId);
      return next;
    });
  }

  const handleContextMenu = (e: React.MouseEvent, item: any, type: 'song'|'folder', folderId?: number) => {
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, item, type, folderId });
  }

  const closeContextMenu = () => {
    setContextMenu(null);
  }

  // Fecha o menu ao clicar fora
  useEffect(() => {
    document.addEventListener('click', closeContextMenu);
    return () => document.removeEventListener('click', closeContextMenu);
  }, []);

  const handleImportCifraClub = async () => {
    if (!importUrl) return;
    setIsImporting(true);
    try {
      const data = await (window as any).api.importCifraClub(importUrl);
      setSelectedSong({ ...data, isNewImport: true, id: 'temp-id' });
      setEditContent(data.rawContent);
      setIsEditMode(true);
      setShowImportModal(false);
      setImportUrl('');
    } catch (err: any) {
      alert(err.message);
    } finally {
      setIsImporting(false);
    }
  }

  const renderFolder = (folder: any, isSubFolder = false) => {
    const isCollapsed = collapsedFolders.has(folder.id);
    return (
      <div key={folder.id} className={isSubFolder ? "song-list" : "folder-item"}>
        <div 
          className="folder-header" 
          style={{ color: folder.isSystem ? 'var(--accent-primary)' : 'var(--text-secondary)', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}
          onContextMenu={(e) => !folder.isSystem && handleContextMenu(e, folder, 'folder')}
          onClick={() => toggleFolder(folder.id)}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            {isCollapsed ? <ChevronRight size={18} /> : <ChevronDown size={18} />}
            <FolderOpen size={18} />
            <span>{folder.name}</span>
          </div>
          {!folder.isSystem && (
            <button 
              className="icon-btn" 
              style={{ width: '24px', height: '24px', padding: 0 }} 
              onClick={(e) => { 
                e.stopPropagation(); 
                setShowAddToFolderModal(folder.id);
                // Pre-popula o array de seleção com os IDs das músicas que já estão na pasta
                setSelectedSongsForFolder(folder.songs ? folder.songs.map((sf: any) => sf.song.id) : []);
              }}
              title="Gerenciar músicas nesta pasta"
            >
              <Plus size={16} />
            </button>
          )}
        </div>
        
        {!isCollapsed && (
          <div className="song-list" style={{ paddingLeft: '16px' }}>
            {folder.songs?.map((sf: any) => (
              <div 
                key={`${folder.id}-${sf.song.id}`} 
                className={`song-item ${selectedSong?.id === sf.song.id ? 'active' : ''}`}
                onClick={() => handleSelectSong(sf.song)}
                onContextMenu={(e) => handleContextMenu(e, sf.song, 'song', folder.id)}
              >
                <Music size={16} />
                <span>{sf.song.title}</span>
                {sf.song.isFavorite && <Star size={12} fill="var(--warning)" color="var(--warning)" style={{marginLeft: 'auto'}}/>}
              </div>
            ))}
            
            {folder.subFolders?.map((sub: any) => renderFolder(sub, true))}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="app-container">
      {/* Import Modal */}
      {showImportModal && (
        <div className="modal-overlay" style={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(0,0,0,0.7)', zIndex: 50, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <div style={{ background: 'var(--bg-secondary)', padding: '24px', borderRadius: '12px', width: '400px' }}>
            <h3 style={{ marginBottom: '16px' }}>Importar do CifraClub</h3>
            <input 
              type="text" 
              placeholder="Cole a URL aqui..." 
              value={importUrl}
              onChange={e => setImportUrl(e.target.value)}
              style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid var(--border-strong)', background: 'var(--bg-primary)', color: 'white', marginBottom: '16px' }}
            />
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
              <button className="icon-btn" style={{ width: 'auto', padding: '0 16px' }} onClick={() => setShowImportModal(false)}>Cancelar</button>
              <button className="icon-btn primary" style={{ width: 'auto', padding: '0 16px' }} onClick={handleImportCifraClub} disabled={isImporting}>
                {isImporting ? 'Importando...' : 'Importar'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Create Folder Modal */}
      {showFolderModal && (
        <div className="modal-overlay" style={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(0,0,0,0.7)', zIndex: 50, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <div style={{ background: 'var(--bg-secondary)', padding: '24px', borderRadius: '12px', width: '400px' }}>
            <h3 style={{ marginBottom: '16px' }}>Nova Pasta</h3>
            <input 
              type="text" 
              placeholder="Nome da pasta..." 
              value={newFolderName}
              onChange={e => setNewFolderName(e.target.value)}
              style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid var(--border-strong)', background: 'var(--bg-primary)', color: 'white', marginBottom: '16px' }}
            />
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
              <button className="icon-btn" style={{ width: 'auto', padding: '0 16px' }} onClick={() => setShowFolderModal(false)}>Cancelar</button>
              <button className="icon-btn primary" style={{ width: 'auto', padding: '0 16px' }} onClick={handleCreateFolder}>
                Criar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Context Menu */}
      {contextMenu && (
        <div 
          style={{
            position: 'fixed',
            top: contextMenu.y,
            left: contextMenu.x,
            background: 'var(--bg-secondary)',
            border: '1px solid var(--border-strong)',
            borderRadius: '6px',
            boxShadow: '0 4px 12px rgba(0,0,0,0.3)',
            zIndex: 100,
            padding: '4px',
            minWidth: '150px'
          }}
        >
          {contextMenu.type === 'song' ? (
            <>
              <button 
                style={{ width: '100%', textAlign: 'left', padding: '8px 12px', background: 'transparent', border: 'none', color: 'white', cursor: 'pointer', borderRadius: '4px' }}
                onMouseEnter={e => e.currentTarget.style.background = 'var(--bg-tertiary)'}
                onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
                onClick={async () => {
                  await (window as any).api.toggleFavorite(contextMenu.item.id, !contextMenu.item.isFavorite);
                  fetchTree();
                  if (selectedSong?.id === contextMenu.item.id) {
                    setSelectedSong({ ...selectedSong, isFavorite: !contextMenu.item.isFavorite });
                  }
                }}
              >
                {contextMenu.item.isFavorite ? 'Desfavoritar' : 'Favoritar'}
              </button>
              
              {contextMenu.folderId && typeof contextMenu.folderId === 'number' && (
                <button 
                  style={{ width: '100%', textAlign: 'left', padding: '8px 12px', background: 'transparent', border: 'none', color: 'var(--warning)', cursor: 'pointer', borderRadius: '4px' }}
                  onMouseEnter={e => e.currentTarget.style.background = 'rgba(245, 158, 11, 0.1)'}
                  onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
                  onClick={() => handleRemoveFromFolder(contextMenu.item.id, contextMenu.folderId!)}
                >
                  Remover da Pasta
                </button>
              )}

              <button 
                style={{ width: '100%', textAlign: 'left', padding: '8px 12px', background: 'transparent', border: 'none', color: 'var(--danger)', cursor: 'pointer', borderRadius: '4px' }}
                onMouseEnter={e => e.currentTarget.style.background = 'rgba(239, 68, 68, 0.1)'}
                onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
                onClick={() => handleDeleteSong(contextMenu.item.id)}
              >
                Excluir do Sistema
              </button>
            </>
          ) : (
            <button 
              style={{ width: '100%', textAlign: 'left', padding: '8px 12px', background: 'transparent', border: 'none', color: 'var(--danger)', cursor: 'pointer', borderRadius: '4px' }}
              onMouseEnter={e => e.currentTarget.style.background = 'rgba(239, 68, 68, 0.1)'}
              onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
              onClick={() => handleDeleteFolder(contextMenu.item.id)}
            >
              Excluir Pasta
            </button>
          )}
        </div>
      )}

      {/* Add To Folder Modal */}
      {showAddToFolderModal !== null && (
        <div className="modal-overlay" style={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(0,0,0,0.7)', zIndex: 50, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <div style={{ background: 'var(--bg-secondary)', padding: '24px', borderRadius: '12px', width: '400px', maxHeight: '80vh', display: 'flex', flexDirection: 'column' }}>
            <h3 style={{ marginBottom: '16px' }}>Gerenciar Músicas na Pasta</h3>
            <div style={{ overflowY: 'auto', flex: 1, marginBottom: '16px', background: 'var(--bg-primary)', borderRadius: '6px', border: '1px solid var(--border-strong)' }}>
              {/* Pegamos a pasta "Todas as músicas" (sys-all) para listar as disponíveis */}
              {folders.find(f => f.id === 'sys-all')?.songs?.map((sf: any) => {
                const isChecked = selectedSongsForFolder.includes(sf.song.id);
                return (
                  <label 
                    key={`add-${sf.song.id}`}
                    style={{ padding: '8px 12px', cursor: 'pointer', borderBottom: '1px solid var(--border-subtle)', display: 'flex', alignItems: 'center', gap: '8px' }}
                    onMouseEnter={e => e.currentTarget.style.background = 'var(--bg-tertiary)'}
                    onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
                  >
                    <input 
                      type="checkbox"
                      checked={isChecked}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setSelectedSongsForFolder([...selectedSongsForFolder, sf.song.id]);
                        } else {
                          setSelectedSongsForFolder(selectedSongsForFolder.filter(id => id !== sf.song.id));
                        }
                      }}
                      style={{ cursor: 'pointer' }}
                    />
                    <Music size={16} />
                    <span style={{ flex: 1 }}>{sf.song.title}</span>
                  </label>
                );
              })}
            </div>
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
              <button className="icon-btn" style={{ width: 'auto', padding: '0 16px' }} onClick={() => setShowAddToFolderModal(null)}>Cancelar</button>
              <button className="icon-btn primary" style={{ width: 'auto', padding: '0 16px' }} onClick={handleSyncFolderSongs}>Confirmar</button>
            </div>
          </div>
        </div>
      )}

      {/* Sidebar */}
      <div className="sidebar">
        <div className="sidebar-header">
          <h1>Teleprompter</h1>
        </div>

        <div className="search-container" style={{ flexDirection: 'column' }}>
          <div className="search-box" style={{ width: '100%', marginBottom: '10px' }}>
            <Search className="search-icon" size={18} />
            <input 
              type="text" 
              placeholder="Pesquisar..." 
              value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
            />
          </div>
          <div style={{ display: 'flex', gap: '8px', width: '100%' }}>
            <button className="icon-btn primary" style={{ flex: 1, gap: '8px' }} title="Nova Pasta" onClick={() => setShowFolderModal(true)}>
              <FolderPlus size={18} /> Nova Pasta
            </button>
            <button className="icon-btn primary" style={{ flex: 1, gap: '8px' }} title="Importar CifraClub" onClick={() => setShowImportModal(true)}>
              <FilePlus size={18} /> Importar
            </button>
          </div>
        </div>

        <div className="tree-view">
          {error && <div style={{ color: 'red', padding: '10px' }}>Erro: {error}</div>}
          {filteredFolders.map(folder => renderFolder(folder, false))}
        </div>

        <div className="sidebar-footer">
          <Settings size={20} />
          <span>Configurações</span>
        </div>
      </div>

      {/* Main Area */}
      <div className="main-area">
        {!selectedSong ? (
          <div className="empty-state">
            <Music size={64} />
            <h2>Selecione uma música...</h2>
          </div>
        ) : (
          <>
            <div className="content-header">
              <div className="content-title">
                <h2>{selectedSong.title}</h2>
                <p>{selectedSong.artist}</p>
              </div>

              <div className="content-actions">
                {!isEditMode ? (
                  <>
                    <button className="icon-btn success" title="Abrir no Palco">
                      <Play size={20} />
                    </button>
                    <button className="icon-btn" title="Editar" onClick={handleEditClick}>
                      <Edit3 size={20} />
                    </button>
                    <button className="icon-btn" title="Favoritar" onClick={handleToggleFavorite}>
                      <Star size={20} fill={selectedSong.isFavorite ? "var(--warning)" : "none"} color={selectedSong.isFavorite ? "var(--warning)" : "currentColor"} />
                    </button>
                    <button className="icon-btn danger" title="Excluir" onClick={() => handleDeleteSong(selectedSong.id)}>
                      <Trash2 size={20} />
                    </button>
                  </>
                ) : (
                  <>
                    <button className="icon-btn danger" style={{ width: 'auto', padding: '0 16px', gap: '8px' }} onClick={handleCancelEdit}>
                      <X size={18} /> Cancelar
                    </button>
                    <button className="icon-btn success" style={{ width: 'auto', padding: '0 16px', gap: '8px' }} onClick={handleSaveSong}>
                      <Save size={18} /> Salvar
                    </button>
                  </>
                )}
              </div>
            </div>

            <div className="editor-area">
              <textarea
                className="editor-textarea"
                readOnly={!isEditMode}
                value={editContent}
                onChange={(e) => setEditContent(e.target.value)}
                placeholder="Nenhuma letra cadastrada..."
              />
            </div>
          </>
        )}
      </div>
    </div>
  )
}

export default App
