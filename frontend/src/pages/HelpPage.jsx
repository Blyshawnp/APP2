import React, { useState } from 'react';
import api from '../api';

const TABS = [
  { id: 'instructions', label: 'Instructions' },
  { id: 'faq', label: 'FAQ' },
  { id: 'integrations', label: 'Integrations' },
  { id: 'troubleshooting', label: 'Troubleshooting' },
];

export default function HelpPage({ onNavigate }) {
  const [tab, setTab] = useState('instructions');
  const [showTutorial, setShowTutorial] = useState(false);

  return (
    <div data-testid="help-page">
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 24 }}>
        <h1>Help & Documentation</h1>
        <button className="btn btn-primary btn-sm" onClick={() => setShowTutorial(true)} data-testid="help-tutorial">Replay Tutorial</button>
      </div>

      <div className="tabs-header">
        {TABS.map(t => (
          <button key={t.id} className={`tab-btn ${tab === t.id ? 'active' : ''}`} onClick={() => setTab(t.id)} data-testid={`help-tab-${t.id}`}>{t.label}</button>
        ))}
      </div>

      {tab === 'instructions' && (
        <div className="card" style={{ lineHeight: 1.8 }}>
          <h2 style={{ color: 'var(--color-primary)', marginBottom: 24 }}>Complete Session Guide</h2>
          <h3>Before You Begin</h3>
          <p>Make sure the candidate is in the Discord chat and ready. Confirm they have their headset plugged in, browser open, and are logged into Gateway, Simple Script, and Call Corp.</p>
          <h3 style={{ marginTop: 24 }}>Step 1: The Basics</h3>
          <p>Enter the candidate's name and verify their technical setup:</p>
          <ul><li><b>Headset:</b> Must be USB with a noise-cancelling microphone.</li><li><b>VPN:</b> If they have one, they must turn it off.</li><li><b>Browser:</b> Default browser, extensions off, pop-ups allowed.</li><li><b>Final Attempt:</b> Check if this is their last mock session.</li></ul>
          <h3 style={{ marginTop: 24 }}>Step 2: Mock Calls (2-3 calls)</h3>
          <p>The candidate must pass <b>two calls</b> — one as a <b>New Donor</b> and one as an <b>Existing Member</b>.</p>
          <ul><li>Select Call Type, Show, Caller, and Donation from the dropdowns.</li><li>Click <b>PASS</b> or <b>FAIL</b> after each call.</li><li>Select <b>Coaching checkboxes</b> for every call (required).</li><li>If FAIL: select at least one <b>Fail Reason</b>.</li></ul>
          <p><b>Routing:</b> 2 passes = Sup Transfers. 2 fails = session ends. 1+1 = Call 3.</p>
          <h3 style={{ marginTop: 24 }}>Step 3: Supervisor Transfers (1-2)</h3>
          <ul><li>Post "WXYZ Supervisor Test Call Being Queued" in Stars Discord channel.</li><li>Call the WXYZ number: <b>1-828-630-7006</b></li><li>Grade with PASS/FAIL and coaching checkboxes.</li><li>Pass Transfer 1 = done. Fail both = Newbie Shift.</li></ul>
          <h3 style={{ marginTop: 24 }}>Step 4: Newbie Shift (if needed)</h3>
          <p>Pick a date, time, and timezone. Use the Google Calendar button to create an event.</p>
          <h3 style={{ marginTop: 24 }}>Step 5: Review & Submit</h3>
          <ul><li>Pass/Fail/Incomplete banner is calculated automatically.</li><li>Coaching and Fail summaries are generated from your checkboxes.</li><li><b>Fill Form</b> opens Chrome and auto-fills the Cert Form.</li><li><b>Save & Finish</b> saves to history and clears the session.</li></ul>
          <h3 style={{ marginTop: 24 }}>Interruptions (Any Screen)</h3>
          <ul><li><b style={{ color: 'var(--color-danger)' }}>NC/NS</b> — No Call / No Show. Auto-fails.</li><li><b style={{ color: 'var(--color-danger)' }}>Stopped Responding</b> — Candidate went silent. Auto-fails.</li><li><b>Tech Issue</b> — Logs technical problems and guides troubleshooting.</li><li><b>Back</b> — Returns to the previous screen.</li></ul>
        </div>
      )}

      {tab === 'faq' && (
        <div className="card" style={{ lineHeight: 1.8 }}>
          <h2 style={{ color: 'var(--color-primary)', marginBottom: 24 }}>Frequently Asked Questions</h2>
          <FAQ q="What if the candidate stops responding?" a='Click the red "Stopped Responding" button. This instantly ends the session as a fail.' />
          <FAQ q="What if the candidate has technical issues?" a='Click "Tech Issue". The app walks you through troubleshooting: check DTE status, clear browsing data, re-login.' />
          <FAQ q="Can I go back and change something?" a='Yes — click "Back" on any screen. Your data is saved as you go.' />
          <FAQ q="What if I forget to select coaching?" a="The app won't let you continue without selecting at least one coaching checkbox." />
          <FAQ q='How do I do a Supervisor Transfer ONLY session?' a='On the Home screen, click "Supervisor Transfer Only". This skips Mock Calls.' />
          <FAQ q='What does "Final Attempt" mean?' a="If this is the candidate's last allowed attempt, check the box. The messaging changes to tell them they've exceeded allowed attempts." />
          <FAQ q="What if 2 calls fail?" a="The session ends immediately and goes to Review. They should reschedule within 24 hours." />
          <FAQ q="Where is my data stored?" a="In the app's database. Nothing goes online unless Google Sheets is enabled." />
          <FAQ q="How does auto-save work?" a="The app saves your session automatically. If it crashes, your session is recovered on reopen." />
        </div>
      )}

      {tab === 'integrations' && (
        <div className="card" style={{ lineHeight: 1.8 }}>
          <h2 style={{ color: 'var(--color-primary)', marginBottom: 24 }}>Integration Setup Guides</h2>
          <p>All integrations are optional. The app works perfectly without them.</p>
          <h3 style={{ marginTop: 32 }}>Gemini AI — Smart Summaries</h3>
          <p>Rewrites your coaching and fail summaries into clean, professional language.</p>
          <ol><li>Go to <b>aistudio.google.com</b></li><li>Sign in, click <b>Get API Key</b>, then <b>Create API key</b></li><li>Copy the key</li><li>In this app: <b>Settings - Gemini</b> - paste key - check Enable - Save</li></ol>
          <h3 style={{ marginTop: 32 }}>Google Sheets — Auto Backup</h3>
          <p>Logs every session to a Google Spreadsheet automatically.</p>
          <ol><li>Go to <b>console.cloud.google.com</b></li><li>Create a project, enable <b>Google Sheets API</b> + <b>Google Drive API</b></li><li>Go to Credentials, <b>Service Account</b>, create one</li><li>Keys tab, <b>Add Key, JSON</b>, download the file</li><li>Open the JSON, copy the <b>client_email</b></li><li>Share your Google Sheet with that email (Editor access)</li><li>Copy the <b>Spreadsheet ID</b> from the Sheet URL</li><li>In this app: <b>Settings - Google Sheet</b> - paste ID - check Enable - Save</li></ol>
          <h3 style={{ marginTop: 32 }}>Google Calendar</h3>
          <p>The "Add to Google Calendar" button on Newbie Shift creates a calendar event. No setup needed.</p>
          <h3 style={{ marginTop: 32 }}>Form Filler</h3>
          <p>Auto-fills the Cert Form in Chrome. Requires the desktop version of the app.</p>
        </div>
      )}

      {tab === 'troubleshooting' && (
        <div className="card" style={{ lineHeight: 1.8 }}>
          <h2 style={{ color: 'var(--color-primary)', marginBottom: 24 }}>Troubleshooting</h2>
          <Issue title="Form filler doesn't work in web version" solution="The form filler uses Selenium and Chrome, which requires the desktop version. In the web version, use the Copy buttons to copy summaries." />
          <Issue title='Google Sheets says "permission denied"' solution="Open the service_account.json file, find the client_email, and share your Sheet with that email address." />
          <Issue title='Gemini says "API key not valid"' solution="Go to aistudio.google.com, create a new API key, and paste it in Settings - Gemini." />
          <Issue title="Session data lost after crash" solution="The app auto-saves regularly. On reopen, your session should be recovered from the draft." />
        </div>
      )}

      {showTutorial && <TutorialOverlay onClose={() => { setShowTutorial(false); api.saveSettings({ tutorial_completed: true }); }} />}
    </div>
  );
}

function FAQ({ q, a }) {
  return (
    <div style={{ marginBottom: 16, paddingBottom: 16, borderBottom: '1px solid var(--border-subtle)' }}>
      <p style={{ fontWeight: 700, marginBottom: 4 }}>Q: {q}</p>
      <p style={{ color: 'var(--text-secondary)' }}>A: {a}</p>
    </div>
  );
}

function Issue({ title, solution }) {
  return (
    <div style={{ marginBottom: 16, paddingBottom: 16, borderBottom: '1px solid var(--border-subtle)' }}>
      <p style={{ fontWeight: 700, color: 'var(--color-danger)', marginBottom: 4 }}>{title}</p>
      <p style={{ color: 'var(--text-secondary)' }}>{solution}</p>
    </div>
  );
}

const TUTORIAL_STEPS = [
  { title: 'Welcome to Mock Testing Suite!', body: 'This tutorial walks you through each screen. The session flow is always: The Basics > Mock Calls > Sup Transfers > Review. The app handles the routing for you.' },
  { title: 'The Basics', body: 'Your first stop. Verify the candidate\'s headset (USB, noise-cancelling), VPN status, and browser setup. Red buttons at the bottom handle NC/NS, Not Ready, and Stopped Responding auto-fails.' },
  { title: 'Mock Calls (Up to 3)', body: 'Grade up to 3 calls. Each needs a call type, show, caller, and coaching checkboxes. 2 Passes (1 New + 1 Existing) moves to Sup Transfers. 2 Fails ends the session.' },
  { title: 'Supervisor Transfers (Up to 2)', body: 'Tests the candidate\'s ability to transfer. Post "WXYZ Supervisor Test Call Being Queued" in Discord. Call 1-828-630-7006. Pass Transfer 1 = done. Fail both = Newbie Shift.' },
  { title: 'Newbie Shift', body: 'Only appears if the candidate needs a follow-up session. Pick a date, time, and timezone. Click "Add to Google Calendar" to create an event.' },
  { title: 'Review & Submit', body: 'The final screen. Review all results, edit coaching and fail summaries, fill the Cert Form, and save the session to history.' },
  { title: 'Tips & Integrations', body: 'Enable Gemini AI for smart summaries, Google Sheets for auto-backup, and Google Calendar for one-click events. All optional — the app works perfectly without them.' },
];

function TutorialOverlay({ onClose }) {
  const [step, setStep] = useState(0);
  const s = TUTORIAL_STEPS[step];
  return (
    <div className="cmodal-overlay open" data-testid="tutorial-overlay">
      <div className="cmodal" style={{ maxWidth: 580 }}>
        <div className="cmodal-title" style={{ color: 'var(--color-primary)' }}>{s.title}</div>
        <div className="cmodal-body" style={{ textAlign: 'left' }}>{s.body}</div>
        <div style={{ display: 'flex', justifyContent: 'center', gap: 8, marginBottom: 16 }}>
          {TUTORIAL_STEPS.map((_, i) => <span key={i} style={{ width: 8, height: 8, borderRadius: '50%', background: i === step ? 'var(--color-primary)' : 'var(--border-default)', display: 'inline-block' }} />)}
        </div>
        <div className="cmodal-btns">
          <button className="btn btn-muted btn-sm" onClick={onClose}>Skip</button>
          <span style={{ flex: 1 }} />
          {step > 0 && <button className="btn btn-muted btn-sm" onClick={() => setStep(step - 1)}>Back</button>}
          <button className={`btn ${step === TUTORIAL_STEPS.length - 1 ? 'btn-success' : 'btn-primary'}`} onClick={() => { if (step < TUTORIAL_STEPS.length - 1) setStep(step + 1); else onClose(); }}>
            {step === TUTORIAL_STEPS.length - 1 ? 'Get Started' : 'Next'}
          </button>
        </div>
      </div>
    </div>
  );
}
