import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import Player from './Player.tsx'

function MainRouter() {
  const hash = window.location.hash;
  if (hash.startsWith('#/player/')) {
    const songId = parseInt(hash.replace('#/player/', ''));
    return <Player songId={songId} />;
  }
  return <App />;
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MainRouter />
  </StrictMode>,
)
