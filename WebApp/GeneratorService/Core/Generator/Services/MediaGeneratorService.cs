using System.Diagnostics;
using Microsoft.Playwright;

namespace GeneratorService.Core.Generator.Services;

public interface IMediaGeneratorService {
    Task<byte[]> GenerateImageAsync(string frontendUrl);
    Task<byte[]> GenerateVideoAsync(string frontendUrl, int fps = 60, int durationSeconds = 5);
}

public class MediaGeneratorService : IMediaGeneratorService {

    public async Task<byte[]> GenerateImageAsync(string frontendUrl) {
        using var playwright = await Playwright.CreateAsync();
        
        // Use Chromium for better rendering compatibility
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions {
            ViewportSize = new ViewportSize { Width = 1080, Height = 1920 } // Default Reels/TikTok resolution
        });
        
        var page = await context.NewPageAsync();
        
        // Go to the hidden render route on the frontend
        await page.GotoAsync(frontendUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        
        // Optionally wait for a custom event if frontend signals readiness
        try {
            await page.WaitForSelectorAsync("#render-ready", new PageWaitForSelectorOptions { Timeout = 5000 });
        } catch (TimeoutException) {
            // If the selector doesn't appear, fallback to taking the screenshot anyway
        }

        return await page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Png });
    }

    public async Task<byte[]> GenerateVideoAsync(string frontendUrl, int fps = 60, int durationSeconds = 5) {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var context = await browser.NewContextAsync(new BrowserNewContextOptions {
            ViewportSize = new ViewportSize { Width = 1080, Height = 1920 }
        });
        
        var page = await context.NewPageAsync();
        await page.GotoAsync(frontendUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        
        try {
            await page.WaitForSelectorAsync("#render-ready", new PageWaitForSelectorOptions { Timeout = 5000 });
        } catch (TimeoutException) { }

        // Inject script to pause CSS animations/transitions and take manual control over time
        await page.EvaluateAsync(@"() => {
            // Disable native transitions so we can control it if we want
            // Or we mock Date.now() / performance.now()
            window.__PLAYWRIGHT_TIME = 0;
            const originalNow = performance.now;
            performance.now = () => window.__PLAYWRIGHT_TIME;
        }");

        int totalFrames = fps * durationSeconds;
        double frameDurationMs = 1000.0 / fps;
        
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try {
            // Frame-by-frame rendering loop
            for (int i = 0; i < totalFrames; i++) {
                var framePath = Path.Combine(tempDir, $"frame_{i:D5}.png");
                
                await page.EvaluateAsync($@"() => {{
                    window.__PLAYWRIGHT_TIME = {i * frameDurationMs};
                    // If using a custom animation controller (like GSAP or Remotion), we step it here
                    if (window.stepAnimation) window.stepAnimation({i * frameDurationMs});
                }}");

                var bytes = await page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Png });
                await File.WriteAllBytesAsync(framePath, bytes);
            }

            // Stitch frames using FFmpeg
            var outputPath = Path.Combine(tempDir, "output.mp4");
            
            // Requires ffmpeg to be installed and available in PATH
            var ffmpegProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "ffmpeg",
                    Arguments = $"-framerate {fps} -i \"{tempDir}/frame_%05d.png\" -c:v libx264 -pix_fmt yuv420p \"{outputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            ffmpegProcess.Start();
            await ffmpegProcess.WaitForExitAsync();
            
            if (ffmpegProcess.ExitCode != 0) {
                var error = await ffmpegProcess.StandardError.ReadToEndAsync();
                throw new Exception($"FFmpeg failed: {error}");
            }

            return await File.ReadAllBytesAsync(outputPath);
        }
        finally {
            if (Directory.Exists(tempDir)) {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
