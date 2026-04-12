import React, { useState, useEffect } from 'react';
import api from '../api';
import { useModal } from '../components/ModalProvider';

export default function HomePage({ onNavigate }) {
  const modal = useModal();
  const [settings, setSettings] = useState({});
  const [stats, setStats] = useState({});
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const [s, st, h] = await Promise.all([api.getSettings(), api.getHistoryStats(), api.getHistory()]);
        setSettings(s); setStats(st); setHistory(h || []);
      } catch (e) { console.error(e); }
      setLoading(false);
    })();
  }, []);

  if (loading) return <div className="page-loading">Loading...</div>;

  const name = settings.display_name || settings.tester_name || 'Tester';
  const recent = (history || []).slice(0, 5);
  const badgeClass = (s) => ({ Pass: 'badge-pass', Fail: 'badge-fail', Incomplete: 'badge-incomplete', 'NC/NS': 'badge-ncns' }[s] || 'badge-ncns');

  return (
    <div data-testid="home-page">
      <div className="home-header">
        <h1>Welcome, {name}!</h1>
        <p className="text-muted">Mock Testing Suite — Certification</p>
      </div>
      <div className="stats-row">
        <StatCard label="Total Sessions" value={stats.total || 0} />
        <StatCard label="Passed" value={stats.passes || 0} color="var(--color-success)" />
        <StatCard label="Failed" value={stats.fails || 0} color="var(--color-danger)" />
        <StatCard label="NC/NS" value={stats.ncns || 0} color="var(--text-tertiary)" />
        <StatCard label="Pass Rate" value={`${stats.pass_rate || 0}%`} color="var(--color-success)" />
      </div>
      <div className="home-section">
        <h3>Recent Sessions</h3>
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          {recent.length === 0 ? (
            <div style={{ padding: 20, textAlign: 'center', color: 'var(--text-tertiary)' }}>No sessions yet. Start testing to see history here.</div>
          ) : recent.map((s, i) => (
            <div key={i} className="recent-row">
              <span className="recent-date">{s.timestamp || 'Unknown'}</span>
              <span className="recent-name">{s.candidate || 'Unknown'}</span>
              <span className={`badge ${badgeClass(s.status)}`}>{s.status || '?'}</span>
            </div>
          ))}
        </div>
      </div>
      <div className="home-actions">
        <button className="btn btn-primary btn-lg" onClick={() => onNavigate('basics')} data-testid="home-start-btn">Start New Session</button>
        <button className="btn btn-success btn-lg" onClick={() => onNavigate('basics')} data-testid="home-sup-only-btn">Supervisor Transfer Only</button>
        <button className="btn btn-muted" onClick={() => onNavigate('history')} data-testid="home-history-btn">Full History</button>
      </div>
    </div>
  );
}

function StatCard({ label, value, color }) {
  return (
    <div className="stat-card">
      <div className="stat-label">{label}</div>
      <div className="stat-value" style={color ? { color } : {}}>{value}</div>
    </div>
  );
}
