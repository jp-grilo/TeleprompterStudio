import { useEffect, useState } from 'react'
import { Search, Plus, Settings, FolderOpen, Music, Play, Edit3, Star, Trash2, X, Save } from 'lucide-react'
import './index.css' // Import CSS here instead of App.css

function App() {
  const [folders, setFolders] = useState<any[]>([])
  const [error, setError] = useState<string | null>(null)
  const [selectedSong, setSelectedSong] = useState<any | null>(null)
  const [isEditMode, setIsEditMode] = useState(false)
  const [editContent, setEditContent] = useState('')

  useEffect(() => {
    const fetchTree = async () => {
      try {
        const data = await (window as any).api.getTree()
        setFolders(data)
      } catch (err: any) {
        setError(err.message)
      }
    }
    fetchTree()
  }, [])

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
    setIsEditMode(false)
    setEditContent(selectedSong.rawContent || '')
  }

  const handleSaveSong = () => {
    // Aqui no futuro chamaremos o Electron para salvar no DB
    setSelectedSong({ ...selectedSong, rawContent: editContent })
    setIsEditMode(false)
  }

  return (
    <div className="app-container">
      {/* Sidebar */}
      <div className="sidebar">
        <div className="sidebar-header">
          <h1>Teleprompter</h1>
        </div>

        <div className="search-container">
          <div className="search-box">
            <Search className="search-icon" size={18} />
            <input type="text" placeholder="Pesquisar..." />
          </div>
          <button className="icon-btn primary" title="Importar/Criar Música">
            <Plus size={20} />
          </button>
        </div>

        <div className="tree-view">
          {error && <div style={{ color: 'red', padding: '10px' }}>Erro: {error}</div>}
          
          {folders.map(folder => (
            <div key={folder.id} className="folder-item">
              <div className="folder-header">
                <FolderOpen size={18} />
                <span>{folder.name}</span>
              </div>
              
              <div className="song-list">
                {folder.songs.map((sf: any) => (
                  <div 
                    key={sf.song.id} 
                    className={`song-item ${selectedSong?.id === sf.song.id ? 'active' : ''}`}
                    onClick={() => handleSelectSong(sf.song)}
                  >
                    <Music size={16} />
                    <span>{sf.song.title}</span>
                  </div>
                ))}
              </div>
            </div>
          ))}
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
                    <button className="icon-btn" title="Favoritar">
                      <Star size={20} />
                    </button>
                    <button className="icon-btn danger" title="Excluir">
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
