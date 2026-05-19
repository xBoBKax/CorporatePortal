const api = '/api/portal';
let pages = [];
let currentPage;

const el = id => document.getElementById(id);

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
  el('pageSelect').onchange = async e => {
    currentPage = await fetch(`${api}/pages/${e.target.value}`).then(r => r.json());
    render();
  };
}

function applyBackground() {
  const { backgroundType, backgroundValue } = currentPage;
  if (backgroundType === 'image') {
    document.body.style.background = `url(${backgroundValue}) center/cover fixed`;
  } else {
    document.body.style.background = backgroundValue || '#f2f4f8';
  }
  document.body.style.color = backgroundType === 'gradient' ? '#fff' : '#111';
}

function render() {
  applyBackground();
  el('pageTitle').textContent = currentPage.title;
  el('welcomeText').textContent = currentPage.welcomeText;

  const groups = [...currentPage.linkGroups].sort((a, b) => a.sortOrder - b.sortOrder);
  el('groupsContainer').innerHTML = groups.map(g => `
    <article class="group">
      <h3>${g.title}</h3>
      ${(g.links || []).sort((a, b) => a.sortOrder - b.sortOrder).map(l => `<a href="${l.url}" target="_blank">${l.title}</a>`).join('')}
    </article>
  `).join('');
}

setInterval(() => {
  const now = new Date();
  el('clock').textContent = `⏰ ${now.toLocaleTimeString('ru-RU')}`;
  el('today').textContent = `📅 ${now.toLocaleDateString('ru-RU', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' })}`;
}, 1000);

load();
