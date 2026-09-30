"""Dependency-free structural checks; run from any working directory."""
import json
import os
import re
import sys
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
errors = []


def check(condition, message):
    if not condition:
        errors.append(message)


required = [
    'README.md', 'AGENTS.md', 'CLAUDE.md', 'CONTRIBUTING.md', '.gitignore',
    *['docs/' + x + '.md' for x in ['VISION', 'GDD', 'ARCHITECTURE', 'ECONOMY',
      'COMMERCIAL', 'GROWTH', 'LIVEOPS', 'SECURITY', 'ROADMAP']],
    *['docs/lore/' + x + '.md' for x in ['LORE_BIBLE', 'CODEX', 'FORGEMASTER', 'MYTHICS']],
    'docs/art/ART_BIBLE.md', 'docs/adr/README.md', 'docs/adr/template.md',
    *['docs/product/' + x + '.md' for x in ['MASTER_BACKLOG', 'GROWTH_MASTER_BACKLOG',
      'OKRS', 'MILESTONES', 'FOUNDATION_REVIEW']],
    *[x + '/README.md' for x in ['game', 'backend', 'content', 'assets', 'blockchain', 'tests']],
    'content/entity.schema.json', '.github/ISSUE_TEMPLATE/task.yml',
    '.github/workflows/foundation.yml', '.github/PULL_REQUEST_TEMPLATE.md',
]
for name in required:
    check((ROOT / name).is_file(), f'Missing required file: {name}')

generated = {'.git', 'Library', 'Temp', 'Logs', 'UserSettings', 'bin', 'obj',
             'artifacts', 'node_modules', '.venv', '__pycache__'}
markdown = []
for directory, children, files in os.walk(ROOT):
    children[:] = [name for name in children if name not in generated]
    markdown.extend(Path(directory) / name for name in files if name.endswith('.md'))
for path in markdown:
    if '.git' in path.parts:
        continue
    body = path.read_text(encoding='utf-8')
    for target in re.findall(r'\[[^\]]*\]\(([^)]+)\)', body):
        target = target.strip('<>')
        url = urlsplit(target)
        if url.scheme or not url.path:
            continue
        resolved = (path.parent / unquote(url.path)).resolve()
        check(resolved.is_relative_to(ROOT), f'Link leaves repository: {path}: {target}')
        check(resolved.exists(), f'Broken local link: {path.relative_to(ROOT)}: {target}')

try:
    schema = json.loads((ROOT / 'content/entity.schema.json').read_text(encoding='utf-8'))
    props = schema['properties']
    check(props['collection']['properties']['rarities']['items']['enum'] ==
          ['Common', 'Uncommon', 'Rare', 'Epic', 'Legendary', 'Infernal'], 'Rarity drift')
    pattern = props['codexId']['pattern']
    for n in range(1000):
        check(bool(re.fullmatch(pattern, f'{n:03d}')) == (1 <= n <= 666),
              f'Canonical ID boundary failure: {n}')
    check(schema['allOf'][0]['else'] == {'not': {'required': ['codexId']}},
          'Anomalous entities must omit canonical IDs')
except (OSError, ValueError, KeyError, TypeError) as exc:
    errors.append(f'Schema structure error: {exc}')

try:
    backlog = (ROOT / 'docs/product/MASTER_BACKLOG.md').read_text(encoding='utf-8')
    ids = re.findall(r'^\| (DC-\d{4}) \|', backlog, re.M)
    check(len(ids) == len(set(ids)), 'Duplicate task IDs')
    check(len(ids) >= 35, 'Expected initial workstreams are missing')
    states = {'BACKLOG', 'READY', 'IN PROGRESS', 'REVIEW / TEST', 'BLOCKED', 'DONE'}
    for line in backlog.splitlines():
        if not re.match(r'\| DC-\d{4} \|', line):
            continue
        cols = [c.strip() for c in line.strip('|').split('|')]
        check(len(cols) == 10 and all(cols), f'Incomplete task: {line}')
        if len(cols) != 10:
            continue
        check(cols[8] in states, f'Invalid state: {cols[0]}')
        for dep in re.findall(r'DC-\d{4}', cols[5]):
            check(dep in ids and dep != cols[0], f'Invalid dependency: {cols[0]} -> {dep}')
    milestone = (ROOT / 'docs/product/MILESTONES.md').read_text(encoding='utf-8')
    for exclusion in ['blockchain', 'NFTs', 'marketplace', 'physical redemption',
                      'real-money economy', 'production backend', 'live multiplayer',
                      '666 generated demons', 'elaborate LiveOps']:
        check(exclusion in milestone, f'M001 exclusion missing: {exclusion}')
except OSError as exc:
    errors.append(str(exc))

if errors:
    print('\n'.join('FAIL: ' + error for error in errors))
    sys.exit(1)
print(f'PASS: {len(required)} required files, {len(markdown)} Markdown files, '
      f'{len(ids)} tasks, local links, schema boundaries and M001 exclusions.')
print('Manual review still required for canon, restricted content and product consistency.')
