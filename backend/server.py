"""
Mock Testing Suite v3.0 — FastAPI Backend
All routes in a single file for simplicity. Uses MongoDB for persistence.
"""
import os
import json
import logging
from datetime import datetime, timezone
from pathlib import Path
from typing import Optional
from contextlib import asynccontextmanager

from fastapi import FastAPI, APIRouter
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv
from motor.motor_asyncio import AsyncIOMotorClient
from pydantic import BaseModel

ROOT_DIR = Path(__file__).parent
load_dotenv(ROOT_DIR / '.env')

mongo_url = os.environ['MONGO_URL']
client = AsyncIOMotorClient(mongo_url)
db = client[os.environ['DB_NAME']]

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# ══════════════════════════════════════════════════════════════════
# CONSTANTS / DEFAULTS
# ══════════════════════════════════════════════════════════════════
APP_VERSION = "3.0"

CALL_TYPES = [
    "New Donor - One Time Donation",
    "Existing Member - Monthly Donation",
    "Increase Sustaining",
]

SUP_REASONS = [
    "Hung up on", "Charged for a cancelled sustaining", "Double Charged",
    "Damaged Gift", "Didn't Receive Gift", "Cancel Sustaining", "Use Own/Other",
]

SHOWS = [
    ["PBS NewsHour", "$60", "$10/mo"],
    ["Frontline", "$75", "$15/mo"],
    ["NOVA", "$120", "$20/mo"],
    ["Masterpiece", "$90", "$12/mo"],
    ["Sesame Street", "$50", "$8/mo"],
    ["Nature", "$80", "$10/mo"],
]

NEW_DONORS = [
    ["John", "Smith", "123 Main St", "Apt 4B", "New York", "NY", "10001", "555-123-4567", "john.smith@email.com"],
    ["Sarah", "Johnson", "456 Oak Ave", "", "Los Angeles", "CA", "90210", "555-234-5678", "sarah.j@email.com"],
    ["Michael", "Williams", "789 Pine Rd", "Suite 100", "Chicago", "IL", "60601", "555-345-6789", "m.williams@email.com"],
    ["Emily", "Brown", "321 Elm St", "", "Houston", "TX", "77001", "555-456-7890", "emily.b@email.com"],
    ["David", "Jones", "654 Maple Dr", "Unit 7", "Phoenix", "AZ", "85001", "555-567-8901", "d.jones@email.com"],
]

EXISTING_MEMBERS = [
    ["Robert", "Davis", "111 Cedar Ln", "", "Philadelphia", "PA", "19101", "555-678-9012", "r.davis@email.com"],
    ["Jennifer", "Miller", "222 Birch Way", "Apt 12", "San Antonio", "TX", "78201", "555-789-0123", "j.miller@email.com"],
    ["James", "Wilson", "333 Walnut St", "", "San Diego", "CA", "92101", "555-890-1234", "j.wilson@email.com"],
    ["Linda", "Moore", "444 Spruce Ave", "Suite 5", "Dallas", "TX", "75201", "555-901-2345", "l.moore@email.com"],
    ["William", "Taylor", "555 Ash Blvd", "", "San Jose", "CA", "95101", "555-012-3456", "w.taylor@email.com"],
]

INCREASE_SUSTAINING = [
    ["Patricia", "Anderson", "666 Poplar Ct", "", "Austin", "TX", "73301", "555-111-2222", "p.anderson@email.com"],
    ["Richard", "Thomas", "777 Willow Rd", "Apt 3A", "Jacksonville", "FL", "32099", "555-222-3333", "r.thomas@email.com"],
]

DISCORD_TEMPLATES = [
    ["Session Starting", "Mock session starting with [CANDIDATE NAME]. Please hold all calls."],
    ["Session Complete", "Mock session with [CANDIDATE NAME] is complete. Calls can resume."],
    ["Sup Transfer Queued", "WXYZ Supervisor Test Call Being Queued"],
]

DEFAULT_PAYMENT = {
    "cc_type": "American Express",
    "cc_number": "3782 822463 10005",
    "cc_exp": "07/2027",
    "cc_cvv": "1928",
    "eft_routing": "021000021",
    "eft_account": "1357902468",
}

TECH_ISSUES = [
    "Internet speed issues",
    "Calls would not route",
    "No script pop",
    "Discord issues",
    "Other",
]

AUTO_FAIL_REASONS = [
    "NC/NS",
    "Stopped responding in chat",
    "Not ready for session",
    "Unable to turn off VPN",
    "Wrong headset (not USB)",
    "Wrong headset (not noise cancelling)",
]

TICKER_MESSAGES = [
    "Welcome to Mock Testing Suite v3.0",
    "Reminder: Log out of Call Corp and Simple Script after each session",
    "Tip: Use the Discord Post button to quickly copy common messages",
    "Need help? Check the Help tab for step-by-step setup guides",
]

DEFAULT_SETTINGS = {
    "setup_complete": False,
    "tutorial_completed": False,
    "tester_name": "",
    "display_name": "",
    "form_url": "",
    "cert_sheet_url": "",
    "theme": "dark",
    "enable_gemini": False,
    "gemini_key": "",
    "enable_sheets": False,
    "sheet_id": "",
    "worksheet": "Sheet1",
    "service_account_path": "service_account.json",
    "enable_calendar": False,
    "discord_templates": DISCORD_TEMPLATES,
    "payment": DEFAULT_PAYMENT,
}


def empty_session():
    return {
        "candidate_name": "",
        "tester_name": "",
        "pronoun": "",
        "final_attempt": False,
        "supervisor_only": False,
        "status": "In Progress",
        "auto_fail_reason": None,
        "tech_issue": "N/A",
        "headset_usb": None,
        "headset_brand": "",
        "noise_cancel": None,
        "vpn_on": None,
        "vpn_off": None,
        "chrome_default": None,
        "extensions_disabled": None,
        "popups_allowed": None,
        "call_1": None,
        "call_2": None,
        "call_3": None,
        "sup_transfer_1": None,
        "sup_transfer_2": None,
        "time_for_sup": None,
        "newbie_shift_data": None,
        "final_status": None,
        "last_saved": None,
        "tech_issues_log": [],
    }


# ══════════════════════════════════════════════════════════════════
# GEMINI SERVICE (summary generation)
# ══════════════════════════════════════════════════════════════════
def _get_coaching_items(data):
    if not data:
        return []
    items = []
    coaching = data.get("coaching", {})
    for key, checked in coaching.items():
        if checked and "_" not in key and key != "Other":
            items.append(key.lower())
        elif checked and "_" in key:
            items.append(key.split("_", 1)[1].lower())
    notes = data.get("coach_notes", "")
    if notes:
        items.append(notes)
    return items


def _get_fail_items(data):
    if not data or data.get("result") != "Fail":
        return []
    items = []
    fails = data.get("fails", {})
    for key, checked in fails.items():
        if checked and key != "Other":
            items.append(key.lower())
    notes = data.get("fail_notes", "")
    if notes:
        items.append(notes)
    return items


def build_clean_coaching(session):
    name = session.get("candidate_name", "Candidate")
    auto_fail = session.get("auto_fail_reason")
    sup_only = session.get("supervisor_only", False)
    lines = []
    if auto_fail:
        lines.append(f"{name} — Auto-fail: {auto_fail}.")
        return "\n".join(lines)
    if not sup_only:
        for i in range(1, 4):
            call = session.get(f"call_{i}")
            if call and call.get("result"):
                result = call["result"]
                ctype = call.get("type", "Unknown type")
                coaching = _get_coaching_items(call)
                coaching_str = ", ".join(coaching) if coaching else "none noted"
                lines.append(f"Call {i} ({ctype}): {result}. Coaching: {coaching_str}.")
    for i in range(1, 3):
        sup = session.get(f"sup_transfer_{i}")
        if sup and sup.get("result"):
            result = sup["result"]
            coaching = _get_coaching_items(sup)
            coaching_str = ", ".join(coaching) if coaching else "none noted"
            lines.append(f"Supervisor Transfer {i}: {result}. Coaching: {coaching_str}.")
    return "\n".join(lines) if lines else "No coaching data recorded."


def build_clean_fail(session):
    name = session.get("candidate_name", "Candidate")
    auto_fail = session.get("auto_fail_reason")
    if auto_fail:
        af = auto_fail.lower()
        if "nc/ns" in af:
            return f"{name} was a No Call / No Show. Session did not occur."
        elif "stopped" in af:
            return f"{name} stopped responding in Discord during the session."
        elif "vpn" in af:
            return f"{name} is using a VPN and was unable to turn it off."
        elif "usb" in af or "noise" in af:
            return f"{name} did not have a qualifying headset: {auto_fail}."
        elif "not ready" in af:
            return f"{name} was not ready for the session: {auto_fail}."
        return f"{name} — {auto_fail}."
    fail_lines = []
    for i in range(1, 4):
        call = session.get(f"call_{i}")
        if call and call.get("result") == "Fail":
            ctype = call.get("type", "Unknown type")
            reasons = _get_fail_items(call)
            reasons_str = ", ".join(reasons) if reasons else "unspecified"
            fail_lines.append(f"Call {i} ({ctype}) failed: {reasons_str}.")
    for i in range(1, 3):
        sup = session.get(f"sup_transfer_{i}")
        if sup and sup.get("result") == "Fail":
            reasons = _get_fail_items(sup)
            reasons_str = ", ".join(reasons) if reasons else "unspecified"
            fail_lines.append(f"Supervisor Transfer {i} failed: {reasons_str}.")
    return "\n".join(fail_lines) if fail_lines else "N/A"


def generate_summaries(session, api_key=""):
    sup_only = session.get("supervisor_only", False)
    auto_fail = session.get("auto_fail_reason")
    calls_passed = sum(1 for i in range(1, 4) if (session.get(f"call_{i}") or {}).get("result") == "Pass")
    sups_passed = sum(1 for i in range(1, 3) if (session.get(f"sup_transfer_{i}") or {}).get("result") == "Pass")
    newbie = session.get("newbie_shift_data")

    fail_is_na = False
    if not auto_fail:
        if sup_only and sups_passed >= 1:
            fail_is_na = True
        elif not sup_only and calls_passed >= 2 and (sups_passed >= 1 or newbie):
            fail_is_na = True

    coaching = build_clean_coaching(session)
    fail = "N/A" if fail_is_na else build_clean_fail(session)
    return {"coaching": coaching, "fail": fail}


# ══════════════════════════════════════════════════════════════════
# APP SETUP
# ══════════════════════════════════════════════════════════════════
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Ensure default settings exist
    existing = await db.settings.find_one({"_id": "app_settings"})
    if not existing:
        await db.settings.insert_one({"_id": "app_settings", **DEFAULT_SETTINGS})
        logger.info("[STARTUP] Created default settings")
    logger.info(f"[STARTUP] Mock Testing Suite v{APP_VERSION}")
    yield
    client.close()
    logger.info("[SHUTDOWN] Server stopped")


app = FastAPI(title="Mock Testing Suite", version=APP_VERSION, lifespan=lifespan)
api_router = APIRouter(prefix="/api")

app.add_middleware(
    CORSMiddleware,
    allow_credentials=True,
    allow_origins=os.environ.get('CORS_ORIGINS', '*').split(','),
    allow_methods=["*"],
    allow_headers=["*"],
)


# ══════════════════════════════════════════════════════════════════
# SETTINGS ROUTES
# ══════════════════════════════════════════════════════════════════
@api_router.get("/settings")
async def get_settings():
    doc = await db.settings.find_one({"_id": "app_settings"}, {"_id": 0})
    if not doc:
        return dict(DEFAULT_SETTINGS)
    return doc


@api_router.put("/settings")
async def save_settings(payload: dict):
    await db.settings.update_one(
        {"_id": "app_settings"},
        {"$set": payload},
        upsert=True
    )
    return {"ok": True}


@api_router.get("/settings/defaults")
async def get_defaults():
    return {
        "call_types": CALL_TYPES,
        "sup_reasons": SUP_REASONS,
        "shows": SHOWS,
        "donors_new": NEW_DONORS,
        "donors_existing": EXISTING_MEMBERS,
        "donors_increase": INCREASE_SUSTAINING,
        "discord_templates": DISCORD_TEMPLATES,
        "payment": DEFAULT_PAYMENT,
        "tech_issues": TECH_ISSUES,
        "auto_fail_reasons": AUTO_FAIL_REASONS,
    }


@api_router.post("/settings/complete-setup")
async def complete_setup(payload: dict):
    update_data = {
        "setup_complete": True,
        "tester_name": payload.get("tester_name", ""),
        "display_name": payload.get("display_name", ""),
    }
    if "form_url" in payload:
        update_data["form_url"] = payload["form_url"]
    if "cert_sheet_url" in payload:
        update_data["cert_sheet_url"] = payload["cert_sheet_url"]
    await db.settings.update_one({"_id": "app_settings"}, {"$set": update_data}, upsert=True)
    return {"ok": True}


# ══════════════════════════════════════════════════════════════════
# SESSION ROUTES
# ══════════════════════════════════════════════════════════════════
@api_router.get("/session/current")
async def get_current_session():
    doc = await db.sessions.find_one({"_id": "active_session"}, {"_id": 0})
    if doc:
        return {"session": doc, "has_active": bool(doc.get("candidate_name"))}
    return {"session": None, "has_active": False}


@api_router.post("/session/start")
async def start_session(payload: dict):
    session = empty_session()
    session.update(payload)
    session["last_saved"] = datetime.now(timezone.utc).strftime("%I:%M %p")
    await db.sessions.replace_one({"_id": "active_session"}, {"_id": "active_session", **session}, upsert=True)
    return {"ok": True, "session": session}


@api_router.put("/session/update")
async def update_session(payload: dict):
    payload["last_saved"] = datetime.now(timezone.utc).strftime("%I:%M %p")
    await db.sessions.update_one({"_id": "active_session"}, {"$set": payload}, upsert=True)
    doc = await db.sessions.find_one({"_id": "active_session"}, {"_id": 0})
    return {"ok": True, "session": doc}


@api_router.post("/session/call")
async def save_call(payload: dict):
    key = f"call_{payload.get('call_num', 1)}"
    await db.sessions.update_one({"_id": "active_session"}, {"$set": {key: payload}})
    return {"ok": True}


@api_router.post("/session/sup")
async def save_sup(payload: dict):
    key = f"sup_transfer_{payload.get('transfer_num', 1)}"
    await db.sessions.update_one({"_id": "active_session"}, {"$set": {key: payload}})
    return {"ok": True}


@api_router.post("/session/finish")
async def finish_session_simple():
    doc = await db.sessions.find_one({"_id": "active_session"}, {"_id": 0})
    if not doc:
        return {"ok": False, "error": "No active session"}
    record = {
        "timestamp": datetime.now(timezone.utc).strftime("%Y-%m-%d %I:%M %p"),
        "candidate": doc.get("candidate_name", "Unknown"),
        "tester_name": doc.get("tester_name", ""),
        "status": doc.get("final_status", "Fail"),
        **doc,
    }
    await db.history.insert_one(record)
    await db.sessions.delete_one({"_id": "active_session"})
    return {"ok": True, "record": {k: v for k, v in record.items() if k != "_id"}}


@api_router.post("/session/discard")
async def discard_session():
    await db.sessions.delete_one({"_id": "active_session"})
    return {"ok": True}


# ══════════════════════════════════════════════════════════════════
# HISTORY ROUTES
# ══════════════════════════════════════════════════════════════════
@api_router.get("/history")
async def get_history():
    docs = await db.history.find({}, {"_id": 0}).sort("timestamp", -1).to_list(500)
    return docs


@api_router.get("/history/stats")
async def get_history_stats():
    docs = await db.history.find({}, {"_id": 0, "status": 1}).to_list(5000)
    total = len(docs)
    passes = sum(1 for d in docs if d.get("status") == "Pass")
    fails = sum(1 for d in docs if d.get("status") == "Fail")
    ncns = sum(1 for d in docs if d.get("status") == "NC/NS" or (d.get("auto_fail_reason") or "").lower().startswith("nc"))
    incomplete = sum(1 for d in docs if d.get("status") == "Incomplete")
    pass_rate = round((passes / total * 100) if total > 0 else 0, 1)
    return {"total": total, "passes": passes, "fails": fails, "ncns": ncns, "incomplete": incomplete, "pass_rate": pass_rate}


@api_router.delete("/history")
async def clear_history():
    await db.history.delete_many({})
    return {"ok": True}


# ══════════════════════════════════════════════════════════════════
# TICKER
# ══════════════════════════════════════════════════════════════════
@api_router.get("/ticker")
async def get_ticker():
    return {"messages": TICKER_MESSAGES}


# ══════════════════════════════════════════════════════════════════
# GEMINI / SUMMARIES
# ══════════════════════════════════════════════════════════════════
@api_router.post("/gemini/summaries")
async def gen_summaries():
    doc = await db.sessions.find_one({"_id": "active_session"}, {"_id": 0})
    if not doc:
        return {"coaching": "No active session.", "fail": "No active session."}
    settings = await db.settings.find_one({"_id": "app_settings"}, {"_id": 0})
    api_key = ""
    if settings and settings.get("enable_gemini"):
        api_key = settings.get("gemini_key", "")
    result = generate_summaries(doc, api_key)
    return result


@api_router.post("/gemini/regenerate")
async def regen_summary(payload: dict):
    summary_type = payload.get("type", "coaching")
    doc = await db.sessions.find_one({"_id": "active_session"}, {"_id": 0})
    if not doc:
        return {"ok": False, "error": "No active session"}
    settings = await db.settings.find_one({"_id": "app_settings"}, {"_id": 0})
    api_key = ""
    if settings and settings.get("enable_gemini"):
        api_key = settings.get("gemini_key", "")
    result = generate_summaries(doc, api_key)
    return {"ok": True, "text": result.get(summary_type, "")}


# ══════════════════════════════════════════════════════════════════
# FINISH SESSION (orchestrator)
# ══════════════════════════════════════════════════════════════════
@api_router.post("/finish-session")
async def finish_all(payload: dict):
    doc = await db.sessions.find_one({"_id": "active_session"}, {"_id": 0})
    if not doc:
        return {"ok": False, "error": "No active session"}
    record = {
        "timestamp": datetime.now(timezone.utc).strftime("%Y-%m-%d %I:%M %p"),
        "candidate": doc.get("candidate_name", "Unknown"),
        "tester_name": doc.get("tester_name", ""),
        "status": doc.get("final_status", "Fail"),
        "coaching_summary": payload.get("coaching_summary", ""),
        "fail_summary": payload.get("fail_summary", ""),
        **doc,
    }
    await db.history.insert_one(record)
    await db.sessions.delete_one({"_id": "active_session"})
    return {"ok": True}


# ══════════════════════════════════════════════════════════════════
# UPDATE / FORM (stubs)
# ══════════════════════════════════════════════════════════════════
@api_router.get("/update")
async def check_update():
    return {"update_available": False, "current_version": APP_VERSION}


@api_router.get("/update/status")
async def update_status():
    return {"update_available": False, "current_version": APP_VERSION}


@api_router.post("/form/fill")
async def fill_form(payload: dict):
    return {"ok": True, "message": "Form filling is available in the desktop version. In the web version, use the Copy buttons to copy summaries."}


@api_router.get("/")
async def root():
    return {"message": f"Mock Testing Suite API v{APP_VERSION}"}


app.include_router(api_router)
