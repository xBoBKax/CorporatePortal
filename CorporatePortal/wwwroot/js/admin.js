const state = { page: null, groups: [], links: [], modules: [], birthdays: [] };
const toast = document.querySelector('#toast');

init().catch(showError);

document.querySelectorAll('[data-reset]').forEach(button => {
  button.addEventListener('click', () => resetForm(button.dataset.reset));
});

document.querySelector('#page-form').addEventListener('submit', submitPage);
document.querySelector('#group-form').addEventListener('submit', submitGroup);
document.querySelector('#link-form').addEventListener('submit', submitLink);
document.querySelector('#module-form').addEventListener('submit', submitModule);
document.querySelector('#birthday-form').addEventListener('submit', submitBirthday);

async function init() {
  await reload();
  renderAll();
}

async function reload() {
  const page = await fetchJson('/api/portal-pages/active');
  state.page = page;
  state.groups = page.linkGroups;
  state.links = page.linkGroups.flatMap(group => group.links.map(link => ({ ...link, groupTitle: group.title })));
  state.modules = page.modules;
  state.birthdays = await fetchJson('/api/employee-birthdays');
}

function renderAll() {
  fillPageForm();
  renderGroups();
  renderGroupSelect();
  renderLinks();
  renderModules();
  renderBirthdays();
}

function fillPageForm() {
  const form = document.querySelector('#page-form');
  setValue(form, 'title', state.page.title);
  setValue(form, 'welcomeText', state.page.welcomeText);
  setValue(form, 'logoUrl', state.page.logoUrl || '');
  setValue(form, 'backgroundType', state.page.backgroundType);
  setValue(form, 'backgroundValue', state.page.backgroundValue || '');
  form.elements.isActive.checked = state.page.isActive;
}

function renderGroups() {
  const container = document.querySelector('#groups-list');
  container.replaceChildren(...state.groups.map(group => item(group.title, `${group.icon || '🔗'} Порядок: ${group.sortOrder} · ${group.isVisible ? 'видима' : 'скрыта'}`, [
    action('Изменить', () => editGroup(group)),
    action('Удалить', () => remove(`/api/link-groups/${group.id}`), 'danger')
  ])));
}

function renderGroupSelect() {
  const select = document.querySelector('#link-form').elements.linkGroupId;
  select.replaceChildren(...state.groups.map(group => {
    const option = document.createElement('option');
    option.value = group.id;
    option.textContent = group.title;
    return option;
  }));
}

function renderLinks() {
  const container = document.querySelector('#links-list');
  container.replaceChildren(...state.links.map(link => item(link.title, `${link.groupTitle} · ${link.url} · ${link.isActive ? 'активна' : 'выключена'}`, [
    action('Изменить', () => editLink(link)),
    action('Удалить', () => remove(`/api/portal-links/${link.id}`), 'danger')
  ])));
}

function renderModules() {
  const container = document.querySelector('#modules-list');
  container.replaceChildren(...state.modules.map(module => item(module.title, `${module.type} · Порядок: ${module.sortOrder} · ${module.isVisible ? 'видим' : 'скрыт'}`, [
    action('Изменить', () => editModule(module)),
    action('Удалить', () => remove(`/api/portal-modules/${module.id}`), 'danger')
  ])));
}

function renderBirthdays() {
  const container = document.querySelector('#birthdays-list');
  container.replaceChildren(...state.birthdays.map(employee => {
    const date = new Date(employee.birthDate).toLocaleDateString('ru-RU');
    return item(employee.fullName, `${date} · ${employee.department || 'без подразделения'} · ${employee.isActive ? 'активен' : 'скрыт'}`, [
      action('Изменить', () => editBirthday(employee)),
      action('Удалить', () => remove(`/api/employee-birthdays/${employee.id}`), 'danger')
    ]);
  }));
}

async function submitPage(event) {
  event.preventDefault();
  const form = event.currentTarget;
  await send(`/api/portal-pages/${state.page.id}`, 'PUT', {
    title: value(form, 'title'),
    welcomeText: value(form, 'welcomeText'),
    logoUrl: nullable(value(form, 'logoUrl')),
    backgroundType: value(form, 'backgroundType') || 'decorative',
    backgroundValue: nullable(value(form, 'backgroundValue')),
    isActive: form.elements.isActive.checked
  });
}

async function submitGroup(event) {
  event.preventDefault();
  const form = event.currentTarget;
  const id = value(form, 'id');
  await send(id ? `/api/link-groups/${id}` : '/api/link-groups', id ? 'PUT' : 'POST', {
    portalPageId: state.page.id,
    title: value(form, 'title'),
    description: nullable(value(form, 'description')),
    icon: nullable(value(form, 'icon')),
    sortOrder: numberValue(form, 'sortOrder'),
    isVisible: form.elements.isVisible.checked
  });
  resetForm('group-form');
}

async function submitLink(event) {
  event.preventDefault();
  const form = event.currentTarget;
  const id = value(form, 'id');
  await send(id ? `/api/portal-links/${id}` : '/api/portal-links', id ? 'PUT' : 'POST', {
    linkGroupId: numberValue(form, 'linkGroupId'),
    title: value(form, 'title'),
    url: value(form, 'url'),
    description: nullable(value(form, 'description')),
    icon: nullable(value(form, 'icon')),
    sortOrder: numberValue(form, 'sortOrder'),
    isActive: form.elements.isActive.checked,
    openInNewTab: form.elements.openInNewTab.checked
  });
  resetForm('link-form');
}

async function submitModule(event) {
  event.preventDefault();
  const form = event.currentTarget;
  const id = value(form, 'id');
  await send(id ? `/api/portal-modules/${id}` : '/api/portal-modules', id ? 'PUT' : 'POST', {
    portalPageId: state.page.id,
    type: value(form, 'type'),
    title: value(form, 'title'),
    content: nullable(value(form, 'content')),
    settingsJson: nullable(value(form, 'settingsJson')),
    sortOrder: numberValue(form, 'sortOrder'),
    isVisible: form.elements.isVisible.checked
  });
  resetForm('module-form');
}

async function submitBirthday(event) {
  event.preventDefault();
  const form = event.currentTarget;
  const id = value(form, 'id');
  await send(id ? `/api/employee-birthdays/${id}` : '/api/employee-birthdays', id ? 'PUT' : 'POST', {
    fullName: value(form, 'fullName'),
    birthDate: value(form, 'birthDate'),
    department: nullable(value(form, 'department')),
    isActive: form.elements.isActive.checked
  });
  resetForm('birthday-form');
}

function editGroup(group) {
  const form = document.querySelector('#group-form');
  setValue(form, 'id', group.id);
  setValue(form, 'title', group.title);
  setValue(form, 'description', group.description || '');
  setValue(form, 'icon', group.icon || '');
  setValue(form, 'sortOrder', group.sortOrder);
  form.elements.isVisible.checked = group.isVisible;
  form.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function editLink(link) {
  const form = document.querySelector('#link-form');
  setValue(form, 'id', link.id);
  setValue(form, 'linkGroupId', link.linkGroupId);
  setValue(form, 'title', link.title);
  setValue(form, 'url', link.url);
  setValue(form, 'description', link.description || '');
  setValue(form, 'icon', link.icon || '');
  setValue(form, 'sortOrder', link.sortOrder);
  form.elements.isActive.checked = link.isActive;
  form.elements.openInNewTab.checked = link.openInNewTab;
  form.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function editModule(module) {
  const form = document.querySelector('#module-form');
  setValue(form, 'id', module.id);
  setValue(form, 'type', module.type);
  setValue(form, 'title', module.title);
  setValue(form, 'content', module.content || '');
  setValue(form, 'settingsJson', module.settingsJson || '');
  setValue(form, 'sortOrder', module.sortOrder);
  form.elements.isVisible.checked = module.isVisible;
  form.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function editBirthday(employee) {
  const form = document.querySelector('#birthday-form');
  setValue(form, 'id', employee.id);
  setValue(form, 'fullName', employee.fullName);
  setValue(form, 'birthDate', employee.birthDate);
  setValue(form, 'department', employee.department || '');
  form.elements.isActive.checked = employee.isActive;
  form.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

async function send(url, method, body) {
  const response = await fetch(url, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  if (!response.ok) throw new Error(await response.text());
  showToast('Сохранено');
  await reload();
  renderAll();
}

async function remove(url) {
  if (!confirm('Удалить запись?')) return;
  const response = await fetch(url, { method: 'DELETE' });
  if (!response.ok) throw new Error(await response.text());
  showToast('Удалено');
  await reload();
  renderAll();
}

function item(title, meta, actions) {
  const element = document.createElement('article');
  element.className = 'admin-item';
  const strong = document.createElement('strong');
  strong.textContent = title;
  const small = document.createElement('small');
  small.textContent = meta;
  const actionBox = document.createElement('div');
  actionBox.className = 'admin-actions';
  actionBox.append(...actions);
  element.append(strong, small, actionBox);
  return element;
}

function action(label, handler, className = 'secondary') {
  const button = document.createElement('button');
  button.type = 'button';
  button.className = className;
  button.textContent = label;
  button.addEventListener('click', handler);
  return button;
}

function resetForm(id) {
  const form = document.querySelector(`#${id}`);
  form.reset();
  if (form.elements.id) form.elements.id.value = '';
  form.querySelectorAll('input[type="checkbox"]').forEach(input => input.checked = true);
}

function value(form, name) { return form.elements[name].value.trim(); }
function setValue(form, name, val) { form.elements[name].value = val ?? ''; }
function numberValue(form, name) { return Number(value(form, name) || 0); }
function nullable(val) { return val ? val : null; }
async function fetchJson(url) { const response = await fetch(url); if (!response.ok) throw new Error(await response.text()); return response.json(); }
function showToast(message) { toast.textContent = message; toast.classList.add('show'); setTimeout(() => toast.classList.remove('show'), 2500); }
function showError(error) { console.error(error); showToast(`Ошибка: ${error.message}`); }
