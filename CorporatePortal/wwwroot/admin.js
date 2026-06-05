const api = '/api/portal';
const el = id => document.getElementById(id);
const baseWidgets = [
  { type: 'clock', title: 'Часы' },
  { type: 'date', title: 'Дата' },
  { type: 'weather', title: 'Погода (заглушка)' },
  { type: 'quote', title: 'Цитата дня' }
];
let pages = [];
let currentPage;

async function load() {
  pages = await fetch(`${api}/pages`).then(r => r.json());
  if (!pages.length) return;
  currentPage = currentPage ? (pages.find(p => p.id === currentPage.id) || pages[0]) : pages[0];
  renderPageSelect();
  render();
  initSortables();
}

function renderPageSelect() {
  el('pageSelect').innerHTML = pages.map(p => `<option value="${p.id}">${p.title}</option>`).join('');
  el('pageSelect').value = currentPage.id;
  el('pageSelect').onchange = async e => { currentPage = await fetch(`${api}/pages/${e.target.value}`).then(r => r.json()); render(); initSortables(); };
}

function cssVarsFromTheme() {
  const accent = el('accentColor').value;
  const card = el('cardBgColor').value;
  const op = el('cardOpacity').value;
  const rgb = parseInt(card.slice(1), 16);
  const r = (rgb >> 16) & 255, g = (rgb >> 8) & 255, b = rgb & 255;
  return `:root{--accent:${accent};--card-bg:rgba(${r},${g},${b},${op});}`;
}

function getGroups() { return [...(currentPage.linkGroups || [])].sort((a, b) => a.sortOrder - b.sortOrder); }
function getWidgets() { return [...(currentPage.widgets || [])].sort((a, b) => a.sortOrder - b.sortOrder); }

function render() {
  el('titleInput').value = currentPage.title;
  el('welcomeInput').value = currentPage.welcomeText;
  el('bgType').value = currentPage.backgroundType;
  el('bgValue').value = currentPage.backgroundValue || '';
  el('themeCss').value = currentPage.themeCss || '';

  const groups = getGroups();
  el('groupSelect').innerHTML = groups.map(g => `<option value="${g.id}">${g.title}</option>`).join('');
  el('groupSortable').innerHTML = groups.map(g => `<li data-id="${g.id}">☰ ${g.title}</li>`).join('');

  const groupId = Number(el('groupSelect').value || groups[0]?.id);
  const links = (groups.find(g => g.id === groupId)?.links || []).sort((a, b) => a.sortOrder - b.sortOrder);
  el('linkSortable').innerHTML = links.map(l => `<li data-id="${l.id}">☰ ${l.title}</li>`).join('');

  const enabled = new Set(getWidgets().filter(w => w.isVisible).map(w => w.type));
  document.querySelectorAll('.widget-toggle').forEach(c => c.checked = enabled.has(c.value));
  el('widgetSortable').innerHTML = getWidgets().map(w => `<li data-id="${w.id}">☰ ${w.title} <span class="hint">(${w.type})</span></li>`).join('');

  el('preview').innerHTML = `<style>${currentPage.themeCss || ''}</style><h3>${currentPage.title}</h3><p>${currentPage.welcomeText}</p>` +
    groups.map(g => `<article class="group"><h4>${g.title}</h4>${(g.links || []).sort((a,b)=>a.sortOrder-b.sortOrder).map(l => `<a>${l.title}</a>`).join('')}</article>`).join('');
}

async function persistPage() {
  currentPage = await fetch(`${api}/pages/${currentPage.id}`, {
    method: 'PUT', headers: { 'content-type': 'application/json' }, body: JSON.stringify(currentPage)
  }).then(r => r.json());
}

function initSortables() {
  Sortable.create(el('groupSortable'), { animation: 150, onEnd: async () => {
    const ids = [...el('groupSortable').children].map(x => Number(x.dataset.id));
    currentPage = await fetch(`${api}/pages/${currentPage.id}/groups/reorder`, { method: 'PUT', headers: { 'content-type': 'application/json' }, body: JSON.stringify(ids)}).then(r => r.json());
    render();
  }});

  Sortable.create(el('linkSortable'), { animation: 150, onEnd: async () => {
    const ids = [...el('linkSortable').children].map(x => Number(x.dataset.id));
    const gid = el('groupSelect').value;
    currentPage = await fetch(`${api}/pages/${currentPage.id}/groups/${gid}/links/reorder`, { method: 'PUT', headers: { 'content-type': 'application/json' }, body: JSON.stringify(ids)}).then(r => r.json());
    render();
  }});

  Sortable.create(el('widgetSortable'), { animation: 150, onEnd: async () => {
    const ids = [...el('widgetSortable').children].map(x => Number(x.dataset.id));
    currentPage.widgets = ids.map((id, i) => ({ ...currentPage.widgets.find(w => w.id === id), sortOrder: i + 1 }));
    await persistPage();
    render();
  }});
}

el('savePage').onclick = async () => {
  currentPage = { ...currentPage, title: el('titleInput').value, welcomeText: el('welcomeInput').value, backgroundType: el('bgType').value, backgroundValue: el('bgValue').value, themeCss: el('themeCss').value };
  await persistPage();
  await load();
};
el('createPage').onclick = async () => { await fetch(`${api}/pages`, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify({ title: `Новая страница ${pages.length+1}`, welcomeText:'Описание страницы', backgroundType:'color', backgroundValue:'#eef2ff', isActive:true, widgets:[] })}); await load(); };
el('addGroup').onclick = async () => { await fetch(`${api}/pages/${currentPage.id}/groups`, { method:'POST', headers:{'content-type':'application/json'}, body:JSON.stringify({ title: el('groupTitle').value, sortOrder: getGroups().length + 1, isVisible:true })}); currentPage = await fetch(`${api}/pages/${currentPage.id}`).then(r => r.json()); render(); initSortables(); };
el('addLink').onclick = async () => { const gid = el('groupSelect').value; const links = getGroups().find(g => g.id === Number(gid))?.links || []; await fetch(`${api}/pages/${currentPage.id}/groups/${gid}/links`, { method:'POST', headers:{'content-type':'application/json'}, body:JSON.stringify({ title:el('linkTitle').value, url:el('linkUrl').value, sortOrder: links.length + 1, isActive:true, openInNewTab:true })}); currentPage = await fetch(`${api}/pages/${currentPage.id}`).then(r => r.json()); render(); initSortables(); };
el('groupSelect').onchange = () => { render(); initSortables(); };
el('applyTheme').onclick = () => { el('themeCss').value = cssVarsFromTheme(); };
el('addEmbedWidget').onclick = async () => {
  currentPage.widgets = getWidgets().concat([{ id: Date.now(), type: 'embed', title: el('embedTitle').value || 'Embed блок', sortOrder: getWidgets().length + 1, isVisible: true, embedHtml: el('embedHtml').value }]);
  await persistPage();
  render();
};
document.querySelectorAll('.widget-toggle').forEach(c => c.onchange = async () => {
  let widgets = getWidgets();
  const found = widgets.find(w => w.type === c.value);
  if (!found && c.checked) widgets.push({ id: Date.now() + Math.floor(Math.random()*999), type: c.value, title: baseWidgets.find(x => x.type === c.value).title, sortOrder: widgets.length + 1, isVisible: true });
  widgets = widgets.map(w => w.type === c.value ? { ...w, isVisible: c.checked } : w);
  currentPage.widgets = widgets;
  await persistPage();
  render();
});

load();
