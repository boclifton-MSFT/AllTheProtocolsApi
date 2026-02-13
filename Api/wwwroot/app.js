/* ═══════════════════════════════════════════════════════════
   AllTheProtocols Weather — Minimal JS
   Only data fetching & DOM population. All visuals are CSS.
   ═══════════════════════════════════════════════════════════ */

const API = 'http://localhost:5249/api/weather';
const GQL = 'http://localhost:5249/graphql';

let currentStationId = null;

const skyEmoji = {
  Clear: '☀️', PartlyCloudy: '⛅', MostlyCloudy: '🌥️',
  Overcast: '☁️', Foggy: '🌫️', Hazy: '🌫️'
};

const fmt = s => s.replace(/([A-Z])/g, ' $1').trim();
const get = url => fetch(url).then(r => r.ok ? r.json() : null);
const gql = query => fetch(GQL, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ query })
}).then(r => r.json());

// ── Bootstrap ────────────────────────────────────────────
async function init() {
  const stations = await get(`${API}/stations`);
  if (!stations) return;

  document.getElementById('stations').innerHTML = stations.map(s =>
    `<button class="station-card" data-id="${s.id}" onclick="selectStation('${s.id}')">
       <span class="station-city">${s.location.city}</span>
       <span class="station-state">${s.location.state}</span>
       <span class="station-status status-${s.status.toLowerCase()}">${s.status}</span>
     </button>`
  ).join('');
}

// ── Station selection ────────────────────────────────────
async function selectStation(id) {
  const dash = document.getElementById('dashboard');

  document.querySelectorAll('.station-card')
    .forEach(c => c.classList.toggle('active', c.dataset.id === id));

  dash.classList.remove('empty');
  dash.classList.add('loading');

  currentStationId = id;
  // Reset unit toggle to °F when switching stations
  const toggle = document.getElementById('unit-toggle');
  if (toggle) toggle.checked = false;
  document.getElementById('temp-unit').textContent = '°F';

  const [conditions, daily, hourly, aq, alerts] = await Promise.all([
    get(`${API}/stations/${id}/conditions`),
    get(`${API}/stations/${id}/forecast/daily`),
    get(`${API}/stations/${id}/forecast/hourly`),
    get(`${API}/stations/${id}/air-quality`),
    get(`${API}/alerts?stationId=${id}`),
  ]);

  renderConditions(conditions);
  renderDaily(daily);
  renderHourly(hourly);
  renderAirQuality(aq);
  renderAlerts(alerts);

  updateQueryPreview();
  dash.classList.remove('loading');
}

// ── Renderers ────────────────────────────────────────────

function renderConditions(c) {
  if (!c) return;
  const icon = (c.precipitation.type !== 'None' ? '🌧️' : null) || skyEmoji[c.skyCondition] || '🌡️';
  const el = id => document.getElementById(id);
  const isCelsius = document.getElementById('unit-toggle')?.checked;
  const unit = isCelsius ? '°C' : '°F';

  el('weather-icon').textContent = icon;
  el('weather-icon').dataset.condition = c.skyCondition;
  el('temp-value').textContent = Math.round(c.temperatureF);
  el('temp-unit').textContent = unit;
  el('condition-text').textContent = fmt(c.skyCondition);
  el('feels-like').textContent = `Feels like ${Math.round(c.feelsLikeF)}${unit}`;
  el('humidity').textContent = `${c.humidityPercent}%`;
  el('wind').textContent =
    `${c.wind.speedMph} mph ${c.wind.direction}` +
    (c.wind.gustsMph ? ` (gusts ${c.wind.gustsMph})` : '');
  el('pressure').textContent = `${c.pressureMb} mb`;
  el('uv').textContent = c.uvIndex;
  el('visibility').textContent = `${c.visibilityMiles} mi`;
  el('dewpoint').textContent = `${Math.round(c.dewPointF)}°F`;
  el('precip').textContent = c.precipitation.type !== 'None'
    ? `${c.precipitation.probabilityPercent}% ${fmt(c.precipitation.type)}`
    : 'None';
}

function renderDaily(days) {
  if (!days?.length) return;
  const min = -10, range = 130; // -10 to 120

  document.getElementById('daily-list').innerHTML = days.map(d => {
    const lo = Math.round(d.lowTempF), hi = Math.round(d.highTempF);
    const left = ((lo - min) / range * 100).toFixed(1);
    const width = ((hi - lo) / range * 100).toFixed(1);
    const date = new Date(d.date + 'T00:00:00');
    const name = date.toLocaleDateString('en-US', { weekday: 'short' });
    return `<div class="day-row">
      <span class="day-name">${name}</span>
      <span class="day-icon">${skyEmoji[d.skyCondition] || '🌡️'}</span>
      <span class="day-low">${lo}°</span>
      <div class="temp-bar-track">
        <div class="temp-bar-fill" style="left:${left}%;width:${width}%"></div>
      </div>
      <span class="day-high">${hi}°</span>
      <span class="day-precip">${d.precipitation.probabilityPercent}%</span>
    </div>`;
  }).join('');
}

function renderHourly(hours) {
  if (!hours?.length) return;

  document.getElementById('hourly-list').innerHTML = hours.map((h, i) => {
    const time = new Date(h.dateTime).toLocaleTimeString('en-US', { hour: 'numeric' });
    return `<div class="hour-card" style="--i:${i}">
      <span class="hour-time">${time}</span>
      <span class="hour-icon">${skyEmoji[h.skyCondition] || '🌡️'}</span>
      <span class="hour-temp">${Math.round(h.temperatureF)}°</span>
      <span class="hour-wind">${h.wind.speedMph} mph</span>
      <span class="hour-precip">${h.precipitation.probabilityPercent}%</span>
    </div>`;
  }).join('');
}

function renderAirQuality(aq) {
  if (!aq) return;
  const rotation = (aq.aqi / 500 * 180);
  document.getElementById('aqi-gauge').style.setProperty('--rotation', `${rotation}deg`);
  document.getElementById('aqi-value').textContent = aq.aqi;

  const cat = document.getElementById('aqi-category');
  cat.textContent = fmt(aq.category);
  cat.dataset.category = aq.category;

  document.getElementById('aqi-pollutant').textContent = aq.primaryPollutant;

  document.getElementById('pollutants').innerHTML =
    Object.entries(aq.pollutants).map(([k, v]) =>
      `<div class="pollutant-card">
         <span class="pollutant-name">${k}</span>
         <span class="pollutant-value">${v}</span>
       </div>`
    ).join('');
}

function renderAlerts(alerts) {
  const label = document.querySelector('label[for="tab-alerts"]');
  label.dataset.count = alerts?.length || 0;

  const list = document.getElementById('alerts-list');
  if (!alerts?.length) {
    list.innerHTML =
      `<div class="no-alerts"><span class="no-alerts-icon">✓</span>No active alerts for this station.</div>`;
    return;
  }

  list.innerHTML = alerts.map(a =>
    `<div class="alert-card severity-${a.severity.toLowerCase()}">
       <div class="alert-header">
         <span class="alert-severity">${a.severity}</span>
         <span class="alert-category">${fmt(a.category)}</span>
       </div>
       <h3 class="alert-title">${a.title}</h3>
       <p class="alert-description">${a.description}</p>
       <div class="alert-meta">
         <span>Affects: ${a.affectedArea}</span>
         <span>Expires: ${new Date(a.expiresAt).toLocaleString()}</span>
       </div>
     </div>`
  ).join('');
}

// ── Go ───────────────────────────────────────────────────
init();

// ── °C/°F Toggle (GraphQL-only feature) ──────────────────
async function toggleUnit() {
  if (!currentStationId) return;
  const celsius = document.getElementById('unit-toggle').checked;
  const unit = celsius ? 'CELSIUS' : 'FAHRENHEIT';
  const query = `{ currentConditions(stationId: "${currentStationId}", unit: ${unit}) {
    observedAt temperatureF feelsLikeF humidityPercent dewPointF
    pressureMb visibilityMiles uvIndex skyCondition
    wind { speedMph direction gustsMph }
    precipitation { type amountInches probabilityPercent }
  }}`;
  const result = await gql(query);
  if (result?.data?.currentConditions) {
    renderConditions(result.data.currentConditions);
  }
}

// ── GraphQL Explorer ─────────────────────────────────────
function buildExplorerQuery() {
  const id = currentStationId || 'den01';
  const boxes = document.querySelectorAll('#panel-explorer .field-check input[data-field]');
  const useCelsius = document.getElementById('explorer-celsius')?.checked;

  const topFields = [];
  const windFields = [];
  const precipFields = [];

  boxes.forEach(cb => {
    if (!cb.checked) return;
    const f = cb.dataset.field;
    if (f.startsWith('wind.'))          windFields.push(f.split('.')[1]);
    else if (f.startsWith('precipitation.')) precipFields.push(f.split('.')[1]);
    else                                 topFields.push(f);
  });

  let fields = topFields.join(' ');
  if (windFields.length)  fields += ` wind { ${windFields.join(' ')} }`;
  if (precipFields.length) fields += ` precipitation { ${precipFields.join(' ')} }`;

  const unitArg = useCelsius ? ', unit: CELSIUS' : '';
  return `{\n  currentConditions(stationId: "${id}"${unitArg}) {\n    ${fields}\n  }\n}`;
}

function updateQueryPreview() {
  const pre = document.getElementById('gql-query');
  if (!pre) return;
  pre.textContent = buildExplorerQuery();
}

async function runExplorerQuery() {
  const query = buildExplorerQuery();
  const resultsEl = document.getElementById('gql-results');
  resultsEl.textContent = '// Loading...';
  try {
    const result = await gql(query);
    resultsEl.textContent = JSON.stringify(result, null, 2);
  } catch (e) {
    resultsEl.textContent = `// Error: ${e.message}`;
  }
}
