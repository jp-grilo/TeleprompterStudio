import { useEffect, useState, useRef } from 'react';
import './index.css';

interface PlayerProps {
  songId: number;
}

export default function Player({ songId }: PlayerProps) {
  const [song, setSong] = useState<any>(null);
  const [mode, setMode] = useState<'scroll' | 'slides'>('scroll');
  const [speed, setSpeed] = useState(1.0);

  // Slides state
  const [slides, setSlides] = useState<string[]>([]);
  const [currentSlide, setCurrentSlide] = useState(0);

  // Scroll state
  const [isPlaying, setIsPlaying] = useState(false);
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const savedMode = localStorage.getItem('playbackMode') as 'scroll' | 'slides';
    if (savedMode) setMode(savedMode);

    const loadSong = async () => {
      const data = await (window as any).api.getSong(songId);
      setSong(data);
      setSpeed(data.scrollSpeed || 1.0);

      // Split raw content into slides (by empty lines)
      const parts = data.rawContent.split(/\n\s*\n/).filter((p: string) => p.trim());
      setSlides(parts);
    };
    loadSong();
  }, [songId]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.code === 'Escape') {
        window.close();
      }

      if (mode === 'scroll') {
        if (e.code === 'Space') {
          e.preventDefault();
          setIsPlaying(prev => !prev);
        }
        if (e.code === 'ArrowUp' || e.code === 'ArrowDown') {
          e.preventDefault();
          const newSpeed = e.code === 'ArrowUp' ? speed + 0.1 : speed - 0.1;
          const clamped = Math.max(0.1, Math.min(newSpeed, 10.0));
          setSpeed(clamped);
          if (song) (window as any).api.updateSongSpeed(song.id, clamped);
        }
      } else {
        // Slides mode
        if (e.code === 'Space' || e.code === 'ArrowRight') {
          e.preventDefault();
          setCurrentSlide(prev => Math.min(prev + 1, slides.length - 1));
        }
        if (e.code === 'ArrowLeft') {
          e.preventDefault();
          setCurrentSlide(prev => Math.max(prev - 1, 0));
        }
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [mode, speed, song, slides.length]);

  // Scroll effect
  useEffect(() => {
    let animationFrame: number;
    let lastTime: number;

    const scrollStep = (time: number) => {
      if (!lastTime) lastTime = time;
      const dt = time - lastTime;
      lastTime = time;

      if (isPlaying && scrollContainerRef.current) {
        // Pixel per second logic. 50px/sec is base speed.
        const baseSpeed = 50;
        const deltaScroll = (baseSpeed * speed * dt) / 1000;
        scrollContainerRef.current.scrollTop += deltaScroll;
      }
      animationFrame = requestAnimationFrame(scrollStep);
    };

    if (mode === 'scroll' && isPlaying) {
      animationFrame = requestAnimationFrame(scrollStep);
    }

    return () => cancelAnimationFrame(animationFrame);
  }, [isPlaying, speed, mode]);

  if (!song) return <div style={{ background: '#000', height: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>Carregando...</div>;

  return (
    <div style={{ background: '#000', color: '#fff', height: '100vh', overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>

      {mode === 'scroll' ? (
        <div ref={scrollContainerRef} style={{ flex: 1, overflowY: 'auto', padding: '10vh 10vw' }}>
          <div style={{ maxWidth: '800px', margin: '0 auto', fontSize: '32px', whiteSpace: 'pre-wrap', lineHeight: '1.6' }}>
            <h1 style={{ fontSize: '48px', color: 'var(--accent-primary)', marginBottom: '8px' }}>{song.title}</h1>
            <h3 style={{ fontSize: '24px', color: '#888', marginBottom: '64px' }}>{song.artist}</h3>
            {song.rawContent}
          </div>
        </div>
      ) : (
        <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '5vh' }}>
          <div style={{ maxWidth: '1000px', width: '100%', textAlign: 'center', fontSize: '48px', whiteSpace: 'pre-wrap', lineHeight: '1.5' }}>
            {currentSlide === 0 ? (
              <div style={{ marginBottom: '64px' }}>
                <h1 style={{ fontSize: '64px', color: 'var(--accent-primary)', marginBottom: '16px' }}>{song.title}</h1>
                <h3 style={{ fontSize: '32px', color: '#888' }}>{song.artist}</h3>
              </div>
            ) : null}
            {slides[currentSlide]}
          </div>

          <div style={{ position: 'absolute', bottom: '24px', left: 0, right: 0, display: 'flex', justifyContent: 'center', gap: '8px' }}>
            {slides.map((_, i) => (
              <div key={i} style={{ width: '12px', height: '12px', borderRadius: '50%', background: i === currentSlide ? 'var(--accent-primary)' : '#333' }} />
            ))}
          </div>
        </div>
      )}

      {/* Overlay HUD - Disappears after a few seconds or on mouse leave? Keep simple for now */}
      <div style={{ position: 'absolute', top: '24px', right: '24px', display: 'flex', flexDirection: 'column', gap: '8px', opacity: 0.7 }}>
        <div style={{ background: 'rgba(255,255,255,0.1)', padding: '8px 16px', borderRadius: '8px', fontSize: '14px' }}>
          MODO: {mode === 'scroll' ? 'Rolagem Contínua' : 'Slides'}
        </div>
        {mode === 'scroll' && (
          <>
            <div style={{ background: 'rgba(255,255,255,0.1)', padding: '8px 16px', borderRadius: '8px', fontSize: '14px' }}>
              VELOCIDADE: {speed.toFixed(1)}x
            </div>
            <div style={{ background: 'rgba(255,255,255,0.1)', padding: '8px 16px', borderRadius: '8px', fontSize: '14px' }}>
              {isPlaying ? 'PAUSAR (Espaço)' : 'TOCAR (Espaço)'}
            </div>
            <div style={{ background: 'rgba(255,255,255,0.1)', padding: '8px 16px', borderRadius: '8px', fontSize: '14px' }}>
              AJUSTAR VELOCIDADE (Setas Cima/Baixo)
            </div>
          </>
        )}
        {mode === 'slides' && (
          <div style={{ background: 'rgba(255,255,255,0.1)', padding: '8px 16px', borderRadius: '8px', fontSize: '14px' }}>
            PASSAR (Espaço / Setas Esquerda-Direita)
          </div>
        )}
        <div style={{ background: 'rgba(255,255,255,0.1)', padding: '8px 16px', borderRadius: '8px', fontSize: '14px' }}>
          SAIR (Esc)
        </div>
      </div>

    </div>
  );
}
