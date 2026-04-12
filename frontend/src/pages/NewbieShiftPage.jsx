import React, { useState, useEffect } from 'react';
import api from '../api';
import { useModal } from '../components/ModalProvider';
import TechIssueDialog from '../components/TechIssueDialog';

export default function NewbieShiftPage({ onNavigate }) {
  const modal = useModal();
  const [techOpen, setTechOpen] = useState(false);
  const [isFinal, setIsFinal] = useState(false);
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const [date, setDate] = useState(tomorrow.toISOString().split('T')[0]);
  const [time, setTime] = useState('');
  const [ampm, setAmpm] = useState('AM');
  const [tz, setTz] = useState('EST (Eastern)');

  useEffect(() => {
    api.getCurrentSession().then(({ session }) => {
      if (session) setIsFinal(session.final_attempt || false);
    }).catch(() => {});
  }, []);

  const gatherData = () => {
    const raw = time.trim().replace(/\D/g, '');
    if (raw.length < 3 || raw.length > 4) return null;
    const formatted = raw.length === 3 ? `${raw[0]}:${raw.slice(1)}` : `${raw.slice(0, 2)}:${raw.slice(2)}`;
    const parts = date.split('-');
    const mmddyyyy = `${parts[1]}/${parts[2]}/${parts[0]}`;
    return { newbie_date: mmddyyyy, newbie_time: `${formatted} ${ampm}`, newbie_tz: tz };
  };

  const handleGcal = () => {
    const d = gatherData();
    if (!d) { modal.warning('Notice', 'Enter a valid time (e.g. 1030 or 945).'); return; }
    const dateStr = date.replace(/-/g, '');
    const title = encodeURIComponent('Newbie Shift - Supervisor Transfer');
    const details = encodeURIComponent(`Mock Testing Suite - Newbie Shift\nTime: ${d.newbie_time}\nTimezone: ${d.newbie_tz}`);
    const url = `https://calendar.google.com/calendar/render?action=TEMPLATE&text=${title}&dates=${dateStr}/${dateStr}&details=${details}`;
    window.open(url, '_blank');
  };

  const handleContinue = async () => {
    const d = gatherData();
    if (!d) { await modal.warning('Notice', 'Enter a valid time (e.g. 1030 or 945).'); return; }
    await api.updateSession({ newbie_shift_data: d });
    onNavigate('review');
  };

  return (
    <div data-testid="newbieshift-page">
      <h1 style={{ marginBottom: 24 }}>Schedule Newbie Shift</h1>
      <div className="card" style={{ padding: 48 }}>
        <div style={{ display: 'flex', gap: 32, justifyContent: 'center', flexWrap: 'wrap' }}>
          <div>
            <label className="text-sm font-bold text-muted" style={{ display: 'block', marginBottom: 6 }}>DATE</label>
            <input type="date" value={date} onChange={e => setDate(e.target.value)} style={{ maxWidth: 180 }} data-testid="newbie-date" />
          </div>
          <div>
            <label className="text-sm font-bold text-muted" style={{ display: 'block', marginBottom: 6 }}>START TIME</label>
            <div style={{ display: 'flex', gap: 6 }}>
              <input type="text" value={time} onChange={e => setTime(e.target.value)} placeholder="e.g. 1030" style={{ maxWidth: 110 }} data-testid="newbie-time" />
              <select value={ampm} onChange={e => setAmpm(e.target.value)} style={{ maxWidth: 70 }} data-testid="newbie-ampm"><option>AM</option><option>PM</option></select>
            </div>
          </div>
          <div>
            <label className="text-sm font-bold text-muted" style={{ display: 'block', marginBottom: 6 }}>TIMEZONE</label>
            <select value={tz} onChange={e => setTz(e.target.value)} style={{ maxWidth: 200 }} data-testid="newbie-tz">
              <option>EST (Eastern)</option><option>CST (Central)</option><option>MST (Mountain)</option><option>PST (Pacific)</option>
            </select>
          </div>
        </div>
      </div>
      <div style={{ marginTop: 24 }}>
        <button className="btn btn-primary" onClick={handleGcal} data-testid="newbie-gcal">Add to Google Calendar</button>
      </div>

      <TechIssueDialog open={techOpen} onClose={() => setTechOpen(false)} isFinalAttempt={isFinal} onNavigate={onNavigate} />

      <div className="footer-bar" data-testid="newbie-footer">
        <button className="btn btn-muted btn-sm" onClick={async () => {
          if (await modal.confirm('Confirm', 'Discard session and lose all progress?')) { await api.discardSession(); onNavigate('home'); }
        }} data-testid="newbie-discard">Discard</button>
        <button className="btn btn-danger btn-sm" onClick={async () => { await api.updateSession({ auto_fail_reason: 'Stopped Responding in Chat', final_status: 'Fail' }); onNavigate('review'); }} data-testid="newbie-stopped">Stopped Responding</button>
        <button className="btn btn-muted btn-sm" onClick={() => setTechOpen(true)} data-testid="newbie-tech">Tech Issue</button>
        <span className="spacer" />
        <button className="btn btn-primary" onClick={handleContinue} data-testid="newbie-continue">Continue to Review</button>
      </div>
    </div>
  );
}
