// commitlint.config.cjs
// Commitlint v20 — a single friendly message via function-rules/header-case

const CONVENTIONAL_TYPES = [
	'feat','fix','chore','docs','style','refactor','test','revert','build','ci',
];
const GITMOJI_TYPES = [
	'🎨','⚡️','🔥','🐛','🚑️','✨','📝','🚀','💄','🎉','✅','🔒️','🔐','🔖','🚨','🚧','💚','⬇️',
	'⬆️','📌','👷','📈','♻️','➕','➖','🔧','🔨','🌐','✏️','💩','⏪️','🔀','📦️','👽️','🚚','📄',
	'💥','🍱','♿️','💡','🍻','💬','🗃️','🔊','🔇','👥','🚸','🏗️','📱','🤡','🥚','🙈','📸','⚗️',
	'🔍️','🏷️','🌱','🚩','🥅','💫','🗑️','🛂','🩹','🧐','⚰️','🧪','👔','🩺','🧱','🧑‍💻','💸','🧵','🦺',
];

// Types that must include a JIRA ticket (prefix/suffix) or allow "WIP":
const JIRA_REQUIRED_TYPES = new Set([
	'fix','feat','refactor','test','🐛','🚑️','✨','💄','🔒️','👽️','💥','🍱','♿️','🚸','🚩','🩹',
]);

const ALL_TYPES = new Set([...CONVENTIONAL_TYPES, ...GITMOJI_TYPES]);
const TYPES_PREVIEW = [...ALL_TYPES].slice(0, 12).join(', ') + ', …';

const ok   = () => [true];
const fail = (m) => [false, m];

function parseHeader(headerRaw) {
	const header = (headerRaw || '').trim();

	// Detect "<type>(optional-scope)!?: <subject>"
	const colonIdx = header.indexOf(':');
	const left = colonIdx >= 0 ? header.slice(0, colonIdx).trim() : header;
	const subject = colonIdx >= 0 ? header.slice(colonIdx + 1).trim() : '';

	// Extract type (everything up to first "(" or "!" if present)
	const m = left.match(/^([^(!:\s]+)(?:\([^)]+\))?!?$/); // captures "type"
	const type = m ? m[1] : left.split(/\s+/)[0] || '';

	// JIRA presence: "ABC-123" prefix OR "#ABC-123" suffix OR "WIP:" subject
	const hasJiraPrefix = /^[A-Z]+-\d+\s+/.test(header);
	const hasJiraSuffix = /#[A-Z]+-\d+\b/.test(header);
	const isWip         = /^WIP:\s*/.test(subject);

	return { header, type, subject, colonIdx, hasJiraPrefix, hasJiraSuffix, isWip };
}

// Single consolidated rule bound to a supported name
function headerFriendlyRule(parsed) {
	const { header } = parsed || {};
	const H = parseHeader(header);

	const problems = [];

	if (!H.type) {
		problems.push('• Missing **type** at the beginning.');
	} else if (!ALL_TYPES.has(H.type)) {
		problems.push(`• Unknown **type** "${H.type}".`);
	}

	if (H.colonIdx < 0) problems.push('• Missing **":"** after the type (e.g. `feat: …`).');
	if (!H.subject)     problems.push('• Missing **subject** after ":".');

	const needsJira = JIRA_REQUIRED_TYPES.has(H.type);
	const hasJira   = H.hasJiraPrefix || H.hasJiraSuffix || H.isWip;
	if (needsJira && !hasJira) {
		problems.push('• This type requires a **JIRA ticket** (e.g. `ABC-123 fix: …` or `fix: … #ABC-123`) or use **`WIP:`**.');
	}

	if (problems.length === 0) return ok();

	const message =
		`
			✖ Your commit message does not match the required format.

			What you wrote:
			  "${H.header || '(empty)'}"

			Expected:
			  <type>(optional-scope)!?: <subject>

			Quick examples:
			  feat: add login page
			  fix: avoid NRE on startup #BIZ-123
			  🐛 fix crash when token is null
			  BIZ-123 fix: correct rounding logic
			  WIP: refactor auth flow

			Why it failed:
			  ${problems.join('\n  ')}

			Allowed types (partial):
			  ${TYPES_PREVIEW}
		`;

	return fail(message);
}

module.exports = {
	extends: ['@commitlint/config-conventional'],
	plugins: ['commitlint-plugin-function-rules'],
	rules: {
		// Disable stock rules to avoid duplicates
		'type-enum': [0],
		'type-empty': [0],
		'subject-empty': [0],
		'subject-case': [0],

		// Use a supported function rule name
		'function-rules/header-case': [2, 'always', headerFriendlyRule],
	},
};
