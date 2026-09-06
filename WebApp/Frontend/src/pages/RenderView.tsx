import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

export default function RenderView() {
  const [searchParams] = useSearchParams();
  const contentId = searchParams.get('contentId');
  const [isReady, setIsReady] = useState(false);
  const [content, setContent] = useState<string | null>(null);

  useEffect(() => {
    // In a real application, we would fetch the content based on contentId from the backend.
    // E.g. fetch(`/api/content/${contentId}`)
    // For now, we simulate loading for 500ms and display a dummy text.
    const timer = setTimeout(() => {
      setContent(contentId ? `Content loaded for ID: ${contentId}` : "Default content");
      setIsReady(true);
    }, 500);

    return () => clearTimeout(timer);
  }, [contentId]);

  return (
    <div style={{ width: '1080px', height: '1920px', background: '#ffffff', color: '#000000', display: 'flex', justifyContent: 'center', alignItems: 'center', fontSize: '3rem', position: 'relative', overflow: 'hidden' }}>
      <h1>{content || "Loading..."}</h1>
      
      {/* Playwright waits for this element to know fonts/images are loaded */}
      {isReady && <div id="render-ready" style={{ display: 'none' }}></div>}

      {/* Example animation div to show time control works */}
      <div 
         id="anim-box" 
         style={{ 
            position: 'absolute', 
            top: '50%', 
            left: '50%', 
            width: '200px', 
            height: '200px', 
            backgroundColor: 'red',
            transform: 'translate(-50%, -50%)'
         }} 
      />
    </div>
  );
}
