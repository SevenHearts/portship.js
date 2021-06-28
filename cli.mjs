#!/usr/bin/env node

import { promises as fsp } from 'fs';
import { URL, fileURLToPath } from 'url';
import path from 'path';

import arg from 'arg';

import { importFormat } from './src/formats.mjs';

const args = arg({
	'--help': Boolean,

	'--out-dir': String,
	'-o': '--out-dir',

	'--cc': String,
	'--cxx': String
});

if (
	process.argv.length === 2 ||
	args['--help'] ||
	args._.length !== 1 ||
	!args['--out-dir']
) {
	console.error(`
  Portship - converts ROSE Online assets for use in Unity

  USAGE
     portship --help
     portship -o ./out/ /path/to/data.idx

  OPTIONS
     --help                  shows this help message

     --out-dir, -o path      output directory

     --cc path               path to C compiler
                             (default: cc)

     --cxx path              path to C++ compiler
                             (default: c++)
`);
	process.exit(2);
}

const parseIDX = await importFormat('IDX');

class Archive {
	constructor({ filepath }) {
		this.filepath = filepath;
	}
}

class VFSDirectory {
	constructor({ parent, name }) {
		this.name = name;
		this.parent = parent;
		this.leafs = new Map();
	}

	set(leaf, node) {
		this.leafs.set(leaf, node);
	}

	has(leaf) {
		return this.leafs.has(leaf);
	}

	get(leaf) {
		return this.leafs.get(leaf, null);
	}

	read() {
		return [...this.leafs.keys()];
	}

	[Symbol.iterator]() {
		return this.leafs.values();
	}

	isFile() {
		return false;
	}
	isDir() {
		return true;
	}
}

class VFSFile {
	constructor({ offset, length, archive, parent, name }) {
		this.parent = parent;
		this.offset = offset;
		this.length = length;
		this.archive = archive;
		this.name = name;

		this.filepathLeafs = [name];

		let cur = this.parent;
		while (cur && cur.name) {
			this.filepathLeafs.unshift(cur.name);
			cur = cur.parent;
		}

		this.filepath = this.filepathLeafs.join('/');
	}

	get ext() {
		return path.extname(this.name);
	}

	asExt(newExt) {
		return path.join(
			path.dirname(this.filepath),
			path.basename(this.filepath, this.ext) + newExt
		);
	}

	isFile() {
		return true;
	}
	isDir() {
		return false;
	}
}

export default class VFS {
	constructor({ idx, rootDir }) {
		this.root = new VFSDirectory({ name: null, parent: null });

		for (const meta of idx.vfsMeta) {
			// Skip over the pseudo-path ROOT.VFS.
			if (meta.path.data.toLowerCase() === 'root.vfs') continue;

			const archive = new Archive({
				filepath: path.join(rootDir, meta.path.data)
			});

			for (const file of meta.index.files) {
				if (file.deleted) continue;

				if (file.compressionType !== 0) {
					console.warn(
						'warning: unexpected compression type (!= 0):',
						file.path.data
					);
					continue;
				}

				if (file.encryptionType !== 0) {
					console.warn(
						'warning: unexpected compression type (!= 0):',
						file.path.data
					);
					continue;
				}

				this.addFile(file.path.data, archive, file.offset, file.length);
			}
		}
	}

	addFile(filepath, archive, offset, length) {
		const leafs = filepath.toLowerCase().split(/[/\\]+/g);

		const key = leafs.pop();

		let cur = this.root;
		for (const leaf of leafs) {
			let next = cur.get(leaf, null);

			if (next) {
				if (!(next instanceof VFSDirectory)) {
					throw new Error(
						`parent path exists and is not a directory: ${filepath} (failed at: ${leaf})`
					);
				}
			} else {
				next = new VFSDirectory({ name: leaf, parent: cur });
				cur.set(leaf, next);
			}

			cur = next;
		}

		if (cur.has(key)) {
			throw new Error(`path already exists: ${filepath}`);
		}

		cur.set(
			key,
			new VFSFile({
				archive,
				offset,
				length,
				parent: cur,
				name: key
			})
		);
	}

	resolve(path, anyType = false) {
		if (Array.isArray(path)) {
			path = path.map(l => l.toLowerCase());
		} else {
			path = path.toLowerCase().split(/[\\/]+/g);
		}

		const name = path.pop();

		let cur = this.root;
		for (const leaf of path) {
			const next = cur.get(leaf);

			if (!next) {
				throw new Error(
					`not found: ${path.join('/')} (failed on '${leaf}')`
				);
			}

			if (!next.isDir()) {
				throw new Error(
					`not found (parent is file): ${path.join(
						'/'
					)} (failed on '${leaf}')`
				);
			}

			cur = next;
		}

		const result = cur.get(name);
		if (!result) {
			throw new Error(
				`not found (not in directory): ${path.join(
					'/'
				)} (failed on '${name}')`
			);
		}
		if (!anyType && !result.isFile()) {
			throw new Error(
				`not found (not a file): ${path.join(
					'/'
				)} (failed on '${name}')`
			);
		}

		return result;
	}

	*walkAll(root) {
		root ??= this.root;

		for (const ent of root) {
			if (ent.isFile()) yield ent;
			else yield* this.walkAll(ent);
		}
	}

	*walk(pattern) {
		for (const file of this.walkAll()) {
			if (file.name.match(pattern)) yield file;
		}
	}

	static async fromIDX(idxPath) {
		const contents = await fsp.readFile(idxPath);

		return new VFS({
			idx: await parseIDX(contents),
			rootDir: path.dirname(idxPath)
		});
	}
}

class Ninjafile {
	static RULE = Symbol();

	constructor() {
		this.var = new Map();
		this.rule = new Map();
		this.build = [];
	}

	static transformVal(val) {
		if (!Array.isArray(val)) val = [val];

		function* genAll() {
			for (const v of val) {
				if (typeof v === 'object' && Ninjafile.RULE in v) {
					yield* v[Ninjafile.RULE];
				} else {
					yield v;
				}
			}
		}

		return [...genAll()];
	}

	*generate(outDir, idxPath) {
		if (!path.isAbsolute(outDir))
			throw new Error(`outDir must be absolute: ${outDir}`);
		if (!path.isAbsolute(idxPath))
			throw new Error(`idxPath must be absolute: ${idxPath}`);

		const format = (strings, args, replacer) => {
			const chunks = [strings[0]];

			for (let i = 1, len = strings.length; i < len; i++) {
				let arg = args[i - 1];

				if (!Array.isArray(arg)) arg = [arg];

				function* xformAll() {
					let delim = false;
					for (const a of arg) {
						if (delim) yield ' ';
						delim = true;
						yield replacer(a);
					}
				}

				chunks.push(...xformAll(), strings[i]);
			}

			return chunks.join('');
		};

		const esc = (strings, ...args) =>
			format(strings, args, l => l.toString().replace(/ /g, '$ '));
		const cmd = (strings, ...args) =>
			format(strings, args, l =>
				typeof l === 'symbol'
					? `$${l.description}`
					: `'${l.toString().replace(/'/g, `\\'`)}'`
			);

		yield `# THIS FILE IS AUTOMATICALLY GENERATED BY PORTSHIP!\n`;
		yield `# DO NOT EDIT MANUALLY!\n`;
		yield `\n`;

		for (const [name, val] of this.var.entries()) {
			yield `${esc`${name}`} = ${cmd`${val}`}\n`;
		}

		if (this.var.size > 0) yield ``;

		yield `rule generate\n`;
		yield cmd`  command = cd ${process.cwd()} && ${
			process.execPath
		} ${fileURLToPath(import.meta.url)} -o $outdir $idxfile\n`;
		yield `  generator = 1\n`;
		yield `  description = Re-generating Portship configuration: $idxfile\n`;
		yield `\n`;
		yield esc`build build.ninja: generate | ${idxPath} ${fileURLToPath(
			import.meta.url
		)}\n`;
		yield cmd`  outdir = ${outDir}\n`;
		yield cmd`  idxfile = ${idxPath}\n`;
		yield `\n`;

		for (const [name, desc] of this.rule.entries()) {
			if (!desc.has('command'))
				throw new Error(`rule is missing 'command': ${name}`);

			yield esc`rule ${name}\n`;
			yield cmd`  command = ${desc.get('command')}\n`;

			const opts = desc.get('options', null);

			if (opts)
				for (const [name, value] of opts.entries()) {
					yield `  ${esc`${name}`} = ${
						name === 'description' ? value : cmd`${value}`
					}\n`;
				}

			yield `\n`;
		}

		for (const [rule, opts, impdefs] of this.build) {
			const impdefline = impdefs.length === 0 ? '' : esc` | ${impdefs}`;
			yield esc`build ${opts.get('out')}: ${rule} ${opts.get('in')}` +
				impdefline +
				'\n';

			for (const [name, value] of opts.entries()) {
				if (name !== 'in' && name !== 'out') {
					yield `  ${esc`${name}`} = ${cmd`${value}`}\n`;
				}
			}

			yield `\n`;
		}
	}

	async writeTo(outDir, idxPath) {
		const fd = await fsp.open(path.join(outDir, 'build.ninja'), 'w');
		try {
			for (const line of this.generate(outDir, idxPath)) {
				await fd.write(line);
			}
		} finally {
			await fd.close();
		}
	}

	addRule(name, { command, ...opts }) {
		if (this.rule.has(name)) {
			throw new Error(`rule exists: ${name}`);
		}

		if (!Array.isArray(command) || command.length === 0) {
			throw new Error(`invalid (empty) command (rule '${name}')`);
		}

		const implicitDeps = [];
		const vars = new Set(['in', 'out']);

		function* transformCommand() {
			for (const arg of command) {
				if (typeof arg === 'symbol') {
					vars.add(arg.description);
				} else if (typeof arg === 'object' && Ninjafile.RULE in arg) {
					for (const imdep of arg[Ninjafile.RULE]) {
						if (typeof imdep === 'symbol')
							throw new Error(
								`implicit dependency cannot be symbol: ${imdep}`
							);
						implicitDeps.push(imdep);
						yield imdep;
					}

					continue;
				}

				yield arg;
			}
		}

		command = [...transformCommand()];

		if (vars.size === 0) {
			throw new Error(`command has no variables (symbols): ${command}`);
		}

		const rule = new Map([
			['command', command],
			[
				'options',
				new Map(
					Object.entries(opts).map(([key, val]) => [
						key,
						Ninjafile.transformVal(val)
					])
				)
			]
		]);

		this.rule.set(name, rule);

		const result = ({ ...opts }) => {
			const optMap = new Map(
				Object.entries(opts).map(([key, val]) => [
					key,
					Ninjafile.transformVal(val)
				])
			);

			for (const reqArg of vars) {
				if (!optMap.has(reqArg)) {
					throw new Error(
						`required argument not specified: ${reqArg}`
					);
				}
			}

			this.build.push([name, optMap, implicitDeps]);

			const makeRuleRecord = list => ({
				get [Ninjafile.RULE]() {
					const out = list;
					return Array.isArray(out) ? out : [out];
				},

				filter(filt) {
					return makeRuleRecord(
						typeof filt === 'string'
							? this[Ninjafile.RULE].filter(x => x.endsWith(filt))
							: this[Ninjafile.RULE].filter(x => x.match(filt))
					);
				}
			});

			return makeRuleRecord(optMap.get('out'));
		};

		return result;
	}
}

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const S = (...strs) => path.resolve(path.join(__dirname, String.raw(...strs)));
const O = (...strs) =>
	path.resolve(path.join(args['--out-dir'], String.raw(...strs)));

const vfs = await VFS.fromIDX(args._[0]);
const ninja = new Ninjafile();

const cc = ninja.addRule('cc', {
	command: [
		args['--cc'] || 'cc',
		'-o',
		Symbol('out'),
		'-Wall',
		'-Wextra',
		'-Werror',
		'-Wshadow',
		'-std=c99',
		Symbol('in')
	],
	description: `Compile $out`
});

const cxx = ninja.addRule('cxx', {
	command: [
		args['--cxx'] || 'c++',
		'-o',
		Symbol('out'),
		'-Wall',
		'-Wextra',
		'-Werror',
		'-Wshadow',
		'-std=c++17',
		Symbol('cflags'),
		Symbol('in')
	],
	description: `Compile $out`
});

const copy_ = ninja.addRule('copy', {
	command: ['cp', Symbol('in'), Symbol('out')],
	description: `Copy: $out`
});

const copy = (from, to) => copy_({ in: from, out: to });

const extractExe = cc({
	in: S`./src/extract.c`,
	out: O`./extract`
});

const extract = ninja.addRule('extract', {
	command: [
		extractExe,
		Symbol('in'),
		Symbol('offset'),
		Symbol('length'),
		Symbol('out')
	],
	description: `Extract $out from $in ($offset:$length)`
});

const compileFormat_ = ninja.addRule('compile-format', {
	command: [
		process.execPath,
		S`./src/compile-format.mjs`,
		Symbol('in'),
		Symbol('out_dir'),

		Symbol('lang')
	],
	description: `Compile Kaitai format: $in`
});

const compileFormat = (name, id, format = 'cpp_stl', ...exts) =>
	compileFormat_({
		in: S`./src/format/${name}.ksy`,
		out: exts.map(ext => O`./Assets/Script/ROSE/Format/${id}${ext}`),
		out_dir: O`./Assets/Script/ROSE/Format`,
		lang: format
	});

const extractConvert = ninja.addRule('extract-convert', {
	command: [
		S`./src/extract-convert.sh`,
		extractExe,
		Symbol('in'),
		Symbol('offset'),
		Symbol('length'),
		Symbol('out')
	],
	description: `Extract+DDS->PNG: $out from $in ($offset:$length)`
});

compileFormat('AIP', 'RoseAip', 'csharp', '.cs');
compileFormat('CHR', 'RoseChr', 'csharp', '.cs');
compileFormat('HIM', 'RoseHim', 'csharp', '.cs');
compileFormat('IFO', 'RoseIfo', 'csharp', '.cs');
compileFormat('LIT', 'RoseLit', 'csharp', '.cs');
compileFormat('LOD', 'RoseLod', 'csharp', '.cs');
compileFormat('MOV', 'RoseMov', 'csharp', '.cs');
compileFormat('ZMS', 'RoseZms', 'csharp', '.cs');
compileFormat('ZON', 'RoseZon', 'csharp', '.cs');
compileFormat('ZSC', 'RoseZsc', 'csharp', '.cs');

copy(
	S`./src/kaitai_struct_csharp_runtime/KaitaiStream.cs`,
	O`./Assets/Script/ROSE/KaitaiStream.cs`
);
copy(
	S`./src/kaitai_struct_csharp_runtime/KaitaiStruct.cs`,
	O`./Assets/Script/ROSE/KaitaiStruct.cs`
);

for (const file of vfs.walk(/\.(zms|zsc|dds)$/i)) {
	if (file.filepath.endsWith('.dds')) {
		extractConvert({
			in: file.archive.filepath,
			out: `./Assets/ROSE/${file.asExt('.png')}`,
			length: file.length,
			offset: file.offset
		});
		continue;
	}

	extract({
		in: file.archive.filepath,
		out: O`./Assets/ROSE/${file.filepath}`,
		length: file.length,
		offset: file.offset
	});
}

await ninja.writeTo(O`./`, path.resolve(args._[0]));
