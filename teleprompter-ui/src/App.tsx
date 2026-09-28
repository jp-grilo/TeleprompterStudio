import { useEffect, useState } from 'react'
import './App.css'

function App() {
  const [folders, setFolders] = useState<any[]>([])
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    // Acessar a API segura exposta pelo preload.ts
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

  return (
    <div style={{ padding: '20px', fontFamily: 'sans-serif' }}>
      <h1>TeleprompterStudio UI (Migração)</h1>
      <p>Status do SQLite: {folders.length > 0 ? '✅ Conectado' : '⏳ Carregando...'}</p>
      
      {error && <div style={{ color: 'red' }}>Erro: {error}</div>}

      <div style={{ marginTop: '20px' }}>
        {folders.map(folder => (
          <div key={folder.id} style={{ marginBottom: '20px' }}>
            <h2 style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <span>📁</span> {folder.name}
            </h2>
            <ul style={{ listStyleType: 'none', paddingLeft: '20px' }}>
              {folder.songs.map((sf: any) => (
                <li key={sf.song.id} style={{ padding: '8px', borderBottom: '1px solid #ccc' }}>
                  <strong>{sf.song.title}</strong> - {sf.song.artist}
                  <div style={{ fontSize: '12px', color: '#666' }}>
                    Speed: {sf.song.scrollSpeed}x | Transpose: {sf.song.transposeAmount}
                  </div>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </div>
  )
}

export default App
