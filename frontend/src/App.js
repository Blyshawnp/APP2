import React, { useState, useEffect, useCallback, useRef } from 'react';
import '@/App.css';
import api from './api';
import { ModalProvider } from './components/ModalProvider';
import HomePage from './pages/HomePage';
import SetupPage from './pages/SetupPage';
import BasicsPage from './pages/BasicsPage';
import CallsPage from './pages/CallsPage';
import SupTransferPage from './pages/SupTransferPage';
import NewbieShiftPage from './pages/NewbieShiftPage';
import ReviewPage from './pages/ReviewPage';
import HistoryPage from './pages/HistoryPage';
import SettingsPage from './pages/SettingsPage';
import HelpPage from './pages/HelpPage';

const NAV_ITEMS = [
  { key: 'home', label: 'Home', icon: 'home' },
  { key: 'basics', label: 'The Basics', icon: 'clipboard-list' },
  { key: 'calls', label: 'Calls', icon: 'phone' },
  { key: 'suptransfer', label: 'Sup Transfer', icon: 'repeat' },
  { key: 'review', label: 'Review', icon: 'file-text' },
  { key: 'history', label: 'History', icon: 'bar-chart-2' },
  { key: 'settings', label: 'Settings', icon: 'settings' },
  { key: 'help', label: 'Help', icon: 'help-circle' },
];

const LINK_ITEMS = [
  { key: 'discord', label: 'Discord Post', icon: 'message-square' },
  { key: 'tracker', label: 'My Tracker Sheet', icon: 'bar-chart' },
  { key: 'cert', label: 'Cert Spreadsheet', icon: 'file-spreadsheet' },
];

function PageRouter({ page, navigate }) {
  const props = { onNavigate: navigate };
  switch (page) {
    case 'setup': return <SetupPage {...props} />;
    case 'home': return <HomePage {...props} />;
    case 'basics': return <BasicsPage {...props} />;
    case 'calls': return <CallsPage {...props} />;
    case 'suptransfer': return <SupTransferPage {...props} />;
    case 'newbieshift': return <NewbieShiftPage {...props} />;
    case 'review': return <ReviewPage {...props} />;
    case 'history': return <HistoryPage {...props} />;
    case 'settings': return <SettingsPage {...props} />;
    case 'help': return <HelpPage {...props} />;
    default: return <HomePage {...props} />;
  }
}

function App() {
  const [page, setPage] = useState('home');
  const [settings, setSettings] = useState(null);
  const [tickerMessages, setTickerMessages] = useState([]);
  const [discordOpen, setDiscordOpen] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const saved = localStorage.getItem('mts-theme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);
  }, []);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const s = await api.getSettings();
        if (cancelled) return;
        setSettings(s);
        if (!s.setup_complete) setPage('setup');
      } catch (err) {
        if (!cancelled) {
          // Backend unreachable — settings will remain null, loading state handles UI
        }
      }
      if (!cancelled) setLoading(false);
    })();
    return () => { cancelled = true; };
  }, []);

  useEffect(() => {
    const fetchTicker = async () => {
      try {
        const data = await api.getTicker();
        if (data.messages?.length > 0) setTickerMessages(data.messages);
      } catch (err) {
        // Ticker fetch is non-critical; silently retry on next interval
      }
    };
    fetchTicker();
    const interval = setInterval(fetchTicker, 90000);
    return () => clearInterval(interval);
  }, []);

  const navigate = useCallback((p) => setPage(p), []);

  const handleLinkClick = useCallback((key) => {
    if (key === 'discord') { setDiscordOpen(true); return; }
    if (!settings) return;
    const url = settings.cert_sheet_url;
    if ((key === 'tracker' || key === 'cert') && url) {
      window.open(url, '_blank');
    }
  }, [settings]);

  const handleExit = useCallback(() => {
    const root = document.getElementById('root');
    if (root) {
      while (root.firstChild) root.removeChild(root.firstChild);
      const msg = document.createElement('div');
      msg.style.cssText = 'display:flex;align-items:center;justify-content:center;height:100vh;color:#888;';
      msg.textContent = 'You can close this window now.';
      root.appendChild(msg);
    }
  }, []);

  const tickerContent = tickerMessages.length > 0
    ? tickerMessages.join('  \u25C6  ') + '  \u25C6  ' + tickerMessages.join('  \u25C6  ')
    : 'Welcome to Mock Testing Suite v3.0';

  return (
    <ModalProvider>
      <div className="app-root" data-testid="app-root">
        {/* Ticker */}
        <div className="ticker-bar">
          <div className="ticker-track">
            <span className="ticker-content">{tickerContent}</span>
          </div>
        </div>

        <div className="app-shell">
          {/* Sidebar */}
          <aside className="sidebar" data-testid="sidebar">
            <div className="sidebar-brand">
              <div className="sidebar-logo">MTS</div>
              <div className="sidebar-title">Mock Testing<br />Suite</div>
              <div className="sidebar-version">v3.0</div>
            </div>
            <nav className="sidebar-nav">
              {NAV_ITEMS.map(item => (
                <button key={item.key} className={`nav-btn ${page === item.key ? 'active' : ''}`} onClick={() => navigate(item.key)} data-testid={`nav-${item.key}`}>
                  <span className="nav-icon"><NavIcon name={item.icon} /></span>
                  <span className="nav-label">{item.label}</span>
                </button>
              ))}
            </nav>
            <div className="sidebar-divider" />
            <div className="sidebar-links">
              {LINK_ITEMS.map(item => (
                <button key={item.key} className="link-btn" onClick={() => handleLinkClick(item.key)} data-testid={`link-${item.key}`}>
                  <span className="nav-icon"><NavIcon name={item.icon} /></span>
                  <span className="nav-label">{item.label}</span>
                </button>
              ))}
            </div>
            <div className="sidebar-footer">
              <button className="exit-btn" onClick={handleExit} data-testid="exit-btn">Exit App</button>
            </div>
          </aside>

          {/* Main */}
          <main className="content-area">
            <div className="page-content" data-testid="page-content">
              {loading
                ? <div className="page-loading">Connecting to server...</div>
                : <PageRouter page={page} navigate={navigate} />
              }
            </div>
            <div className="status-bar">
              <span id="status-text"></span>
              <span className="status-spacer" />
              <span>Mock Testing Suite v3.0 — By Shawn P. Bly</span>
            </div>
          </main>
        </div>

        {/* Discord Modal */}
        {discordOpen && <DiscordModal settings={settings} onClose={() => setDiscordOpen(false)} />}
      </div>
    </ModalProvider>
  );
}

function DiscordModal({ settings, onClose }) {
  const templates = settings?.discord_templates || [];
  return (
    <div className="modal-overlay open" onClick={e => { if (e.target.classList.contains('modal-overlay')) onClose(); }} data-testid="discord-modal">
      <div className="modal" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Discord Post Templates</h2>
          <button className="modal-close" onClick={onClose}>&times;</button>
        </div>
        <div className="modal-body">
          {templates.length === 0 ? (
            <p className="text-muted" style={{ padding: 20 }}>No templates configured. Add them in Settings - Discord tab.</p>
          ) : templates.map(([title, message], i) => (
            <DiscordRow key={i} title={title} message={message} />
          ))}
        </div>
      </div>
    </div>
  );
}

function DiscordRow({ title, message }) {
  const [copied, setCopied] = useState(false);
  return (
    <div className="discord-row">
      <div className="discord-title">{title}</div>
      <div className="discord-msg">{message}</div>
      <button className={`discord-copy ${copied ? 'copied' : ''}`} onClick={() => {
        navigator.clipboard.writeText(message);
        setCopied(true);
        setTimeout(() => setCopied(false), 1500);
      }}>{copied ? 'Copied!' : 'Copy'}</button>
    </div>
  );
}

function NavIcon({ name }) {
  const icons = {
    'home': 'H', 'clipboard-list': 'B', 'phone': 'C', 'repeat': 'S', 'file-text': 'R',
    'bar-chart-2': 'Hi', 'settings': 'St', 'help-circle': '?',
    'message-square': 'D', 'bar-chart': 'T', 'file-spreadsheet': 'Cs',
  };
  return <span style={{ fontSize: 14, fontWeight: 700 }}>{icons[name] || name[0]?.toUpperCase()}</span>;
}

export default App;
