import { Routes, Route } from 'react-router-dom'
import Dashboard from './pages/Dashboard'
import RenderView from './pages/RenderView'
import './App.css'

function App() {
  return (
    <Routes>
      <Route path="/" element={<Dashboard />} />
      <Route path="/render" element={<RenderView />} />
    </Routes>
  )
}

export default App
