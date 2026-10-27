const http = require('http');
const https = require('https');
const fs = require('fs');
const path = require('path');
const url = require('url');

// Load local .env if available
const envFile = path.resolve(__dirname, '..', '.env');
if (fs.existsSync(envFile)) {
  const envContent = fs.readFileSync(envFile, 'utf8');
  envContent.split(/\r?\n/).forEach(line => {
    const trimmed = line.trim();
    if (trimmed && !trimmed.startsWith('#')) {
      const idx = trimmed.indexOf('=');
      if (idx !== -1) {
        const key = trimmed.slice(0, idx).trim();
        const val = trimmed.slice(idx + 1).trim().replace(/^["']|["']$/g, '');
        if (!process.env[key]) process.env[key] = val;
      }
    }
  });
}

const PORT = process.env.PORT || 5153;
const GEMINI_API_KEY = process.env.GOOGLE_API_KEY || process.env.GEMINI_API_KEY || '';
const GEMINI_MODEL = process.env.GEMINI_MODEL || 'gemini-3.5-flash-lite';

const dataDir = path.join(__dirname, 'src', 'HammadLatifLab.ChatBot.Api', 'Data');
const faqsPath = path.join(dataDir, 'faqs.json');
const testsPath = path.join(dataDir, 'tests.json');

let faqs = [];
let tests = [];

try {
  faqs = JSON.parse(fs.readFileSync(faqsPath, 'utf8'));
  console.log(`[DevServer] Loaded ${faqs.length} FAQs from ${faqsPath}`);
} catch (e) {
  console.error('[DevServer] Failed to load faqs.json:', e.message);
}

try {
  tests = JSON.parse(fs.readFileSync(testsPath, 'utf8'));
  console.log(`[DevServer] Loaded ${tests.length} tests from ${testsPath}`);
} catch (e) {
  console.error('[DevServer] Failed to load tests.json:', e.message);
}

// In-memory chat history: sessionId -> [{ role, content }]
const sessionStore = new Map();

function searchTests(query, top = 5) {
  if (!query) return [];
  const tokens = query.toLowerCase().split(/\s+/).filter(t => t.length > 2);
  if (tokens.length === 0) return [];

  const scored = [];
  for (const t of tests) {
    let score = 0;
    const nameLower = (t.name || '').toLowerCase();
    const keywords = (t.keywords || []).map(k => k.toLowerCase());

    for (const token of tokens) {
      if (nameLower.includes(token)) score += 3;
      if (keywords.some(k => k.includes(token))) score += 2;
    }

    if (score > 0) {
      scored.push({ test: t, score });
    }
  }

  scored.sort((a, b) => b.score - a.score);
  return scored.slice(0, top).map(s => s.test);
}

function searchFaqs(query, top = 3) {
  if (!query) return [];
  const tokens = query.toLowerCase().split(/\s+/).filter(t => t.length > 2);
  if (tokens.length === 0) return [];

  const scored = [];
  for (const f of faqs) {
    let score = 0;
    const qLower = (f.question || '').toLowerCase();
    const quLower = (f.questionUrdu || '').toLowerCase();
    const aLower = (f.answer || '').toLowerCase();
    const keywords = (f.keywords || []).map(k => k.toLowerCase());

    for (const token of tokens) {
      if (qLower.includes(token) || quLower.includes(token)) score += 3;
      if (keywords.some(k => k.includes(token))) score += 2;
      if (aLower.includes(token)) score += 1;
    }

    if (score > 0) {
      scored.push({ faq: f, score });
    }
  }

  scored.sort((a, b) => b.score - a.score);
  return scored.slice(0, top).map(s => s.faq);
}

function buildPrompt(userMessage, matchedTests, matchedFaqs, history) {
  const totalCount = tests.length;
  const sampleNames = tests.slice(0, 300).map(t => t.name).join('; ');

  let prompt = `You are the assistant of Hammad Latif Lab.\n`;
  prompt += `- You may greet and do short, friendly small talk.\n`;
  prompt += `- For any lab fact (number of tests, test names, prices, report time, timings, home sampling, reports) use ONLY the CONTEXT below. Never use your own knowledge.\n`;
  prompt += `- If the user asks how many tests we have, answer with the number in LAB OVERVIEW (${totalCount}).\n`;
  prompt += `- If the user asks about or to list tests, mention a few names (max 10) from ALL TEST NAMES and say they can ask about any specific test for price and time.\n`;
  prompt += `- Price and report time must come ONLY from MATCHED TESTS. Never guess, round or change a number. If the user asks for a price/time but the test is not in MATCHED TESTS, politely ask which exact test they mean.\n`;
  prompt += `- If several tests are matched, list them (max 5) with price and time.\n`;
  prompt += `- If the answer is not in the CONTEXT, say so politely and give the helpline: +92-42-111-455-522 or WhatsApp 0311-1222873.\n`;
  prompt += `- Never give diagnosis, medical advice or medicine suggestions.\n`;
  prompt += `- If the question is unrelated to the lab, politely say you can only help with Hammad Latif Lab services.\n`;
  prompt += `- Reply in the same language the user wrote in (English, Urdu or Roman Urdu). Keep answers short, polite and clear.\n\n`;

  prompt += `GREETING RULES:\n`;
  prompt += `- The chat window already shows a welcome message. NEVER begin a reply with 'Hello', 'Hi', 'Welcome', 'Assalam-o-Alaikum' or any welcome line, and never say 'Welcome to Hammad Latif Lab'.\n`;
  prompt += `- Start directly with the answer.\n`;
  prompt += `- Greet back ONLY if the user's current message is itself only a greeting (e.g. 'hello', 'hi', 'salam', 'assalam o alaikum'), and then reply with ONE short line, for example: 'Wa Alaikum Assalam! How can I help you?'\n`;
  prompt += `- Do not repeat the lab name in every reply.\n\n`;

  if (history && history.length > 0) {
    prompt += `- This is an ongoing conversation. Do NOT greet or welcome the user again.\n\n`;
  }

  prompt += `=== CONTEXT ===\n`;
  prompt += `LAB OVERVIEW:\nTotal tests available: ${totalCount}\n\n`;
  prompt += `ALL TEST NAMES:\n${sampleNames}\n\n`;

  if (matchedTests.length > 0) {
    prompt += `MATCHED TESTS:\n`;
    for (const t of matchedTests) {
      prompt += `Test: ${t.name} | Report time: ${t.time} | Price: Rs. ${t.price}\n`;
    }
    prompt += `\n`;
  }

  if (matchedFaqs.length > 0) {
    prompt += `MATCHED FAQS:\n`;
    for (const f of matchedFaqs) {
      prompt += `Q: ${f.question} | A: ${f.answer}\n`;
    }
    prompt += `\n`;
  }

  if (history && history.length > 0) {
    prompt += `=== CONVERSATION SO FAR ===\n`;
    for (const h of history) {
      prompt += `${h.role}: ${h.content}\n`;
    }
    prompt += `\n`;
  }

  prompt += `=== USER MESSAGE ===\n${userMessage}\n`;
  return prompt;
}

function callGemini(prompt, model = GEMINI_MODEL) {
  return new Promise((resolve, reject) => {
    if (!GEMINI_API_KEY) {
      return reject(new Error('GEMINI_API_KEY not configured'));
    }

    const payload = JSON.stringify({
      contents: [{ role: 'user', parts: [{ text: prompt }] }],
      generationConfig: {
        temperature: 0.2,
        maxOutputTokens: 512
      }
    });

    const req = https.request({
      hostname: 'generativelanguage.googleapis.com',
      path: `/v1beta/models/${model}:generateContent?key=${GEMINI_API_KEY}`,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(payload)
      },
      timeout: 10000
    }, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => {
        if (res.statusCode >= 200 && res.statusCode < 300) {
          try {
            const parsed = JSON.parse(data);
            const text = parsed?.candidates?.[0]?.content?.parts?.[0]?.text;
            resolve(text || '');
          } catch (e) {
            reject(new Error('Failed to parse Gemini response'));
          }
        } else {
          reject(new Error(`Gemini status ${res.statusCode}: ${data}`));
        }
      });
    });

    req.on('error', reject);
    req.on('timeout', () => {
      req.destroy();
      reject(new Error('Gemini API timeout'));
    });

    req.write(payload);
    req.end();
  });
}

function getFallbackMessage(userMessage) {
  const lower = (userMessage || '').toLowerCase();
  const isUrdu = ['hai', 'kya', 'kiya', 'kitna', 'kitne', 'ka', 'ki', 'ke', 'ko', 'mein', 'me ', 'bata', 'karein', 'bataen', 'hain', 'kab', 'mujhe'].some(w => lower.includes(w));

  if (isUrdu) {
    return 'Maazrat, yeh test ya maloomat hamaray mojooda record mein dastiyab nahi hai.\n\n' +
           'Mazeed maloomat, charges ya test booking ke liye barah-e-karam hamari 24/7 Helpline: **+92-42-111-455-522** ya WhatsApp: **0311-1222873** par rabta karein. Hamara amla aapki poori rehnumai ke liye 24/7 hazir hai.';
  }

  return 'Sorry, this test or information is not available in our current records.\n\n' +
         'For further details, test booking, or queries, please contact our 24/7 Helpline at **+92-42-111-455-522** or WhatsApp at **0311-1222873**. Our team is available 24/7 to assist you.';
}

const server = http.createServer(async (req, res) => {
  // CORS
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, x-requested-with');

  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  const parsedUrl = url.parse(req.url, true);
  const pathname = parsedUrl.pathname.replace(/\/$/, '') || '/';

  if (pathname === '/health') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ status: 'Healthy', timestamp: new Date().toISOString() }));
    return;
  }

  if (pathname === '/api/Tests' || pathname === '/api/tests') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(tests));
    return;
  }

  if (pathname === '/api/Tests/search' || pathname === '/api/tests/search') {
    const q = parsedUrl.query.query || '';
    const results = searchTests(q, 10);
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(results));
    return;
  }

  if (pathname === '/api/Faqs' || pathname === '/api/faqs') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(faqs));
    return;
  }

  if (pathname === '/api/Chat/message' || pathname === '/api/chat/message') {
    if (req.method !== 'POST') {
      res.writeHead(405, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ error: 'Method Not Allowed' }));
      return;
    }

    let body = '';
    req.on('data', chunk => body += chunk);
    req.on('end', async () => {
      try {
        const payload = JSON.parse(body || '{}');
        const userMessage = (payload.message || '').trim();
        const sessionId = payload.sessionId || `sess_${Math.random().toString(36).substring(2, 10)}`;

        if (!userMessage) {
          res.writeHead(400, { 'Content-Type': 'application/json' });
          res.end(JSON.stringify({ error: 'Message cannot be empty' }));
          return;
        }

        const history = sessionStore.get(sessionId) || [];
        const matchedTests = searchTests(userMessage, 5);
        const matchedFaqs = searchFaqs(userMessage, 3);

        const prompt = buildPrompt(userMessage, matchedTests, matchedFaqs, history);

        let answer = '';
        let isGrounded = true;

        try {
          answer = await callGemini(prompt, GEMINI_MODEL);
          if (!answer || answer.trim() === '') {
            answer = getFallbackMessage(userMessage);
            isGrounded = false;
          }
        } catch (geminiErr) {
          console.warn(`[DevServer] Gemini call failed (${geminiErr.message}). Using fallback.`);
          if (matchedFaqs.length > 0) {
            answer = matchedFaqs[0].answer;
          } else if (matchedTests.length > 0) {
            const t = matchedTests[0];
            answer = `Test **${t.name}** is available at Hammad Latif Lab.\n• Price: **Rs. ${t.price}**\n• Report Time: **${t.time}**\nFor booking or home sampling, contact 24/7 Helpline: **+92-42-111-455-522**.`;
          } else {
            answer = getFallbackMessage(userMessage);
            isGrounded = false;
          }
        }

        history.push({ role: 'User', content: userMessage });
        history.push({ role: 'Assistant', content: answer });
        while (history.length > 6) history.shift();
        sessionStore.set(sessionId, history);

        const responseObj = {
          answer: answer.trim(),
          isGrounded,
          matchedFaqIds: matchedFaqs.map(f => f.id),
          matchedTestIds: matchedTests.map(t => t.id),
          sessionId
        };

        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify(responseObj));
      } catch (err) {
        console.error('[DevServer] Error handling /api/Chat/message:', err);
        res.writeHead(500, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ error: 'Internal Server Error' }));
      }
    });
    return;
  }

  res.writeHead(404, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify({ error: 'Not Found', path: pathname }));
});

server.listen(PORT, () => {
  console.log(`[DevServer] Hammad Latif Lab API running on http://localhost:${PORT}`);
  console.log(`[DevServer] Health endpoint: http://localhost:${PORT}/health`);
  console.log(`[DevServer] Chat endpoint: http://localhost:${PORT}/api/Chat/message`);
});
