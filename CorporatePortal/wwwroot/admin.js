const api = '/api/portal';
let pages = [];
let currentPage;

const el = id => document.getElementById(id);

async function load() {
  pages = await fetch(`${api}/pages`).then(r => r.json());
  if (!pages.length) return;

  if (!currentPage) {
    currentPage = pages[0];
  } else {
    currentPage = pages.find(p => p.id === currentPage.id) || pages[0];
  }

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

function render() {
  el('titleInput').value = currentPage.title;
  el('welcomeInput').value = currentPage.welcomeText;
  el('bgType').value = currentPage.backgroundType;
  el('bgValue').value = currentPage.backgroundValue || '';

  const groups = [...currentPage.linkGroups].sort((a, b) => a.sortOrder - b.sortOrder);
  el('groupSelect').innerHTML = groups.map(g => `<option value="${g.id}">${g.title}</option>`).join('');

  el('preview').innerHTML = `
    <h3>${currentPage.title}</h3>
    <p>${currentPage.welcomeText}</p>
    ${groups.map(g => `
      <article class="group">
        <h4>${g.title}</h4>
        ${(g.links || []).sort((a, b) => a.sortOrder - b.sortOrder).map(l => `<a href="${l.url}" target="_blank">${l.title}</a>`).join('')}
      </article>
    `).join('')}
  `;
}

el('savePage').onclick = async () => {
  const payload = {
    ...currentPage,
    title: el('titleInput').value,
    welcomeText: el('welcomeInput').value,
    backgroundType: el('bgType').value,
    backgroundValue: el('bgValue').value
  };
  currentPage = await fetch(`${api}/pages/${currentPage.id}`, {
    method: 'PUT',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(payload)
  }).then(r => r.json());

  await load();
};

el('createPage').onclick = async () => {
  const payload = {
    title: `Новая страница ${pages.length + 1}`,
    welcomeText: 'Описание страницы',
    backgroundType: 'color',
    backgroundValue: '#eef2ff',
    isActive: true
  };
  await fetch(`${api}/pages`, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(payload)
  });
  await load();
};

el('addGroup').onclick = async () => {
  const payload = {
    title: el('groupTitle').value,
    sortOrder: currentPage.linkGroups.length + 1,
    isVisible: true
  };
  await fetch(`${api}/pages/${currentPage.id}/groups`, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(payload)
  });

  currentPage = await fetch(`${api}/pages/${currentPage.id}`).then(r => r.json());
  render();
};

el('addLink').onclick = async () => {
  const groupId = el('groupSelect').value;
  const selectedGroup = currentPage.linkGroups.find(g => g.id === Number(groupId));
  const payload = {
    title: el('linkTitle').value,
    url: el('linkUrl').value,
    sortOrder: (selectedGroup?.links?.length || 0) + 1,
    isActive: true,
    openInNewTab: true
  };

  await fetch(`${api}/pages/${currentPage.id}/groups/${groupId}/links`, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(payload)
  });

  currentPage = await fetch(`${api}/pages/${currentPage.id}`).then(r => r.json());
  render();
};

load();
