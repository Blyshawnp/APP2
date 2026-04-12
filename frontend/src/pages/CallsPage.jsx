import React, { useState, useEffect, useCallback } from 'react';
import api from '../api';
import { useModal } from '../components/ModalProvider';
import TechIssueDialog from '../components/TechIssueDialog';

const CALL_COACHING = [
  { id: 'c-show-app', label: 'Show appreciation', children: ['For Current/Existing Donors', 'After donation amount is given'] },
  { id: 'c-phonetics', label: 'Phonetics table provided to candidate' },
  { id: 'c-dontask', label: "Don't Ask, Just Verify Address and Phone Number", helper: 'Existing member already provided address and phone number' },
  { id: 'c-verify', label: 'Verification', children: ['Name', 'Address', 'Phone', 'Email', 'Card/EFT', 'Phonetics for Sound Alike Letters'] },
  { id: 'c-verbatim', label: 'Read script verbatim', helper: 'No adlibbing or skipping sections' },
  { id: 'c-nav', label: 'Use effective script navigation', children: ['Scroll down to avoid missing parts of the script', 'Use the Back and Next buttons and not the Icons'] },
  { id: 'c-other', label: 'Other' },
];

const CALL_FAILS = [
  'Skipped parts of script', 'Volunteered info', 'Wrong donation', 'Background noise on call',
  'Paraphrased script', 'Wrong thank you gift', 'Script navigation issues', 'Other',
];

export default function CallsPage({ onNavigate }) {
  const modal = useModal();
  const [callNum, setCallNum] = useState(1);
  const [result, setResult] = useState(null);
  const [defaults, setDefaults] = useState({});
  const [settings, setSettings] = useState({});
  const [techOpen, setTechOpen] = useState(false);
  const [callSetup, setCallSetup] = useState({ type: '', show: '', caller: '', donation: '' });
  const [coaching, setCoaching] = useState({});
  const [coachNotes, setCoachNotes] = useState('');
  const [fails, setFails] = useState({});
  const [failNotes, setFailNotes] = useState('');
  const [randFlags, setRandFlags] = useState({});
  const [isFinal, setIsFinal] = useState(false);

  const pick = (a) => a[Math.floor(Math.random() * a.length)];
  const rollRandom = useCallback(() => {
    setRandFlags({ phone: pick(['Cell', 'Landline']), text: pick(['Yes', 'No']), enews: pick(['Yes', 'No']), ship: pick(['Yes', 'No']), ccfee: pick(['Yes', 'No']) });
  }, []);

  useEffect(() => {
    (async () => {
      try {
        const [d, s] = await Promise.all([api.getDefaults(), api.getSettings()]);
        setDefaults(d); setSettings(s);
        const types = s.call_types || d.call_types || [];
        const shows = s.shows || d.shows || [];
        if (types.length) setCallSetup(prev => ({ ...prev, type: types[0] }));
        if (shows.length) setCallSetup(prev => ({ ...prev, show: shows[0][0] }));
        const { session } = await api.getCurrentSession();
        if (session) setIsFinal(session.final_attempt || false);
      } catch {}
      rollRandom();
    })();
  }, [rollRandom]);

  const callTypes = settings.call_types || defaults.call_types || [];
  const shows = settings.shows || defaults.shows || [];
  const getCallers = () => {
    const ct = callSetup.type.toLowerCase();
    if (ct.includes('increase')) return settings.donors_increase || defaults.donors_increase || [];
    if (ct.includes('new')) return settings.donors_new || defaults.donors_new || [];
    return settings.donors_existing || defaults.donors_existing || [];
  };

  const callers = getCallers();
  const callerIdx = Math.max(0, callers.findIndex(c => `${c[0]} ${c[1]}` === callSetup.caller));
  const currentCaller = callers[callerIdx] || callers[0] || [];
  const showData = shows.find(s => s[0] === callSetup.show);

  const getDonations = () => {
    if (!showData) return ['Other'];
    const ct = callSetup.type.toLowerCase();
    const isMonthly = !ct.includes('one time');
    const amt = isMonthly ? showData[2] : showData[1];
    return amt ? [amt, 'Other'] : ['Other'];
  };

  const buildScenario = () => {
    if (!currentCaller.length) return 'Select call type, show, and caller.';
    const fname = currentCaller[0];
    const fullName = `${currentCaller[0]} ${currentCaller[1]}`;
    const ct = callSetup.type.toLowerCase();
    const donorType = ct.includes('new') ? 'a new donor' : 'an existing member';
    const isSustaining = ct.includes('sustaining') || ct.includes('monthly') || ct.includes('increase');
    let action = 'make a one-time donation of';
    if (ct.includes('increase')) action = 'increase their sustaining donation to';
    else if (isSustaining) action = 'start a new sustaining donation of';
    let html = `<b>For this call you will portray ${fullName}.</b> ${fname} is ${donorType} wishing to ${action} ${callSetup.donation || getDonations()[0]} to support ${callSetup.show}.<br/><br/>`;
    html += `<b>Phone Type:</b> ${randFlags.phone || 'Cell'}<br/>`;
    if (randFlags.phone === 'Cell') html += `<b>Text Messages:</b> ${randFlags.text || 'No'}<br/>`;
    html += `<b>E-Newsletter:</b> ${randFlags.enews || 'No'}<br/>`;
    html += `<b>Cover $6 Shipping:</b> ${randFlags.ship || 'No'}<br/>`;
    if (isSustaining) html += `<b>Cover $2 CC Fee:</b> ${randFlags.ccfee || 'No'}`;
    return html;
  };

  const resetCall = () => {
    setResult(null);
    setCoaching({});
    setCoachNotes('');
    setFails({});
    setFailNotes('');
    rollRandom();
  };

  const handleContinue = async () => {
    if (!result) { await modal.warning('Notice', 'You must select PASS or FAIL.'); return; }
    if (result === 'Fail') {
      const hasCheck = Object.values(fails).some(v => v);
      if (!hasCheck) { await modal.warning('Notice', 'You must select at least one Fail Reason.'); return; }
      if (fails['Other'] && !failNotes.trim()) { await modal.warning('Notice', 'You selected "Other" — please provide notes.'); return; }
    }

    const callData = {
      call_num: callNum, result, type: callSetup.type, show: callSetup.show,
      caller: callSetup.caller || (currentCaller.length ? `${currentCaller[0]} ${currentCaller[1]}` : ''),
      donation: callSetup.donation || getDonations()[0],
      coaching, coach_notes: coachNotes, fails, fail_notes: failNotes,
    };
    await api.saveCall(callData);

    const { session } = await api.getCurrentSession();
    let passes = [], failCount = 0;
    for (let i = 1; i <= 3; i++) {
      const c = session[`call_${i}`];
      if (c && c.result === 'Pass') passes.push(c.type || '');
      else if (c && c.result === 'Fail') failCount++;
    }

    if (passes.length === 2) {
      const hasNew = passes.some(t => t.toLowerCase().includes('new'));
      const hasExt = passes.some(t => t.toLowerCase().includes('existing'));
      if (!hasNew || !hasExt) {
        const missing = hasNew ? 'Existing Member' : 'New Donor';
        await modal.warning('Call Type Error', `You must pass one New Donor and one Existing Member call.<br><br>Change this call's type to a "${missing}" scenario.`);
        return;
      }
    }

    if (failCount >= 2) {
      await api.updateSession({ final_status: 'Fail' });
      await modal.warning('Notice', 'The candidate has failed 2 calls. Proceeding to Review.');
      onNavigate('review');
      return;
    }

    if (passes.length >= 2) {
      const hasTime = await modal.confirm('Confirm', 'Is there enough time for Supervisor Transfers?');
      if (hasTime) {
        await api.updateSession({ time_for_sup: true });
        onNavigate('suptransfer');
      } else {
        await api.updateSession({ time_for_sup: false });
        onNavigate('newbieshift');
      }
      return;
    }

    setCallNum(prev => prev + 1);
    resetCall();
  };

  return (
    <div data-testid="calls-page">
      <h1 style={{ marginBottom: 24 }}>Call #{callNum}</h1>
      <div className="split-layout">
        <div className="card">
          <h3 style={{ marginBottom: 16 }}>Call Setup</h3>
          <div className="form-row"><label>Call Type</label>
            <select value={callSetup.type} onChange={e => { setCallSetup(p => ({ ...p, type: e.target.value })); rollRandom(); }} data-testid="call-type">
              {callTypes.map(t => <option key={t}>{t}</option>)}
            </select>
          </div>
          <div className="form-row"><label>Show</label>
            <select value={callSetup.show} onChange={e => setCallSetup(p => ({ ...p, show: e.target.value }))} data-testid="call-show">
              {shows.map(s => <option key={s[0]}>{s[0]}</option>)}
            </select>
          </div>
          <div className="form-row"><label>Caller</label>
            <select value={callSetup.caller} onChange={e => { setCallSetup(p => ({ ...p, caller: e.target.value })); rollRandom(); }} data-testid="call-caller">
              {callers.map(c => <option key={`${c[0]}${c[1]}`}>{c[0]} {c[1]}</option>)}
            </select>
          </div>
          <div className="form-row"><label>Donation</label>
            <select value={callSetup.donation} onChange={e => setCallSetup(p => ({ ...p, donation: e.target.value }))} data-testid="call-donation">
              {getDonations().map(d => <option key={d}>{d}</option>)}
            </select>
          </div>
        </div>
        <div className="card card-scenario">
          <h3 style={{ color: 'var(--border-scenario)', marginBottom: 8 }}>SCENARIO</h3>
          <div style={{ lineHeight: 1.7 }} dangerouslySetInnerHTML={{ __html: buildScenario() }} />
        </div>
      </div>

      <div className="card" style={{ marginBottom: 16 }}>
        <h3 style={{ marginBottom: 8 }}>Payment Simulation</h3>
        <div className="payment-grid">
          <div className="payment-card payment-card-cc">
            <div style={{ fontWeight: 700, fontSize: 12, marginBottom: 6 }}>AMERICAN EXPRESS</div>
            <div className="font-mono font-bold" style={{ fontSize: 18, letterSpacing: 2 }}>3782 822463 10005</div>
            <div style={{ fontWeight: 600, fontSize: 13, marginTop: 4 }}>EXP: 07/2027 &nbsp; CVV: 1928</div>
          </div>
          <div className="payment-card payment-card-eft">
            <div style={{ fontWeight: 700, fontSize: 12, marginBottom: 6 }}>EFT / BANK DRAFT</div>
            <div className="font-mono font-bold" style={{ fontSize: 15 }}>RTN: 021000021</div>
            <div className="font-mono font-bold" style={{ fontSize: 15 }}>ACC: 1357902468</div>
          </div>
        </div>
      </div>

      {currentCaller.length > 0 && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginBottom: 8 }}>Caller Demographics</h3>
          <div style={{ textAlign: 'center' }}>
            <b>{currentCaller[0]} {currentCaller[1]}</b><br />
            {currentCaller[2]}{currentCaller[3] ? `, ${currentCaller[3]}` : ''}, {currentCaller[4]}, {currentCaller[5]} {currentCaller[6]}<br />
            Phone: {currentCaller[7]} | Email: {currentCaller[8]}
          </div>
        </div>
      )}

      <div className="card" style={{ marginBottom: 16 }}>
        <h3 style={{ marginBottom: 8 }}>Call Result</h3>
        <div className="result-btns">
          <button className={`result-btn ${result === 'Pass' ? 'selected-pass' : ''}`} onClick={() => setResult('Pass')} data-testid="call-pass">PASS</button>
          <button className={`result-btn ${result === 'Fail' ? 'selected-fail' : ''}`} onClick={() => setResult('Fail')} data-testid="call-fail">FAIL</button>
        </div>
      </div>

      <div className="card" style={{ marginBottom: 16 }}>
        <h3>Coaching Given</h3>
        <p className="text-muted text-sm" style={{ marginBottom: 16 }}>One or more may be selected</p>
        <CoachingGrid items={CALL_COACHING} checked={coaching} onChange={setCoaching} />
        <div style={{ marginTop: 16 }}>
          <label className="text-sm font-bold">Other Coaching Notes</label>
          <textarea rows={2} value={coachNotes} onChange={e => setCoachNotes(e.target.value)} disabled={!coaching['Other']} style={{ marginTop: 4 }} data-testid="call-coach-notes" />
        </div>
      </div>

      {result === 'Fail' && (
        <div className="card card-fail" style={{ marginBottom: 16 }}>
          <h3 style={{ color: 'var(--color-danger)' }}>Fail Reasons</h3>
          <p className="text-muted text-sm" style={{ marginBottom: 16 }}>One or more may be selected</p>
          <FailGrid items={CALL_FAILS} checked={fails} onChange={setFails} />
          <div style={{ marginTop: 16 }}>
            <label className="text-sm font-bold">Other Fail Notes</label>
            <textarea rows={2} value={failNotes} onChange={e => setFailNotes(e.target.value)} disabled={!fails['Other']} style={{ marginTop: 4 }} data-testid="call-fail-notes" />
          </div>
        </div>
      )}

      <TechIssueDialog open={techOpen} onClose={() => setTechOpen(false)} isFinalAttempt={isFinal} onNavigate={onNavigate} />

      <div className="footer-bar" data-testid="calls-footer">
        <button className="btn btn-muted btn-sm" onClick={() => { if (callNum > 1) { setCallNum(n => n - 1); resetCall(); } else onNavigate('basics'); }} data-testid="calls-back">Back</button>
        <button className="btn btn-danger btn-sm" onClick={async () => { await api.updateSession({ auto_fail_reason: 'Stopped Responding in Chat', final_status: 'Fail' }); onNavigate('review'); }} data-testid="calls-stopped">Stopped Responding</button>
        <button className="btn btn-muted btn-sm" onClick={() => setTechOpen(true)} data-testid="calls-tech">Tech Issue</button>
        <span className="spacer" />
        <button className="btn btn-primary" onClick={handleContinue} data-testid="calls-continue">Continue</button>
      </div>
    </div>
  );
}

function CoachingGrid({ items, checked, onChange }) {
  const toggle = (key) => onChange(prev => ({ ...prev, [key]: !prev[key] }));
  const half = Math.ceil(items.length / 2);
  return (
    <div className="coaching-grid">
      <div>
        {items.slice(0, half).map(item => (
          <CoachingItem key={item.id} item={item} checked={checked} onToggle={toggle} />
        ))}
      </div>
      <div>
        {items.slice(half).map(item => (
          <CoachingItem key={item.id} item={item} checked={checked} onToggle={toggle} />
        ))}
      </div>
    </div>
  );
}

function CoachingItem({ item, checked, onToggle }) {
  const parentChecked = !!checked[item.label];
  return (
    <div className="coaching-group">
      <label className="checkbox-label">
        <input type="checkbox" checked={parentChecked} onChange={() => onToggle(item.label)} />
        <span>{item.label}</span>
      </label>
      {item.helper && <div className="helper-text">{item.helper}</div>}
      {item.children && item.children.map(child => (
        <label key={child} className={`checkbox-label sub-item ${!parentChecked ? 'disabled' : ''}`}>
          <input type="checkbox" disabled={!parentChecked} checked={!!checked[`${item.label}_${child}`]} onChange={() => onToggle(`${item.label}_${child}`)} />
          <span>{child}</span>
        </label>
      ))}
    </div>
  );
}

function FailGrid({ items, checked, onChange }) {
  const toggle = (key) => onChange(prev => ({ ...prev, [key]: !prev[key] }));
  const half = Math.ceil(items.length / 2);
  return (
    <div className="coaching-grid">
      <div>{items.slice(0, half).map(item => (
        <label key={item} className="checkbox-label"><input type="checkbox" checked={!!checked[item]} onChange={() => toggle(item)} /><span>{item}</span></label>
      ))}</div>
      <div>{items.slice(half).map(item => (
        <label key={item} className="checkbox-label"><input type="checkbox" checked={!!checked[item]} onChange={() => toggle(item)} /><span>{item}</span></label>
      ))}</div>
    </div>
  );
}
