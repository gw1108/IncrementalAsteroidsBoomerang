# Session Export — 2026-04-23

Topic: Research on tools/skills/MCPs for an AI pipeline that turns a gameplay video + prompt into a GDD/PRD and then a production plan.

---

## Prompt 1 (user)

> Start a web-search-researcher.md agent that will answer this research question: What tools, skills, or MCPs would I need to have an AI agent be able to watch a video of a video game plus a prompt and develop a product requirement document/game design document. The goal is to take this document and create a production plan to implement the features described. Assume the video is something a human has recorded of their screen. It can also have an overlay of keyboard input or have a text representation of keystrokes.

## Assistant action

Launched `web-search-researcher` subagent with a structured brief covering: video-capable LLMs and frame-extraction tooling, OCR, keystroke-log fusion, existing gameplay-understanding research, relevant MCP servers, GDD generation patterns, and practical limits.

## Research agent report

### AI Agent Pipeline for Video-to-GDD-to-Production-Plan

#### Video Understanding / Multimodal Input

**Which LLMs accept video natively (as of April 2026)**

- **Gemini 2.5 Pro** — the clear leader. Native video alongside images, audio, text in a single prompt. 1M token context (2M coming). ~258 tokens/sec at 1 FPS. $1.25–$2.50 per million input tokens. A 10-minute gameplay recording at 1 FPS ≈ 154,800 tokens ≈ $0.19–$0.39 on input. Practical cutoff is ~20–30 min per call given output budget + overhead.
- **GPT-4o** — no true native video via API as of April 2026. Must pre-convert to frames (2–4 FPS). $2.50/M input tokens.
- **Claude (Sonnet 4.6 / Opus 4.7)** — images + text only. No video, no audio. Workaround: extract frames externally and pass as image sequence; less efficient than Gemini.

**Architectural implication**: use Gemini 2.5 Pro for raw video comprehension, then hand structured text (timestamped scene descriptions, inferred mechanics, HUD state) to Claude for PRD/GDD authoring and planning, where Claude's writing quality is stronger.

**Frame extraction and chunking**

- `llm-video-frames` (Simon Willison) — CLI plugin using ffmpeg, configurable FPS, stamps timestamp overlay on each frame. https://github.com/simonw/llm-video-frames
- `videostil` — FPS-configurable extractor with embedding-based dedup; important for game recordings with static HUD.
- **Katna** — Python library for smart keyframe extraction maximizing visual information change; better than uniform sampling for detecting state transitions. https://github.com/keplerlab/katna
- `ffmpeg-mcp` (AmolDerickSoans) — full ffmpeg as MCP tools (transcode, trim, filter). https://github.com/AmolDerickSoans/ffmpeg-mcp
- `video-audio-mcp` (misbahsy) — simpler ffmpeg wrapper.

**Recommended sampling**: 1–2 FPS for menu/UI sections; 4–6 FPS for action sequences. Pre-pass at 0.5 FPS to index scenes, then targeted higher-FPS second pass on flagged segments.

**OCR for on-screen text**

- **PaddleOCR MCP (official)** — 100+ languages, bounding boxes, confidence scores. Best open-source for structured game UI.
- `mcp-ocr` — Tesseract wrapper, fully local, lower accuracy on stylized fonts.
- **Mistral OCR MCP** — cloud, strong on complex layouts including tables.
- Gemini's native vision already reads on-screen text; dedicated OCR only needed for bounding-box precision or cheaper pre-filter.

**Fusing keystroke log with video timeline**

1. Export keystrokes as `[timestamp_ms, key, event_type]` JSON.
2. Stamp each extracted frame with its timestamp.
3. Interleave in context:

```
[00:02:14] Frame → <image>
Inputs in prior 1s: W (held), SPACE (pressed), LMB (held)

[00:02:15] Frame → <image>
Inputs in prior 1s: W (held), E (pressed)
```

Works directly with `llm-video-frames` + a small preprocessing script. Prompt engineering problem, not infrastructure.

#### Gameplay-Specific Analysis

- **SIMA / SIMA 2 (DeepMind)** — player agent, not analyzer. Closed source. Evidence multimodal game understanding is maturing but not reusable as API. https://deepmind.google/blog/sima-generalist-ai-agent-for-3d-virtual-environments/
- **VideoGameBench** — 23 games, VLM agents must complete levels from raw frames. *No current model (GPT-4o, Claude Sonnet 3.7, Gemini 2.5 Pro) completed even level 1 of any game.* Critical: LLMs can describe but cannot reliably infer mechanics without scaffolding. https://www.vgbench.com/
- **GameplayQA** (arXiv 2603.24329) — benchmark for POV multi-video understanding; highlights failure of existing benchmarks on action games.
- **VPT (OpenAI)** — Minecraft agent trained on 70K hours of gameplay. Produces action-conditioned models, not descriptions. Research only.
- **Automated GDD-to-Unity** (arXiv 2509.08847) — fine-tuned LLaMA-3 parses GDDs and generates Unity C# templates (4.8/5.0 on compilation/adherence). Not released but architecture replicable in Claude Code.

#### MCP Servers Relevant to the Pipeline

| Stage | Server | Notes |
|---|---|---|
| Video processing | `ffmpeg-mcp` (AmolDerickSoans) | Full ffmpeg |
| Video processing | `video-audio-mcp` (misbahsy) | Simpler editing wrapper |
| YouTube ingestion | `mcp-youtube` (anaisbetts) | Subtitle/transcript extraction |
| YouTube ingestion | `youtube-mcp-server` (mourad-ghafiri) | VAD transcription, 99 languages |
| OCR | PaddleOCR MCP (official) | Bounding boxes |
| OCR | `mcp-ocr` | Tesseract, local |
| OCR | Mistral OCR MCP | Cloud, complex layouts |
| Documents | Notion MCP (official) | PRDs, databases, wikis |
| Documents | Google Drive MCP (Anthropic) | Docs/Sheets |
| Tasks | Linear MCP | Issues, milestones |
| Tasks | Jira MCP | JQL, tickets |
| Tasks | **GitHub MCP (official)** | Best fit for GitHub-hosted project |
| Tasks | Asana MCP (official) | Hosted server |
| Unity impl | **Coplay MCP** | 86 Unity Editor tools; already installed in this env |

For a Unity dev on GitHub: GitHub MCP is the natural fit (Issues = task tree, Milestones = GDD phases, Projects = kanban).

**Coplay MCP** exposes scene manipulation, component config, prefab creation, material gen, Input Actions setup, UI element creation. Once a task tree exists, Claude can execute directly in the running Unity Editor. https://github.com/CoplayDev/unity-mcp — https://docs.coplay.dev/coplay-mcp/claude-code-guide

#### Document Generation and Planning Patterns

**Three-pass prompt pattern for GDD extraction:**

1. **Inventory pass** (Gemini) — "Enumerate every distinct mechanic, UI screen, and system. Note timestamp of first appearance and quote any on-screen text." Output: flat JSON feature inventory.
2. **Structure pass** (Claude) — "Using the inventory, write a GDD with sections: Core Loop, Controls, Systems, UI/HUD, Progression, Open Questions. For each feature mark **observed vs. inferred**."
3. **Gap-flagging pass** (Claude) — reviews draft, flags every inference not directly shown (e.g., "save system inferred from save icon — not demonstrated"). These become mandatory human review items.

**MetaGPT** — alternative multi-agent framework with PM/architect/engineer/QA roles. IBM tutorial with DeepSeek shows structured markdown output. Separate framework from Claude Code. https://www.ibm.com/think/tutorials/multi-agent-prd-ai-automation-metagpt-ollama-deepseek

**Claude Code Game Studios template** (Donchitos) — 49 agents, 72 slash commands, 7-phase pipeline. Studio hierarchy (Directors → Department Leads → Specialists) with approval gates. Forkable. https://github.com/Donchitos/Claude-Code-Game-Studios

**Recommended decomposition**:

```
Orchestrator agent reads GDD
  → spawns parallel subagents per GDD section:
      - "Systems" subagent: identifies C# classes needed
      - "UI" subagent: identifies prefabs and Canvas elements
      - "Data" subagent: identifies ScriptableObject schemas
  → each subagent writes tasks to a shared tasks/todo.md
  → orchestrator merges, de-duplicates, and orders by dependency
  → pushes tasks to GitHub Issues via GitHub MCP
```

Use Claude Code's `Task` tool (isolated-context subagents), not sequential tool calls, to prevent context bleed.

#### Gotchas and Limits

- **Cost**: 30 min at 1 FPS ≈ 464K tokens ≈ $0.58 just to read. At 2 FPS, double. Keep recordings under 20 min or pre-segment by phase.
- **Hallucination hot spots**:
  - Inferred mechanics not directly shown (crafting inferred from inventory existing).
  - Numbers/stats on stylized fonts (OCR 70–80%, not 99%).
  - Off-screen systems (save, load, multiplayer) that are never demonstrated.
- **Sampling gaps**: 200ms button presses can vanish at 1 FPS. Run second-pass 4–6 FPS on flagged "mechanic demo" segments.
- **Mandatory HITL checkpoints**:
  1. After feature inventory — before GDD is written.
  2. After GDD "Open Questions" is populated — designer fills gaps.
  3. After task tree — before Issues are created (dependency review).
  4. After first Coplay MCP iteration — confirm Editor state matches GDD intent.

#### Compact Shortlist (Unity + Claude Code)

| Stage | Tool |
|---|---|
| Video read | Gemini 2.5 Pro API |
| Frame extraction | `llm-video-frames` or `ffmpeg-mcp` |
| Smart keyframes | Katna |
| OCR (optional) | PaddleOCR MCP |
| Keystroke fusion | Custom preprocessing script |
| GDD authoring | Claude Opus/Sonnet + 3-pass prompt |
| Multi-agent PRD (optional) | MetaGPT |
| Decomposition | Claude Code subagents + Game Studios template |
| Task storage | GitHub MCP |
| Document storage | Notion MCP |
| Unity implementation | Coplay MCP (already installed) |

#### Sources

- LLM Usage Limits 2026 — exploreaitogether.com/llm-usage-limits-comparison/
- Multimodal input processing in AI chatbots — datastudios.org
- Gemini Developer API pricing — ai.google.dev/gemini-api/docs/pricing
- Gemini 2.5 Pro release — futureagi.com/blogs/google-gemini-2-5-pro-2025
- Can ChatGPT Watch Videos? 2025 Guide — glbgpt.com
- Claude Vision — platform.claude.com/docs/en/build-with-claude/vision
- simonw/llm-video-frames — github.com/simonw/llm-video-frames
- Simon Willison blog on video frames — simonwillison.net/2025/May/5/llm-video-frames/
- Katna — github.com/keplerlab/katna
- ffmpeg-mcp — github.com/AmolDerickSoans/ffmpeg-mcp
- video-audio-mcp — github.com/misbahsy/video-audio-mcp
- mcp-youtube — github.com/anaisbetts/mcp-youtube
- youtube-mcp-server — github.com/mourad-ghafiri/youtube-mcp-server
- PaddleOCR MCP — pulsemcp.com/servers/paddlepaddle-paddleocr
- OCR MCP roundup — chatforest.com/reviews/ocr-document-intelligence-mcp-servers/
- Mistral OCR MCP — pulsemcp.com/servers/everaldo-mistral-ocr
- VideoGameBench — vgbench.com and vgbench.com/blog.html
- GameplayQA — arxiv.org/abs/2603.24329
- SIMA — deepmind.google/blog/sima-generalist-ai-agent-for-3d-virtual-environments/
- Automated Unity Game Template Generation — arxiv.org/html/2509.08847
- Notion MCP — developers.notion.com/guides/mcp/mcp and github.com/makenotion/notion-mcp-server
- Asana MCP — developers.asana.com/docs/using-asanas-mcp-server
- Awesome MCP Servers (tasks) — github.com/TensorBlock/awesome-mcp-servers
- Claude Code MCP docs — code.claude.com/docs/en/mcp
- Coplay MCP — coplay.dev/coplay-mcp and github.com/CoplayDev/unity-mcp
- Coplay + Claude Code setup — docs.coplay.dev/coplay-mcp/claude-code-guide
- MetaGPT multi-agent PRD tutorial — ibm.com/think/tutorials/multi-agent-prd-ai-automation-metagpt-ollama-deepseek
- Claude-Code-Game-Studios — github.com/Donchitos/Claude-Code-Game-Studios
- Claude Code subagents — code.claude.com/docs/en/sub-agents
- Multimodal LLM Pipeline for Video Understanding — eng-mhasan.medium.com
- Hallucination detection — futureagi.com/blogs/detect-hallucination-generative-ai-2025

---

## Prompt 2 (user)

> Export this session's prompts and results to a .md file.

## Assistant action

Wrote this file to `session-2026-04-23-video-to-gdd-research.md` at the project root.
