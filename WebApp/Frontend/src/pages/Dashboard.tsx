import { useState } from 'react'
import heroImg from '../assets/hero.png'
import reactLogo from '../assets/react.svg'
import viteLogo from '../assets/vite.svg'
import '../App.css'

export default function Dashboard() {
  const [count, setCount] = useState(0)

  // Example function to call backend to generate media
  const handleGenerate = async (type: 'image' | 'video') => {
    const contentName = "ExamplePost"; // In real app, this comes from state
    alert(`Requesting to generate ${type} for ${contentName}. Check network tab!`);
    
    // NOTE: This assumes user is authenticated and cookies are sent.
    // fetch(`/api/generate/${type}/${contentName}`, { method: 'POST' });
  };

  return (
    <>
      <section id="center">
        <div className="hero">
          <img src={heroImg} className="base" width="170" height="179" alt="" />
          <img src={reactLogo} className="framework" alt="React logo" />
          <img src={viteLogo} className="vite" alt="Vite logo" />
        </div>
        <div>
          <h1>Social Media Post Generator</h1>
          <p>
            Create amazing posts & render them to images and videos!
          </p>
        </div>
        <div style={{ display: 'flex', gap: '10px', justifyContent: 'center', marginTop: '20px' }}>
          <button type="button" className="counter" onClick={() => setCount((count) => count + 1)}>
            Count is {count}
          </button>
          <button type="button" className="counter" onClick={() => handleGenerate('image')}>
            Generate Image (1 Credit)
          </button>
          <button type="button" className="counter" onClick={() => handleGenerate('video')}>
            Generate Video (5 Credits)
          </button>
        </div>
      </section>

      <div className="ticks"></div>
      
      <section id="spacer"></section>
    </>
  )
}
