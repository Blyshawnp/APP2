import React, { useState, useEffect, useCallback, useMemo } from 'react';
import DOMPurify from 'dompurify';
import api from '../api';
import { useModal } from '../components/ModalProvider';
import TechIssueDialog from '../components/TechIssueDialog';

const SUP_COACHING = [
  { label: 'Minimize dead air', helper: 'Maintain engagement throughout hold and transfer' },
  { label: 'Queue Not Changed', helper: 'Did not change queue to ACD Direct Supervisor' },
  { label: 'Caller Placed On Hold' },
  { label: 'Verification', children: ['Name', 'Address', 'Phone', 'Email', 'Card/EFT', 'Phonetics for Sound Alike Letters'] },
  { label: 'Discord permission', helper: 'Ask explicit permission to transfer via Discord' },
  { label: 'Did not notify caller of transfer', helper: 'Notify caller before transferring' },
  { label: 'Screenshots/Discord Chat', helper: 'Coached with standard instructions and screenshots' },
  { label: 'Other' },
];

const SUP_FAILS = [
  'Did not ask permission to transfer', 'Did not minimize dead air', 'Caller Placed On Hold',
  'Transferred to wrong queue', 'Did not inform caller of transfer', 'Other',
];

const SUP_REASONS = [
  'Hung up on', 'Charged for a cancelled sustaining', 'Double Charged',
  'Damaged Gift', "Didn't Receive Gift", 'Cancel Sustaining', 'Use Own/Other',
];

export default function SupTransferPage({ onNavigate }) {
  const modal = useModal();
  const [transferNum, setTransferNum] = useState(1);
  const [result, setResult] = useState(null);
  const [defaults, setDefaults] = useState({});
  const [settings, setSettings] = useState({});
  const [techOpen, setTechOpen] = useState(false);
  const [setup, setSetup] = useState({ caller: '', show: '', reason: SUP_REASONS[0] });
  const [coaching, setCoaching] = useState({});
  const [coachNotes, setCoachNotes] = useState('');
  const [fails, setFails] = useState({});
  const [failNotes, setFailNotes] = useState('');
  const [isFinal, setIsFinal] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [d, s] = await Promise.all([api.getDefaults(), api.getSettings()]);
        if (cancelled) return;
        setDefaults(d); setSettings(s);
        const { session } = await api.getCurrentSession();
        if (!cancelled && session) setIsFinal(session.final_attempt || false);
      } catch (err) {
        // Failed to load transfer setup data — page renders with empty dropdowns
      }
    })();
    return () => { cancelled = true; };
  }, []);

  const shows = settings.shows || defaults.shows || [];
  const callers = useMemo(() => settings.donors_existing || defaults.donors_existing || [], [settings.donors_existing, defaults.donors_existing]);
  const callerIdx = Math.max(0, callers.findIndex(c => `${c[0]} ${c[1]}` === setup.caller));
  const currentCaller = useMemo(() => callers[callerIdx] || callers[0] || [], [callers, callerIdx]);

  const scenarioHtml = useMemo(() => {
    if (!currentCaller.length) return 'Select caller, show, and reason.';
    const fullName = `${currentCaller[0]} ${currentCaller[1]}`;
    const fname = currentCaller[0];
    const phone = ['Cell', 'Landline'][Math.floor(Math.random() * 2)];
    const raw = `<b>For this call you will portray ${fullName}.</b> ${fname} would like to speak with a supervisor. The caller was ${(setup.reason || '').toLowerCase()} during a previous call.<br/><b>Phone Type:</b> ${phone}`;
    return DOMPurify.sanitize(raw);
  }, [currentCaller, setup.reason]);

  const resetTransfer = () => {
    setResult(null);
    setCoaching({});
    setCoachNotes('');
    setFails({});
    setFailNotes('');
  };

  const handleContinue = async () => {
    if (!result) { await modal.warning('Notice', 'Select PASS or FAIL.'); return; }
    if (result === 'Fail' && !Object.values(fails).some(v => v)) { await modal.warning('Notice', 'Select at least one Fail Reason.'); return; }

    const data = {
      transfer_num: transferNum, result,
      caller: setup.caller || (currentCaller.length ? `${currentCaller[0]} ${currentCaller[1]}` : ''),
      show: setup.show || (shows.length ? shows[0][0] : ''), reason: setup.reason,
      coaching, coach_notes: coachNotes, fails, fail_notes: failNotes,
    };
    await api.saveSupTransfer(data);

    if (transferNum === 1) {
      if (result === 'Pass') { onNavigate('review'); }
      else { setTransferNum(2); resetTransfer(); }
    } else {
      if (result === 'Fail') {
        const { session } = await api.getCurrentSession();
        if (session && session.final_attempt) { await api.updateSession({ final_status: 'Fail' }); onNavigate('review'); }
        else { onNavigate('newbieshift'); }
      } else { onNavigate('review'); }
    }
  };

  const toggle = (key, setter) => setter(prev => ({ ...prev, [key]: !prev[key] }));

  return (
    <div data-testid="suptransfer-page">
      <h1 style={{ marginBottom: 8 }}>Supervisor Transfer #{transferNum}</h1>
      <div className="card" style={{ textAlign: 'center', marginBottom: 16, padding: 16, background: 'var(--color-primary)', border: 'none' }}>
        <div style={{ color: 'white', fontWeight: 700, fontSize: '1.125rem' }}>Call Corp WXYZ Test Transfer #: 1-828-630-7006</div>
      </div>
      <div className="card" style={{ marginBottom: 16, display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '16px 24px' }}>
        <span><b>Discord Post for Stars:</b> WXYZ Supervisor Test Call Being Queued</span>
        <button className="btn btn-primary btn-sm" onClick={() => {
          navigator.clipboard.writeText('WXYZ Supervisor Test Call Being Queued');
          setCopied(true); setTimeout(() => setCopied(false), 1500);
        }} data-testid="sup-copy-discord">{copied ? 'Copied!' : 'Copy'}</button>
      </div>

      <div className="split-layout">
        <div className="card">
          <h3 style={{ marginBottom: 16 }}>Call Setup</h3>
          <div className="form-row"><label>Caller</label>
            <select value={setup.caller} onChange={e => setSetup(p => ({ ...p, caller: e.target.value }))} data-testid="sup-caller">
              {callers.map(c => <option key={`${c[0]}${c[1]}`}>{c[0]} {c[1]}</option>)}
            </select>
          </div>
          <div className="form-row"><label>Show</label>
            <select value={setup.show} onChange={e => setSetup(p => ({ ...p, show: e.target.value }))} data-testid="sup-show">
              {shows.map(s => <option key={s[0]}>{s[0]}</option>)}
            </select>
          </div>
          <div className="form-row"><label>Reason</label>
            <select value={setup.reason} onChange={e => setSetup(p => ({ ...p, reason: e.target.value }))} data-testid="sup-reason">
              {SUP_REASONS.map(r => <option key={r}>{r}</option>)}
            </select>
          </div>
        </div>
        <div className="card card-scenario">
          <h3 style={{ color: 'var(--border-scenario)', marginBottom: 8 }}>SCENARIO</h3>
          <div style={{ lineHeight: 1.7 }} dangerouslySetInnerHTML={{ __html: scenarioHtml }} />
        </div>
      </div>

      {currentCaller.length > 0 && (
        <div className="card" style={{ margin: '16px 0' }}>
          <h3 style={{ marginBottom: 8 }}>Caller Demographics</h3>
          <div style={{ textAlign: 'center' }}>
            <b>{currentCaller[0]} {currentCaller[1]}</b><br />
            {currentCaller[2]}{currentCaller[3] ? `, ${currentCaller[3]}` : ''}, {currentCaller[4]}, {currentCaller[5]} {currentCaller[6]}<br />
            Phone: {currentCaller[7]} | Email: {currentCaller[8]}
          </div>
        </div>
      )}

      <div className="card" style={{ marginBottom: 16 }}>
        <h3>Transfer Result</h3>
        <div className="result-btns">
          <button className={`result-btn ${result === 'Pass' ? 'selected-pass' : ''}`} onClick={() => setResult('Pass')} data-testid="sup-pass">PASS</button>
          <button className={`result-btn ${result === 'Fail' ? 'selected-fail' : ''}`} onClick={() => setResult('Fail')} data-testid="sup-fail">FAIL</button>
        </div>
      </div>

      <div className="card" style={{ marginBottom: 16 }}>
        <h3>Coaching Given</h3>
        <p className="text-muted text-sm" style={{ marginBottom: 16 }}>One or more may be selected</p>
        <div className="coaching-grid">
          <div>{SUP_COACHING.slice(0, 4).map(item => <CoachItem key={item.label} item={item} checked={coaching} onToggle={k => toggle(k, setCoaching)} />)}</div>
          <div>{SUP_COACHING.slice(4).map(item => <CoachItem key={item.label} item={item} checked={coaching} onToggle={k => toggle(k, setCoaching)} />)}</div>
        </div>
        <div style={{ marginTop: 16 }}><label className="text-sm font-bold">Other Notes</label><textarea rows={2} value={coachNotes} onChange={e => setCoachNotes(e.target.value)} disabled={!coaching['Other']} style={{ marginTop: 4 }} /></div>
      </div>

      {result === 'Fail' && (
        <div className="card card-fail" style={{ marginBottom: 16 }}>
          <h3 style={{ color: 'var(--color-danger)' }}>Fail Reasons</h3>
          <div className="coaching-grid">
            <div>{SUP_FAILS.slice(0, 3).map(item => (
              <label key={item} className="checkbox-label"><input type="checkbox" checked={!!fails[item]} onChange={() => toggle(item, setFails)} /><span>{item}</span></label>
            ))}</div>
            <div>{SUP_FAILS.slice(3).map(item => (
              <label key={item} className="checkbox-label"><input type="checkbox" checked={!!fails[item]} onChange={() => toggle(item, setFails)} /><span>{item}</span></label>
            ))}</div>
          </div>
          <div style={{ marginTop: 16 }}><label className="text-sm font-bold">Other Fail Notes</label><textarea rows={2} value={failNotes} onChange={e => setFailNotes(e.target.value)} disabled={!fails['Other']} style={{ marginTop: 4 }} /></div>
        </div>
      )}

      <TechIssueDialog open={techOpen} onClose={() => setTechOpen(false)} isFinalAttempt={isFinal} onNavigate={onNavigate} />

      <div className="footer-bar" data-testid="sup-footer">
        <button className="btn btn-muted btn-sm" onClick={() => { if (transferNum > 1) { setTransferNum(1); resetTransfer(); } else onNavigate('calls'); }} data-testid="sup-back">Back</button>
        <button className="btn btn-danger btn-sm" onClick={async () => { await api.updateSession({ auto_fail_reason: 'Stopped Responding in Chat', final_status: 'Fail' }); onNavigate('review'); }} data-testid="sup-stopped">Stopped Responding</button>
        <button className="btn btn-muted btn-sm" onClick={() => setTechOpen(true)} data-testid="sup-tech">Tech Issue</button>
        <span className="spacer" />
        <button className="btn btn-primary" onClick={handleContinue} data-testid="sup-continue">Continue</button>
      </div>
    </div>
  );
}

function CoachItem({ item, checked, onToggle }) {
  const parentChecked = !!checked[item.label];
  return (
    <div className="coaching-group">
      <label className="checkbox-label"><input type="checkbox" checked={parentChecked} onChange={() => onToggle(item.label)} /><span>{item.label}</span></label>
      {item.helper && <div className="helper-text">{item.helper}</div>}
      {item.children && item.children.map(child => (
        <label key={child} className={`checkbox-label sub-item ${!parentChecked ? 'disabled' : ''}`}>
          <input type="checkbox" disabled={!parentChecked} checked={!!checked[`${item.label}_${child}`]} onChange={() => onToggle(`${item.label}_${child}`)} /><span>{child}</span>
        </label>
      ))}
    </div>
  );
}
