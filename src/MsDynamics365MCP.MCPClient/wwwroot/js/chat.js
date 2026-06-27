(() => {
  'use strict';

  // ── DOM refs ─────────────────────────────────────────
  const panel       = document.getElementById('chat-panel');
  const toggle      = document.getElementById('chat-toggle');
  const mainContent = document.getElementById('main-content');
  const messages    = document.getElementById('chat-messages');
  const input       = document.getElementById('chat-input');
  const sendBtn     = document.getElementById('send-btn');
  const clearBtn    = document.getElementById('clear-btn');
  const closeBtn    = document.getElementById('close-btn');

  // ── State ─────────────────────────────────────────────
  let isLoading = false;
  let typingEl  = null;
  let sessionId = crypto.randomUUID();

  // ── Panel open / close ─────────────────────────────────
  function openPanel() {
    panel.classList.remove('collapsed');
    mainContent.classList.add('panel-open');
    toggle.classList.add('hidden');
    input.focus();
  }

  function closePanel() {
    panel.classList.add('collapsed');
    mainContent.classList.remove('panel-open');
    toggle.classList.remove('hidden');
  }

  toggle.addEventListener('click', openPanel);
  closeBtn.addEventListener('click', closePanel);

  // ── Auto-grow textarea ─────────────────────────────────
  input.addEventListener('input', () => {
    input.style.height = 'auto';
    input.style.height = Math.min(input.scrollHeight, 120) + 'px';
  });

  // ── Send on Enter (Shift+Enter = newline) ──────────────
  input.addEventListener('keydown', e => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if (!isLoading) sendMessage();
    }
  });

  sendBtn.addEventListener('click', () => { if (!isLoading) sendMessage(); });
  clearBtn.addEventListener('click', clearMessages);

  // ── Suggestion chips ───────────────────────────────────
  document.querySelectorAll('.suggestion-chip').forEach(chip => {
    chip.addEventListener('click', () => {
      if (isLoading) return;
      input.value = chip.textContent;
      input.dispatchEvent(new Event('input'));
      sendMessage();
    });
  });

  // ── Send message ───────────────────────────────────────
  async function sendMessage() {
    const text = input.value.trim();
    if (!text || isLoading) return;

    removeErrorToast();
    appendMessage('user', text);

    input.value = '';
    input.style.height = 'auto';
    setLoading(true);

    try {
      const res = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: text, sessionId })
      });

      const data = await res.json();

      if (!res.ok || data.isSuccess === false) {
        const errMsg = data.error || data.detail || `HTTP ${res.status}`;
        showError(errMsg);
        return;
      }

      appendMessage('ai', data.response, {
        tools: data.toolsInvoked,
        model: data.model
      });

    } catch (err) {
      showError('Network error — is MCPClient running? ' + err.message);
    } finally {
      setLoading(false);
    }
  }

  // ── Render helpers ─────────────────────────────────────
  function appendMessage(role, text, meta = {}) {
    removeTyping();

    const wrap = document.createElement('div');
    wrap.className = `message ${role}`;

    // Avatar
    const avatar = document.createElement('div');
    avatar.className = 'msg-avatar';
    avatar.textContent = role === 'user' ? '👤' : '🤖';
    wrap.appendChild(avatar);

    // Bubble
    const bubble = document.createElement('div');
    bubble.className = 'msg-bubble';
    bubble.textContent = text;
    wrap.appendChild(bubble);

    // Meta row
    const metaRow = document.createElement('div');
    metaRow.className = 'msg-meta';
    metaRow.textContent = formatTime(new Date());

    if (role === 'ai' && meta.tools?.length) {
      const badge = document.createElement('span');
      badge.className = 'tools-badge';
      badge.innerHTML = `🔧 ${meta.tools.join(', ')}`;
      metaRow.appendChild(badge);
    }

    wrap.appendChild(metaRow);
    messages.appendChild(wrap);
    scrollToBottom();
  }

  function showTyping() {
    typingEl = document.createElement('div');
    typingEl.className = 'typing-indicator';
    typingEl.innerHTML = `
      <div class="typing-dots">
        <span></span><span></span><span></span>
      </div>
      <span class="typing-label">AI is thinking…</span>`;
    messages.appendChild(typingEl);
    scrollToBottom();
  }

  function removeTyping() {
    typingEl?.remove();
    typingEl = null;
  }

  function showError(msg) {
    removeErrorToast();
    const el = document.createElement('div');
    el.className = 'error-toast';
    el.id = 'error-toast';
    el.textContent = '⚠ ' + msg;
    panel.querySelector('.chat-messages').after(el);
  }

  function removeErrorToast() {
    document.getElementById('error-toast')?.remove();
  }

  function setLoading(loading) {
    isLoading = loading;
    sendBtn.disabled = loading;
    input.disabled   = loading;
    if (loading) showTyping();
    else         removeTyping();
  }

  function scrollToBottom() {
    messages.scrollTop = messages.scrollHeight;
  }

  function clearMessages() {
    messages.innerHTML = `
      <div class="msg-system">
        💡 Session started — ask about contacts, accounts, opportunities, or cases.
      </div>`;
    sessionId = crypto.randomUUID();
    removeErrorToast();
  }

  function formatTime(d) {
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  // ── Init: open panel by default ────────────────────────
  openPanel();
})();
