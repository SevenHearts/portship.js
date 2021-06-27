import { promises as fsp } from 'fs';
import path from 'path';

import { compileFormat } from './formats.mjs';

if (process.argv.length !== 5)
	throw new Error(`expected 3 arguments, got ${process.argv.length - 2}`);

const files = await compileFormat(process.argv[2], process.argv[4]);

const promises = [];

for (const [name, content] of Object.entries(files)) {
	const fullPath = path.join(process.argv[3], name);
	promises.push(fsp.writeFile(fullPath, content));
}

await Promise.all(promises);
