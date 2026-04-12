import React, { useState, useEffect } from 'react';
import api from '../api';
import { useModal } from '../components/ModalProvider';

const TABS = ['General', 'Gemini', 'Google Sheet', 'Calendar', 'Discord', 'Payment'];

export default function SettingsPage({ onNavigate }) {
  const modal = useModal();
  const [tab, setTab] = useState(0);
  const [s, setS] = useState({});
  const [discord, setDiscord] = useState([]);
  const [pay, setPay] = useState({});

  useEffect(() => {
    api.getSettings().then(data => {
      setS(data);
      setDiscord(data.discord_templates || []);
      setPay(data.payment || {});
    }).catch(() => {});
  }, []);

  const set = (key, val) => setS(prev => ({ ...prev, [key]: val }));

  const handleSave = async () => {
    const payload = {
      tester_name: s.tester_name || '',
      display_name: s.display_name || '',
      form_url: s.form_url || '',
      cert_sheet_url: s.cert_sheet_url || '',
      enable_gemini: s.enable_gemini || false,
      gemini_key: s.gemini_key || '',
      enable_sheets: s.enable_sheets || false,
      sheet_id: s.sheet_id || '',
      worksheet: s.worksheet || 'Sheet1',
      service_account_path: s.service_account_path || 'service_account.json',
      enable_calendar: s.enable_calendar || false,
      discord_templates: discord,
      payment: pay,
    };
    try {
      await api.saveSettings(payload);
      await modal.alert('Settings Saved', 'Your settings have been saved successfully.');
    } catch (e) { await modal.error('Save Failed', e.message); }
  };

  const addDiscord = () => setDiscord(prev => [...prev, ['', '']]);
  const removeDiscord = (i) => setDiscord(prev => prev.filter((_, idx) => idx !== i));
  const updateDiscord = (i, field, val) => setDiscord(prev => prev.map((item, idx) => idx === i ? (field === 0 ? [val, item[1]] : [item[0], val]) : item));

  return (
    <div data-testid="settings-page">
      <h1 style={{ marginBottom: 24 }}>Settings</h1>
      <div className="tabs-header">
        {TABS.map((t, i) => (
          <button key={t} className={`tab-btn ${i === tab ? 'active' : ''}`} onClick={() => setTab(i)} data-testid={`settings-tab-${i}`}>{t}</button>
        ))}
      </div>

      {tab === 0 && (
        <div className="card" data-testid="settings-general">
          <div className="form-row"><label>Tester Name</label><input type="text" value={s.tester_name || ''} onChange={e => set('tester_name', e.target.value)} style={{ maxWidth: 300 }} data-testid="settings-name" /></div>
          <div className="form-row"><label>Display Name</label><input type="text" value={s.display_name || ''} onChange={e => set('display_name', e.target.value)} placeholder="Home screen greeting" style={{ maxWidth: 300 }} data-testid="settings-display" /></div>
          <div className="form-row"><label>Cert Form URL</label><input type="text" value={s.form_url || ''} onChange={e => set('form_url', e.target.value)} style={{ maxWidth: 500 }} data-testid="settings-form-url" /></div>
          <div className="form-row"><label>Cert Sheet URL</label><input type="text" value={s.cert_sheet_url || ''} onChange={e => set('cert_sheet_url', e.target.value)} style={{ maxWidth: 500 }} data-testid="settings-sheet-url" /></div>
          <div style={{ marginTop: 16 }}>
            <label className="text-sm font-bold">Theme</label>
            <button className="btn btn-ghost btn-sm" style={{ marginLeft: 8 }} onClick={() => {
              const current = document.documentElement.getAttribute('data-theme') || 'dark';
              const next = current === 'dark' ? 'light' : 'dark';
              document.documentElement.setAttribute('data-theme', next);
              localStorage.setItem('mts-theme', next);
              api.saveSettings({ theme: next });
            }} data-testid="settings-theme-toggle">Toggle Light/Dark</button>
          </div>
        </div>
      )}

      {tab === 1 && (
        <div className="card" data-testid="settings-gemini">
          <label className="checkbox-label" style={{ marginBottom: 16 }}>
            <input type="checkbox" checked={s.enable_gemini || false} onChange={e => set('enable_gemini', e.target.checked)} data-testid="settings-gemini-on" />
            <span>Enable Gemini AI Summaries</span>
          </label>
          <div className="form-row"><label>API Key</label><input type="password" value={s.gemini_key || ''} onChange={e => set('gemini_key', e.target.value)} placeholder="From aistudio.google.com" style={{ maxWidth: 400 }} data-testid="settings-gemini-key" /></div>
          <p className="text-muted text-sm" style={{ marginTop: 16 }}>Go to aistudio.google.com - Get API Key - Create API Key - Paste above.</p>
        </div>
      )}

      {tab === 2 && (
        <div className="card" data-testid="settings-sheets">
          <label className="checkbox-label" style={{ marginBottom: 16 }}>
            <input type="checkbox" checked={s.enable_sheets || false} onChange={e => set('enable_sheets', e.target.checked)} data-testid="settings-sheets-on" />
            <span>Enable Google Sheets Backup</span>
          </label>
          <div className="form-row"><label>Spreadsheet ID</label><input type="text" value={s.sheet_id || ''} onChange={e => set('sheet_id', e.target.value)} style={{ maxWidth: 400 }} /></div>
          <div className="form-row"><label>Worksheet Name</label><input type="text" value={s.worksheet || 'Sheet1'} onChange={e => set('worksheet', e.target.value)} style={{ maxWidth: 200 }} /></div>
          <div className="form-row"><label>Service Account File</label><input type="text" value={s.service_account_path || 'service_account.json'} onChange={e => set('service_account_path', e.target.value)} style={{ maxWidth: 400 }} /></div>
        </div>
      )}

      {tab === 3 && (
        <div className="card" data-testid="settings-calendar">
          <label className="checkbox-label">
            <input type="checkbox" checked={s.enable_calendar || false} onChange={e => set('enable_calendar', e.target.checked)} data-testid="settings-cal-on" />
            <span>Enable Google Calendar for Newbie Shifts</span>
          </label>
          <p className="text-muted text-sm" style={{ marginTop: 16 }}>The calendar button generates a Google Calendar template URL. No additional setup needed.</p>
        </div>
      )}

      {tab === 4 && (
        <div className="card" data-testid="settings-discord">
          <p className="text-muted text-sm" style={{ marginBottom: 16 }}>These templates appear in the Discord Post popup. Edit them here.</p>
          {discord.map(([title, msg], i) => (
            <div key={i} style={{ display: 'flex', gap: 8, alignItems: 'flex-start', marginBottom: 8 }}>
              <input type="text" value={title} onChange={e => updateDiscord(i, 0, e.target.value)} placeholder="Title" style={{ maxWidth: 140 }} />
              <textarea value={msg} onChange={e => updateDiscord(i, 1, e.target.value)} rows={2} style={{ flex: 1 }} />
              <button className="btn btn-danger btn-sm" onClick={() => removeDiscord(i)} style={{ flexShrink: 0 }}>X</button>
            </div>
          ))}
          <button className="btn btn-primary btn-sm" onClick={addDiscord} style={{ marginTop: 16 }} data-testid="settings-discord-add">+ Add Template</button>
        </div>
      )}

      {tab === 5 && (
        <div className="card" data-testid="settings-payment">
          <h3 style={{ marginBottom: 16 }}>Credit Card</h3>
          <div className="form-row"><label>Type</label><input type="text" value={pay.cc_type || ''} onChange={e => setPay(p => ({ ...p, cc_type: e.target.value }))} style={{ maxWidth: 200 }} /></div>
          <div className="form-row"><label>Number</label><input type="text" value={pay.cc_number || ''} onChange={e => setPay(p => ({ ...p, cc_number: e.target.value }))} style={{ maxWidth: 250 }} /></div>
          <div className="form-row"><label>Exp</label><input type="text" value={pay.cc_exp || ''} onChange={e => setPay(p => ({ ...p, cc_exp: e.target.value }))} style={{ maxWidth: 120 }} /></div>
          <div className="form-row"><label>CVV</label><input type="text" value={pay.cc_cvv || ''} onChange={e => setPay(p => ({ ...p, cc_cvv: e.target.value }))} style={{ maxWidth: 100 }} /></div>
          <h3 style={{ margin: '24px 0 16px' }}>EFT</h3>
          <div className="form-row"><label>Routing</label><input type="text" value={pay.eft_routing || ''} onChange={e => setPay(p => ({ ...p, eft_routing: e.target.value }))} style={{ maxWidth: 200 }} /></div>
          <div className="form-row"><label>Account</label><input type="text" value={pay.eft_account || ''} onChange={e => setPay(p => ({ ...p, eft_account: e.target.value }))} style={{ maxWidth: 200 }} /></div>
        </div>
      )}

      <div className="footer-bar" data-testid="settings-footer">
        <span className="spacer" />
        <button className="btn btn-primary btn-lg" onClick={handleSave} data-testid="settings-save">Save Settings</button>
      </div>
    </div>
  );
}
