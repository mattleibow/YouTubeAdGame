// ── localStorage helpers ───────────────────────────────────────────────────
window.gameStorage = {
    _prefix: 'crowdrunner_',

    save(key, value) {
        localStorage.setItem(this._prefix + key, value);
    },

    load(key) {
        return localStorage.getItem(this._prefix + key);
    },

    remove(key) {
        localStorage.removeItem(this._prefix + key);
    },

    /** Return all map names stored under crowdrunner_map_* */
    listMapNames() {
        const mapPrefix = this._prefix + 'map_';
        return Object.keys(localStorage)
            .filter(k => k.startsWith(mapPrefix))
            .map(k => k.substring(mapPrefix.length));
    },

    saveMap(name, json) {
        localStorage.setItem(this._prefix + 'map_' + name, json);
    },

    loadMap(name) {
        return localStorage.getItem(this._prefix + 'map_' + name);
    },

    removeMap(name) {
        localStorage.removeItem(this._prefix + 'map_' + name);
    },

    /** Active custom map (the one selected for 'Custom' mode) */
    saveActiveMap(json) {
        localStorage.setItem(this._prefix + 'active_custom', json);
    },

    loadActiveMap() {
        return localStorage.getItem(this._prefix + 'active_custom');
    }
};

// ── JSON download helper ───────────────────────────────────────────────────
window.downloadJson = (filename, json) => {
    const blob = new Blob([json], { type: 'application/json' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href     = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
