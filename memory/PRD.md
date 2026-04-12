# Mock Testing Suite v3.0 — PRD

## Original Problem Statement
User uploaded an Electron-based Mock Testing Suite app (FastAPI + vanilla HTML/CSS/JS) that needed to be adapted to work on the Emergent web platform. Requirements:
- All screens working (Setup, Home, Basics, Calls, Sup Transfer, Newbie Shift, Review, History, Settings, Help)
- Tech Issues button with full workflow
- Fix Review screen, Settings screen, Setup wizard

## Architecture
- **Backend**: FastAPI (Python) on port 8001 with MongoDB storage
- **Frontend**: React SPA on port 3000
- **Database**: MongoDB (settings, sessions, history collections)
- **Routing**: React state-based routing (no hash), sidebar navigation

## User Personas
- **Mock Testers**: Staff who conduct mock call testing sessions with candidates
- **Candidates**: People being tested on their call center skills

## Core Requirements (Static)
1. Setup wizard on first run (name, URLs, power-ups info)
2. Session flow: Basics → Calls (up to 3) → Sup Transfers (up to 2) → Review
3. Tech Issues button available on every session screen with branching workflows
4. Settings with tabs: General, Gemini AI, Google Sheets, Calendar, Discord, Payment
5. Session history with stats, search, and detail view
6. Help with Instructions, FAQ, Integrations, Troubleshooting
7. Auto-fail scenarios (NC/NS, Stopped Responding, headset/VPN issues)
8. Summary generation (coaching + fail) from checkbox data

## What's Been Implemented (April 2026)
- Full FastAPI backend with MongoDB (settings, session, history, ticker, summaries, finish-session)
- React frontend with all 10 pages (Setup, Home, Basics, Calls, SupTransfer, NewbieShift, Review, History, Settings, Help)
- Tech Issues dialog with full workflow:
  - Internet speed test (25 Mbps down / 10 Mbps up threshold)
  - Calls won't route → DTE status check → Browser troubleshooting
  - No script pop → Browser troubleshooting
  - Discord/Other issues with resolution tracking
- Setup wizard (3-step: Name → URLs → Power-ups)
- Review screen with coaching/fail summaries, Copy, Regenerate, Fill Form, Save & Finish
- Settings with all 6 tabs, Discord template management, theme toggle
- History with stats, search, table, detail modal
- Sidebar navigation, ticker bar, Discord post popup
- Dark/Light theme support

### Code Quality Fixes Applied (April 2026)
- **XSS**: All `dangerouslySetInnerHTML` uses sanitized via DOMPurify; `innerHTML` replaced with safe DOM methods
- **Hook Dependencies**: All `useEffect`/`useCallback`/`useMemo` hooks have complete dependency arrays; cancellation tokens for async effects
- **Component Complexity**: TechIssueDialog split into 13 focused sub-components; CallsPage business logic extracted into helper functions; App routing extracted into PageRouter component
- **Backend Complexity**: `build_clean_fail` / `build_clean_coaching` / `generate_summaries` refactored into small helpers with early returns and guard clauses
- **Silent Error Handling**: All empty `catch {}` blocks replaced with descriptive comments explaining why silence is intentional
- **Console Statements**: All `console.error` in production paths removed or replaced with silent handling

## Integrations Status
- **Gemini AI**: Placeholder ready (enable in Settings → Gemini, requires API key)
- **Google Sheets**: Placeholder ready (enable in Settings → Google Sheet)
- **Google Calendar**: Working via URL template (no API needed)
- **Form Filler**: Web version shows copy guidance (desktop-only feature)

## Prioritized Backlog
### P0 (Critical)
- None — all core features working

### P1 (Important)
- Gemini AI integration for professional summary rewriting
- Google Sheets backup integration
- Full end-to-end session flow testing with real data

### P2 (Nice to Have)
- Tutorial overlay on first launch after setup
- Session draft auto-recovery
- Export session history as CSV
- Customizable caller data in settings

## Next Tasks
1. User testing of full session flow
2. Gemini AI integration (if user provides API key)
3. Google Sheets integration (if user provides service account)
4. Polish UI animations and transitions
