const groupTemplate = document.querySelector('#group-template');
const moduleTemplate = document.querySelector('#module-template');

loadPortal().catch(error => {
  document.querySelector('#portal-welcome').textContent = `Не удалось загрузить портал: ${error.message}`;
});

async function loadPortal() {
  const [page, todayBirthdays, upcomingBirthdays] = await Promise.all([
    fetchJson('/api/portal-pages/active'),
    fetchJson('/api/employee-birthdays/today'),
    fetchJson('/api/employee-birthdays/upcoming?days=30')
  ]);

  document.title = page.title;
  document.querySelector('#portal-title').textContent = page.title;
  document.querySelector('#portal-welcome').textContent = page.welcomeText;
  if (page.logoUrl) {
    document.querySelector('#portal-logo').src = page.logoUrl;
  }

  renderGroups(page.linkGroups.filter(group => group.isVisible));
  renderModules(page.modules.filter(module => module.isVisible), todayBirthdays, upcomingBirthdays);
}

function renderGroups(groups) {
  const container = document.querySelector('#link-groups');
  container.replaceChildren(...groups.map(group => {
    const fragment = groupTemplate.content.cloneNode(true);
    fragment.querySelector('.group-icon').textContent = group.icon || '🔗';
    fragment.querySelector('.group-title').textContent = group.title;
    const description = fragment.querySelector('.group-description');
    description.textContent = group.description || '';
    description.hidden = !group.description;
    const list = fragment.querySelector('ul');
    group.links.filter(link => link.isActive).forEach(link => {
      const item = document.createElement('li');
      const anchor = document.createElement('a');
      anchor.href = link.url;
      anchor.target = link.openInNewTab ? '_blank' : '_self';
      anchor.rel = link.openInNewTab ? 'noopener noreferrer' : '';
      const icon = document.createElement('span');
      icon.className = 'link-icon';
      icon.textContent = link.icon || '↔';
      const title = document.createElement('span');
      title.textContent = link.title;
      anchor.append(icon, title);
      item.append(anchor);
      list.append(item);
    });
    return fragment;
  }));
}

function renderModules(modules, todayBirthdays, upcomingBirthdays) {
  const container = document.querySelector('#modules');
  container.replaceChildren(...modules.map(module => {
    const fragment = moduleTemplate.content.cloneNode(true);
    fragment.querySelector('h3').textContent = module.title;
    const content = fragment.querySelector('.module-content');

    if (module.type === 'Birthdays') {
      content.append(renderBirthdays(todayBirthdays, upcomingBirthdays));
    } else if (module.type === 'Weather') {
      content.append(renderWeather(module));
    } else if (module.type === 'Html') {
      content.innerHTML = module.content || '';
    } else {
      content.textContent = module.content || '';
    }

    return fragment;
  }));
}

function renderBirthdays(today, upcoming) {
  const wrapper = document.createElement('div');
  wrapper.className = 'birthday-list';
  wrapper.append(section('Сегодня:', today), section('Ближайшие именинники:', upcoming));
  return wrapper;
}

function section(title, employees) {
  const block = document.createElement('div');
  const heading = document.createElement('strong');
  heading.textContent = title;
  block.append(heading);
  if (!employees.length) {
    const empty = document.createElement('p');
    empty.textContent = 'Нет записей';
    block.append(empty);
    return block;
  }
  employees.forEach(employee => {
    const item = document.createElement('p');
    const date = new Date(employee.birthDate).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' });
    item.textContent = `${employee.fullName} — ${date}`;
    block.append(item);
  });
  return block;
}

function renderWeather(module) {
  let settings = {};
  try {
    settings = module.settingsJson ? JSON.parse(module.settingsJson) : {};
  } catch {
    settings = {};
  }

  const wrapper = document.createElement('div');
  wrapper.className = 'weather-widget';
  wrapper.innerHTML = `<span class="weather-icon">⛅</span><div><strong>${settings.temperature || '+14°'}</strong><br><span>${settings.city || 'Ваш город'}</span><br><small>${settings.summary || module.content || 'Погода'}</small></div>`;
  return wrapper;
}

async function fetchJson(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}`);
  }
  return response.json();
}
