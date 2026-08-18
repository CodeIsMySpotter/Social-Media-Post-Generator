import { createIcons, Image, Download, ChevronDown, Code2 } from 'lucide';
import * as htmlToImage from 'html-to-image';

// Initialize Icons
createIcons({
  icons: {
    Image,
    Download,
    ChevronDown,
    Code2
  }
});

// DOM Elements
const formatSelect = document.getElementById('format-select');
const htmlInput = document.getElementById('html-input');
const previewWorkspace = document.getElementById('preview-workspace');
const previewWrapper = document.getElementById('preview-wrapper');
const previewContent = document.getElementById('preview-content');
const resolutionLabel = document.getElementById('resolution-label');
const generateBtn = document.getElementById('generate-btn');

// State
let currentWidth = 1080;
let currentHeight = 1350;
let isGenerating = false;

// Initialize
function init() {
  updateFormat();
  
  // Event Listeners
  formatSelect.addEventListener('change', updateFormat);
  htmlInput.addEventListener('input', updatePreview);
  window.addEventListener('resize', updateScale);
  generateBtn.addEventListener('click', generateImage);
}

// Update format dimensions based on selection
function updateFormat() {
  const selectedOption = formatSelect.options[formatSelect.selectedIndex];
  currentWidth = parseInt(selectedOption.dataset.width);
  currentHeight = parseInt(selectedOption.dataset.height);
  
  resolutionLabel.textContent = `${currentWidth} x ${currentHeight}`;
  
  // Apply dimensions to preview content
  previewContent.style.width = `${currentWidth}px`;
  previewContent.style.height = `${currentHeight}px`;
  
  updateScale();
}

// Update preview scale to fit the workspace
function updateScale() {
  const workspaceRect = previewWorkspace.getBoundingClientRect();
  
  // Add some padding (40px total = 20px each side)
  const availableWidth = workspaceRect.width - 40;
  const availableHeight = workspaceRect.height - 40;
  
  const scaleX = availableWidth / currentWidth;
  const scaleY = availableHeight / currentHeight;
  
  // Use the smaller scale to ensure it fits entirely
  const scale = Math.min(scaleX, scaleY);
  
  previewWrapper.style.transform = `scale(${scale})`;
}

// Inject HTML into preview
function updatePreview() {
  const html = htmlInput.value;
  
  if (!html.trim()) {
    previewContent.innerHTML = `
      <div class="placeholder-content">
        <i data-lucide="code-2"></i>
        <p>Paste HTML to see preview</p>
      </div>
    `;
    // Re-init icon for placeholder
    createIcons({
      icons: { Code2 },
      nameAttr: 'data-lucide',
      attrs: {
        class: 'lucide lucide-code-2'
      }
    });
    return;
  }
  
  previewContent.innerHTML = html;
}

// Generate image using html-to-image
async function generateImage() {
  if (isGenerating) return;
  
  try {
    isGenerating = true;
    
    // UI Feedback
    const originalText = generateBtn.innerHTML;
    generateBtn.innerHTML = 'Generating...';
    generateBtn.disabled = true;
    generateBtn.style.opacity = '0.7';

    // The wrapper is scaled, but we target the content which has original dimensions
    // html-to-image works well with this setup, but just in case, we can pass explicit dimensions
    const dataUrl = await htmlToImage.toPng(previewContent, {
      width: currentWidth,
      height: currentHeight,
      pixelRatio: 1, // Keep original size
      style: {
        transform: 'none', // Ensure no transforms are applied during render
      }
    });
    
    // Trigger download
    const link = document.createElement('a');
    link.download = `postgen_${currentWidth}x${currentHeight}_${Date.now()}.png`;
    link.href = dataUrl;
    link.click();
    
    // Restore UI
    generateBtn.innerHTML = originalText;
    generateBtn.disabled = false;
    generateBtn.style.opacity = '1';
    
  } catch (error) {
    console.error('Error generating image:', error);
    alert('Failed to generate image. Please check the console for details.');
    
    // Restore UI
    generateBtn.innerHTML = `<i data-lucide="download"></i> Generate Image`;
    generateBtn.disabled = false;
    generateBtn.style.opacity = '1';
    
    // Re-init icon
    createIcons({ icons: { Download } });
  } finally {
    isGenerating = false;
  }
}

// Start
init();
