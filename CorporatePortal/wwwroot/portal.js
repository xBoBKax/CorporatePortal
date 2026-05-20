const api = '/api/portal';
const el = id => document.getElementById(id);
let pages = [];
let currentPage;

async function load() {
  pages = await fetch(`${api}/pages`).then(r => r.json());
  if (!pages.length) return;
  currentPage = pages[0];
  renderPageSelect();
  render();
}

function renderPageSelect() {
  el('pageSelect').innerHTML = pages.map(p => `<option value="${p.id}">${p.title}</option>`).join('');
  el('pageSelect').value = currentPage.id;
  el('pageSelect').onchange = async e => { currentPage = await fetch(`${api}/pages/${e.target.value}`).then(r => r.json()); render(); };
}

function applyBackground() {
  const { backgroundType, backgroundValue } = currentPage;
  document.body.style.background = backgroundType === 'image' ? `url(${backgroundValue}) center/cover fixed` : (backgroundValue || '#f2f4f8');
  document.body.style.color = backgroundType === 'gradient' ? '#fff' : '#111';
}

function widgetContent(widget) {
  const now = new Date();
  if (widget.type === 'clock') return `⏰ ${now.toLocaleTimeString('ru-RU')}`;
  if (widget.type === 'date') return `📅 ${now.toLocaleDateString('ru-RU',{weekday:'long',day:'2-digit',month:'long',year:'numeric'})}`;
  if (widget.type === 'weather') return '🌤 Погода: подключите погодный API';
  if (widget.type === 'quote') return '💡 "Делаем хорошо или делаем снова"';
  if (widget.type === 'embed') return widget.embedHtml || '<em>Пустой embed</em>';
  return '—';
}

function renderWidgets() {
  const widgets = (currentPage.widgets || []).filter(w => w.isVisible).sort((a,b)=>a.sortOrder-b.sortOrder);
  el('widgets').innerHTML = widgets.map(w => `<article class="widget"><h3>${w.title}</h3><div>${widgetContent(w)}</div></article>`).join('');
}

function render() {
  applyBackground();
  el('pageTitle').textContent = currentPage.title;
  el('welcomeText').textContent = currentPage.welcomeText;
  document.getElementById('themeStyle').textContent = currentPage.themeCss || '';
  const groups = [...currentPage.linkGroups].sort((a,b)=>a.sortOrder-b.sortOrder);
  el('groupsContainer').innerHTML = groups.map(g => `<article class="group"><h3>${g.title}</h3>${(g.links||[]).sort((a,b)=>a.sortOrder-b.sortOrder).map(l => `<a href="${l.url}" target="_blank">${l.title}</a>`).join('')}</article>`).join('');
  renderWidgets();
}

setInterval(() => { if (currentPage) renderWidgets(); }, 1000);
load();
