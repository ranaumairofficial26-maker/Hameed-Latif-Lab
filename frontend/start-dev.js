const { spawn } = require('child_process');
const http = require('http');
const path = require('path');

const BACKEND_PORT = 5153;
const FRONTEND_PORT = 4100;

function isBackendRunning() {
  return new Promise((resolve) => {
    const req = http.get(`http://localhost:${BACKEND_PORT}/health`, (res) => {
      resolve(res.statusCode === 200);
    });
    req.on('error', () => resolve(false));
    req.setTimeout(1500, () => {
      req.destroy();
      resolve(false);
    });
  });
}

async function start() {
  const running = await isBackendRunning();
  if (!running) {
    console.log(`[Dev] Starting Hammad Latif Lab backend on http://localhost:${BACKEND_PORT}...`);
    const serverPath = path.resolve(__dirname, '..', 'backend', 'dev-server.js');
    const backend = spawn('node', [serverPath], { stdio: 'inherit' });

    backend.on('error', (err) => {
      console.error('[Dev] Failed to start backend dev-server:', err);
    });

    process.on('exit', () => backend.kill());
  } else {
    console.log(`[Dev] Backend already active on http://localhost:${BACKEND_PORT}`);
  }

  console.log(`[Dev] Starting Angular on http://localhost:${FRONTEND_PORT}...`);
  const ngCmd = process.platform === 'win32' ? 'npx.cmd' : 'npx';
  const frontend = spawn(ngCmd, ['ng', 'serve', '--port', String(FRONTEND_PORT)], { stdio: 'inherit', cwd: __dirname });

  frontend.on('error', (err) => {
    console.error('[Dev] Failed to start Angular:', err);
  });
}

start();
