import { promises as fsp } from 'fs';

import getStdin from 'get-stdin';
import camelCase from 'camelcase';

import { importFormat } from './formats.mjs';

if (process.argv.length !== 3) {
	throw new Error('invalid argument count');
}

const contents = await getStdin.buffer();
const loadSTB = await importFormat('STB');

const stb = await loadSTB(contents);

const titles = stb.columnTitles.map(t =>
	camelCase(
		t.data
			.split(':')[0]
			.replace(/[^a-z0-9]+/gi, ' ')
			.trim()
	)
);

const objects = stb.data.rows.map(row =>
	row.cells.reduce((res, v, i) => {
		let data = v.data;
		if (data === '') data = null;
		else if (data.match(/^\d+$/)) data = parseInt(data, 10);
		res[titles[i]] = data;
		return res;
	}, {})
);

await fsp.writeFile(process.argv[2], JSON.stringify({ stb: objects }));
