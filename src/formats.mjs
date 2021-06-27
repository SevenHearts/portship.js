import { promises as fsp } from 'fs';
import { URL } from 'url';
import path from 'path';

import arg from 'arg';
import YAML from 'yaml';
import KaitaiStructCompiler from 'kaitai-struct-compiler';
import { KaitaiStream } from 'kaitai-struct';

const kaitaiCompiler = new KaitaiStructCompiler();

//console.log(kaitaiCompiler.languages.join(", "));

export async function compileFormat(filepath, language) {
	const contents = await fsp.readFile(filepath, 'utf-8');
	const spec = YAML.parse(contents);
	return kaitaiCompiler.compile(language, spec, null, false);
}

export async function importFormat(name) {
	function kaitaiRequire(what) {
		switch (what) {
			case 'kaitai-struct/KaitaiStream':
				return KaitaiStream;
			default:
				throw new Error(
					`kaitai struct class attempted to require something unexpected: ${what}`
				);
		}
	}

	const filepath = new URL(
		path.join('./format', name + '.ksy'),
		import.meta.url
	);

	const files = await compileFormat(filepath, 'javascript');

	// This is a bit of a hack. But it works.
	if (Object.keys(files).length !== 1) {
		// Should never happen.
		throw new Error('kaitai compiler produced too many/too little files!');
	}

	const parserModule = { exports: {} };

	new Function('require', 'module', files[Object.keys(files)[0]])(
		kaitaiRequire,
		parserModule
	);

	return buf => new parserModule.exports(new KaitaiStream(buf));
}
