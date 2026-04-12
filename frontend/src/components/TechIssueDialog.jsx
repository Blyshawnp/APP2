import React, { useState } from 'react';
import { useModal } from './ModalProvider';
import api from '../api';

/*
  TechIssueDialog — Full workflow for technical issues.
  Triggered from any screen's "Tech Issue" button.
  Handles: Internet speed, Calls won't route, No script pop, Discord, Other.
*/

const TECH_ISSUES = [
  { id: 'internet', label: 'Internet speed issues' },
  { id: 'calls', label: 'Calls would not route' },
  { id: 'script', label: 'No script pop' },
  { id: 'discord', label: 'Discord issues' },
  { id: 'other', label: 'Other' },
];

export default function TechIssueDialog({ open, onClose, isFinalAttempt, onNavigate }) {
  const modal = useModal();
  const [step, setStep] = useState('select'); // select, speed-ask, speed-input, dte-ask, browser-ask, browser-steps, browser-result, other-notes, resolved-ask, complete-ask
  const [selected, setSelected] = useState({});
  const [otherNotes, setOtherNotes] = useState('');
  const [speedDown, setSpeedDown] = useState('');
  const [speedUp, setSpeedUp] = useState('');
  const [currentIssue, setCurrentIssue] = useState(null);

  if (!open) return null;

  const reset = () => {
    setStep('select');
    setSelected({});
    setOtherNotes('');
    setSpeedDown('');
    setSpeedUp('');
    setCurrentIssue(null);
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  const handleCheckboxChange = (id) => {
    setSelected(prev => ({ ...prev, [id]: !prev[id] }));
  };

  const processIssues = async () => {
    const checkedIds = Object.keys(selected).filter(k => selected[k]);
    if (checkedIds.length === 0) return;

    for (const id of checkedIds) {
      setCurrentIssue(id);
      if (id === 'internet') {
        setStep('speed-ask');
        return;
      } else if (id === 'calls') {
        setStep('dte-ask');
        return;
      } else if (id === 'script') {
        setStep('browser-ask');
        return;
      } else if (id === 'discord') {
        await logIssue('Discord issues', true);
      } else if (id === 'other') {
        setStep('other-notes');
        return;
      }
    }
    handleClose();
  };

  const logIssue = async (issue, resolved) => {
    try {
      const { session } = await api.getCurrentSession();
      const log = session?.tech_issues_log || [];
      log.push({ issue, resolved, timestamp: new Date().toISOString() });
      await api.updateSession({ tech_issues_log: log, tech_issue: issue });
    } catch (e) {
      console.error('Failed to log tech issue:', e);
    }
  };

  const goToReview = () => {
    handleClose();
    onNavigate('review');
  };

  const goToNewbie = () => {
    handleClose();
    onNavigate('newbieshift');
  };

  const continueToNextIssue = () => {
    const checkedIds = Object.keys(selected).filter(k => selected[k]);
    const currentIdx = checkedIds.indexOf(currentIssue);
    if (currentIdx < checkedIds.length - 1) {
      const nextId = checkedIds[currentIdx + 1];
      setCurrentIssue(nextId);
      if (nextId === 'internet') { setStep('speed-ask'); return; }
      if (nextId === 'calls') { setStep('dte-ask'); return; }
      if (nextId === 'script') { setStep('browser-ask'); return; }
      if (nextId === 'discord') { logIssue('Discord issues', true); }
      if (nextId === 'other') { setStep('other-notes'); return; }
    }
    handleClose();
  };

  // ═══════════ RENDER STEPS ═══════════
  const renderContent = () => {
    switch (step) {
      case 'select':
        return (
          <div>
            <h3 className="ti-title">Technical Issues Log</h3>
            <p className="ti-subtitle">Select all issues that occurred during the session:</p>
            <div className="ti-checklist">
              {TECH_ISSUES.map(issue => (
                <label key={issue.id} className="ti-check-item" data-testid={`tech-issue-${issue.id}`}>
                  <input type="checkbox" checked={!!selected[issue.id]} onChange={() => handleCheckboxChange(issue.id)} />
                  <span>{issue.label}</span>
                </label>
              ))}
            </div>
            <div className="ti-actions">
              <button className="btn btn-muted" onClick={handleClose} data-testid="tech-issue-cancel">Cancel</button>
              <button className="btn btn-primary" onClick={processIssues} disabled={!Object.values(selected).some(v => v)} data-testid="tech-issue-continue">Continue</button>
            </div>
          </div>
        );

      case 'speed-ask':
        return (
          <div>
            <h3 className="ti-title">Internet Speed Issues</h3>
            <p className="ti-body">Did you have the candidate do a speed test?</p>
            <div className="ti-actions">
              <button className="btn btn-muted" onClick={() => { setStep('speed-input'); }} data-testid="speed-test-no">No, have them do one</button>
              <button className="btn btn-primary" onClick={() => setStep('speed-input')} data-testid="speed-test-yes">Yes</button>
            </div>
          </div>
        );

      case 'speed-input':
        return (
          <div>
            <h3 className="ti-title">Speed Test Results</h3>
            <p className="ti-subtitle">Enter the speed test results:</p>
            <div className="ti-form">
              <div className="ti-field">
                <label>Download Speed (Mbps)</label>
                <input type="number" value={speedDown} onChange={e => setSpeedDown(e.target.value)} placeholder="e.g. 50" data-testid="speed-download" />
              </div>
              <div className="ti-field">
                <label>Upload Speed (Mbps)</label>
                <input type="number" value={speedUp} onChange={e => setSpeedUp(e.target.value)} placeholder="e.g. 15" data-testid="speed-upload" />
              </div>
            </div>
            <div className="ti-actions">
              <button className="btn btn-muted" onClick={() => setStep('speed-ask')}>Back</button>
              <button className="btn btn-primary" onClick={() => {
                const dl = parseFloat(speedDown);
                const ul = parseFloat(speedUp);
                if (isNaN(dl) || isNaN(ul)) return;
                if (dl < 25 || ul < 10) {
                  setStep('speed-fail');
                } else {
                  logIssue('Internet speed issues - speeds OK', true);
                  continueToNextIssue();
                }
              }} data-testid="speed-submit">Check Results</button>
            </div>
          </div>
        );

      case 'speed-fail':
        return (
          <div>
            <h3 className="ti-title ti-warn">Speed Test Failed</h3>
            <div className="ti-alert ti-alert-danger">
              <p><strong>Download:</strong> {speedDown} Mbps {parseFloat(speedDown) < 25 ? '(BELOW 25 Mbps minimum)' : '(OK)'}</p>
              <p><strong>Upload:</strong> {speedUp} Mbps {parseFloat(speedUp) < 10 ? '(BELOW 10 Mbps minimum)' : '(OK)'}</p>
            </div>
            {isFinalAttempt ? (
              <div className="ti-alert ti-alert-warning">
                <p><strong>FINAL ATTEMPT:</strong> This counts as a fail. Have the candidate email certification for exceptions.</p>
              </div>
            ) : (
              <div className="ti-alert ti-alert-info">
                <p>Internet speeds are too low. Have the candidate reschedule within 24 hours.</p>
              </div>
            )}
            <div className="ti-actions">
              <button className="btn btn-danger" onClick={async () => {
                await logIssue('Internet speed issues - failed speed test', false);
                await api.updateSession({ auto_fail_reason: 'Internet speed too low', final_status: 'Fail' });
                goToReview();
              }} data-testid="speed-fail-review">Go to Review</button>
            </div>
          </div>
        );

      case 'dte-ask':
        return (
          <div>
            <h3 className="ti-title">Calls Would Not Route</h3>
            <p className="ti-body">Is the candidate's DTE status set to <strong>"Ready"</strong> in the Call Corp Dashboard?</p>
            <div className="ti-actions">
              <button className="btn btn-danger" onClick={() => setStep('dte-fix')} data-testid="dte-no">No</button>
              <button className="btn btn-primary" onClick={() => setStep('browser-ask')} data-testid="dte-yes">Yes</button>
            </div>
          </div>
        );

      case 'dte-fix':
        return (
          <div>
            <h3 className="ti-title">Fix DTE Status</h3>
            <p className="ti-body">Have them change their DTE status to <strong>"Ready"</strong> in the Call Corp Dashboard.</p>
            <p className="ti-subtitle" style={{ marginTop: 12 }}>Did that resolve the issue?</p>
            <div className="ti-actions">
              <button className="btn btn-danger" onClick={() => setStep('browser-ask')} data-testid="dte-fix-no">No</button>
              <button className="btn btn-success" onClick={() => { logIssue('Calls would not route - fixed DTE status', true); continueToNextIssue(); }} data-testid="dte-fix-yes">Yes, Resolved</button>
            </div>
          </div>
        );

      case 'browser-ask':
        return (
          <div>
            <h3 className="ti-title">Browser Troubleshooting</h3>
            <p className="ti-body">Have you tried having the candidate do the following?</p>
            <ol className="ti-steps">
              <li>Log out of all systems</li>
              <li>Clear browsing data (cache and cookies)</li>
              <li>Close the browser completely</li>
              <li>Sign back in via ACD Direct</li>
            </ol>
            <div className="ti-actions">
              <button className="btn btn-muted" onClick={() => setStep('browser-steps')} data-testid="browser-no">No, show steps</button>
              <button className="btn btn-primary" onClick={() => setStep('browser-result')} data-testid="browser-yes">Yes, already tried</button>
            </div>
          </div>
        );

      case 'browser-steps':
        return (
          <div>
            <h3 className="ti-title">Follow These Steps</h3>
            <div className="ti-alert ti-alert-info">
              <p>Have the candidate complete these steps in order:</p>
              <ol className="ti-steps">
                <li><strong>Log out</strong> of Call Corp, Simple Script, and Gateway</li>
                <li><strong>Clear browsing data</strong> — Go to browser settings, clear cache and cookies</li>
                <li><strong>Close the browser</strong> completely (all windows)</li>
                <li><strong>Reopen browser</strong> and sign back in via ACD Direct</li>
              </ol>
            </div>
            <p className="ti-subtitle" style={{ marginTop: 12 }}>Have them try these steps now, then click below.</p>
            <div className="ti-actions">
              <button className="btn btn-primary" onClick={() => setStep('browser-result')} data-testid="browser-steps-done">Done, Check Result</button>
            </div>
          </div>
        );

      case 'browser-result':
        return (
          <div>
            <h3 className="ti-title">Did that resolve the issue?</h3>
            <div className="ti-actions">
              <button className="btn btn-danger" onClick={async () => {
                const issueType = currentIssue === 'calls' ? 'Calls would not route' : 'No script pop';
                await logIssue(`${issueType} - browser troubleshooting failed`, false);
                if (isFinalAttempt) {
                  setStep('browser-fail-final');
                } else {
                  setStep('browser-fail-reschedule');
                }
              }} data-testid="browser-result-no">No</button>
              <button className="btn btn-success" onClick={() => {
                const issueType = currentIssue === 'calls' ? 'Calls would not route' : 'No script pop';
                logIssue(`${issueType} - resolved after browser troubleshooting`, true);
                continueToNextIssue();
              }} data-testid="browser-result-yes">Yes, Resolved</button>
            </div>
          </div>
        );

      case 'browser-fail-final':
        return (
          <div>
            <h3 className="ti-title ti-warn">Final Attempt - Issue Unresolved</h3>
            <div className="ti-alert ti-alert-danger">
              <p><strong>FINAL ATTEMPT:</strong> Ask admins in chat before scheduling a Newbie Shift. Have the candidate email certification for exceptions.</p>
            </div>
            <div className="ti-actions">
              <button className="btn btn-warning" onClick={() => {
                goToNewbie();
              }} data-testid="browser-fail-newbie">Go to Newbie Shift</button>
            </div>
          </div>
        );

      case 'browser-fail-reschedule':
        return (
          <div>
            <h3 className="ti-title ti-warn">Issue Not Resolved</h3>
            <div className="ti-alert ti-alert-warning">
              <p>The issue could not be resolved. Route to Newbie Shift screen to reschedule.</p>
            </div>
            <div className="ti-actions">
              <button className="btn btn-warning" onClick={() => {
                goToNewbie();
              }} data-testid="browser-fail-reschedule-btn">Go to Newbie Shift</button>
            </div>
          </div>
        );

      case 'other-notes':
        return (
          <div>
            <h3 className="ti-title">Other Technical Issue</h3>
            <p className="ti-subtitle">Describe the issue:</p>
            <textarea className="ti-textarea" value={otherNotes} onChange={e => setOtherNotes(e.target.value)} placeholder="Describe the technical issue..." rows={4} data-testid="other-notes-input" />
            <p className="ti-subtitle" style={{ marginTop: 12 }}>Was the issue resolved?</p>
            <div className="ti-actions">
              <button className="btn btn-danger" onClick={async () => {
                await logIssue(`Other: ${otherNotes}`, false);
                setStep('complete-ask');
              }} data-testid="other-not-resolved">No</button>
              <button className="btn btn-success" onClick={async () => {
                await logIssue(`Other: ${otherNotes}`, true);
                continueToNextIssue();
              }} data-testid="other-resolved">Yes, Resolved</button>
            </div>
          </div>
        );

      case 'complete-ask':
        return (
          <div>
            <h3 className="ti-title">Session Completion</h3>
            <p className="ti-body">Were you able to complete the session despite the issues?</p>
            <div className="ti-actions">
              <button className="btn btn-danger" onClick={async () => {
                await api.updateSession({ final_status: 'Fail' });
                goToReview();
              }} data-testid="complete-no">No - End Session</button>
              <button className="btn btn-success" onClick={() => {
                handleClose();
              }} data-testid="complete-yes">Yes - Continue Session</button>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="cmodal-overlay open" data-testid="tech-issue-dialog">
      <div className="cmodal" style={{ maxWidth: 520, width: '90vw', textAlign: 'left' }}>
        {renderContent()}
      </div>
    </div>
  );
}
