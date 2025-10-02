# Commit Message Hooks

Ensure commit messages follow the required format by setting up Git hooks with Husky.

## Prerequisites
- Node.js LTS (includes npm)
	- Verify:
	  ```bash
	  node -v
	  npm -v
	  ```
- Allow script execution if needed:
	- macOS/Linux:
	  ```bash
	  chmod +x ./setup-husky.sh
	  ```
	- Windows (if script execution is blocked):
	  ```powershell
	  Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
	  ```

## Setup (run once after cloning)
- macOS/Linux:
  ```bash
  bash ./setup-husky.sh
  ```
- Windows (PowerShell):
  ```powershell
  pwsh ./setup-husky.sh
  ```

After setup, commit messages will be validated automatically on commit.

## Debug into commitlint.config.js ?

```bash
	npx commitlint --edit .git/COMMIT_EDITMSG
```
or
```bash
	npm run debug:lint:commit
```
